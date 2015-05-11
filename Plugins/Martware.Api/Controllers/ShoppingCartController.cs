using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Nop.Plugin.Misc.WebApiServices.Models;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.UI.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Misc.WebApiServices.Controllers
{
    public class ShoppingCartController : BaseApiController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly AddressSettings _addressSettings;

        #endregion
        #region Constructors
        public ShoppingCartController(IProductService productService, 
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService, 
            IPictureService pictureService,
            ILocalizationService localizationService, 
            IProductAttributeService productAttributeService, 
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            ITaxService taxService, ICurrencyService currencyService, 
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeFormatter checkoutAttributeFormatter, 
            IOrderProcessingService orderProcessingService,
            IDiscountService discountService,
            ICustomerService customerService, 
            IGiftCardService giftCardService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService, 
            IOrderTotalCalculationService orderTotalCalculationService,
            ICheckoutAttributeService checkoutAttributeService, 
            IPaymentService paymentService,
            IWorkflowMessageService workflowMessageService,
            IPermissionService permissionService, 
            IDownloadService downloadService,
            ICacheManager cacheManager,
            IWebHelper webHelper, 
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings, 
            OrderSettings orderSettings,
            ShippingSettings shippingSettings, 
            TaxSettings taxSettings,
            CaptchaSettings captchaSettings, 
            AddressSettings addressSettings)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._orderProcessingService = orderProcessingService;
            this._discountService = discountService;
            this._customerService = customerService;
            this._giftCardService = giftCardService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._paymentService = paymentService;
            this._workflowMessageService = workflowMessageService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._genericAttributeService = genericAttributeService;
            
            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._captchaSettings = captchaSettings;
            this._addressSettings = addressSettings;
        }

        #endregion

        #region Utilities
        [NonAction]
        protected virtual PictureModel PrepareCartItemPictureModel(ShoppingCartItem sci,
            int pictureSize, bool showDefaultPicture, string productName)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConsumer.CART_PICTURE_MODEL_KEY, sci.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            var model = _cacheManager.Get(pictureCacheKey,
                //as we cache per user (shopping cart item identifier)
                //let's cache just for 3 minutes
                3, () =>
                {
                    //shopping cart item picture
                    Picture sciPicture = null;

                    //first, let's see whether a shopping cart item has some attribute values with custom pictures
                    var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(sci.AttributesXml);
                    foreach (var pvaValue in pvaValues)
                    {
                        var pvavPicture = _pictureService.GetPictureById(pvaValue.PictureId);
                        if (pvavPicture != null)
                        {
                            sciPicture = pvavPicture;
                            break;
                        }
                    }

                    //now let's load the default product picture
                    var product = sci.Product;
                    if (sciPicture == null)
                    {
                        sciPicture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                    }

                    //let's check whether this product has some parent "grouped" product
                    if (sciPicture == null && !product.VisibleIndividually && product.ParentGroupedProductId > 0)
                    {
                        sciPicture = _pictureService.GetPicturesByProductId(product.ParentGroupedProductId, 1).FirstOrDefault();
                    }
                    return new PictureModel()
                    {
                        ImageUrl = _pictureService.GetPictureUrl(sciPicture, pictureSize, showDefaultPicture),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), productName),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), productName),
                    };
                });
            return model;
        }
        /// <summary>
        /// Prepare shopping cart model
        /// </summary>
        /// <param name="model">Model instance</param>
        /// <param name="cart">Shopping cart</param>
        /// <param name="isEditable">A value indicating whether cart is editable</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether we should validate checkout attributes when preparing the model</param>
        /// <param name="prepareEstimateShippingIfEnabled">A value indicating whether we should prepare "Estimate shipping" model</param>
        /// <param name="setEstimateShippingDefaultAddress">A value indicating whether we should prefill "Estimate shipping" model with the default customer address</param>
        /// <param name="prepareAndDisplayOrderReviewData">A value indicating whether we should prepare review data (such as billing/shipping address, payment or shipping data entered during checkout)</param>
        /// <returns>Model</returns>
        [NonAction]
        protected virtual void PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareEstimateShippingIfEnabled = true, bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            if (cart.Count == 0)
                return;

            #region Simple properties

            model.IsEditable = isEditable;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowProductSku;
            var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, _workContext.CurrentCustomer);
            bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
            }
            model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

            //gift card and gift card boxes
            model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCode = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode);
            var discount = _discountService.GetDiscountByCouponCode(discountCouponCode);
            if (discount != null &&
                discount.RequiresCouponCode &&
                _discountService.IsDiscountValid(discount, _workContext.CurrentCustomer))
                model.DiscountBox.CurrentCode = discount.CouponCode;
            model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Checkout attributes

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                var caModel = new ShoppingCartModel.CheckoutAttributeModel()
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = attribute.DefaultValue
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    caModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var caValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                    foreach (var caValue in caValues)
                    {
                        var pvaValueModel = new ShoppingCartModel.CheckoutAttributeValueModel()
                        {
                            Id = caValue.Id,
                            Name = caValue.GetLocalized(x => x.Name),
                            ColorSquaresRgb = caValue.ColorSquaresRgb,
                            IsPreSelected = caValue.IsPreSelected,
                        };
                        caModel.Values.Add(pvaValueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(caValue);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                            else if (priceAdjustmentBase < decimal.Zero)
                                pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                        }
                    }
                }



                //set already selected attributes
                string selectedCheckoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                    case AttributeControlType.ColorSquares:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                //clear default selection
                                foreach (var item in caModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedCaValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
                                foreach (var caValue in selectedCaValues)
                                    foreach (var item in caModel.Values)
                                        if (caValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                                if (enteredText.Count > 0)
                                    caModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                            if (selectedDateStr.Count > 0)
                            {
                                DateTime selectedDate;
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate))
                                {
                                    //successfully parsed
                                    caModel.SelectedDay = selectedDate.Day;
                                    caModel.SelectedMonth = selectedDate.Month;
                                    caModel.SelectedYear = selectedDate.Year;
                                }
                            }

                        }
                        break;
                    default:
                        break;
                }

                model.CheckoutAttributes.Add(caModel);
            }

            #endregion

     

            #region Cart items

            foreach (var sci in cart)
            {
                var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel()
                {
                    Id = sci.Id,
                    Sku = sci.Product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    ProductId = sci.Product.Id,
                    ProductName = sci.Product.GetLocalized(x => x.Name),
                    ProductSeName = sci.Product.GetSeName(),
                    Quantity = sci.Quantity,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml),
                };

                //allow editing?
                //1. setting enabled?
                //2. simple product?
                //3. has attribute or gift card?
                //4. visible individually?
                cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                    sci.Product.ProductType == ProductType.SimpleProduct &&
                    (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) || sci.Product.IsGiftCard) &&
                    sci.Product.VisibleIndividually;

                //allowed quantities
                var allowedQuantities = sci.Product.ParseAllowedQuatities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new System.Web.Mvc.SelectListItem()
                    {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }

                //recurring info
                if (sci.Product.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), sci.Product.RecurringCycleLength, sci.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                //unit prices
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
                    decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                }
                //subtotal, discount
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
                    decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    decimal shoppingCartItemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
                    decimal shoppingCartItemDiscountBase = shoppingCartItemSubTotalWithoutDiscountBase - shoppingCartItemSubTotalWithDiscountBase;
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                        _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
                }

                //item warnings
                var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                    _workContext.CurrentCustomer,
                    sci.ShoppingCartType,
                    sci.Product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.Quantity,
                    false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);

                model.Items.Add(cartItemModel);
            }

            #endregion

            #region Button payment methods

            var boundPaymentMethods = _paymentService
                .LoadActivePaymentMethods(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id)
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            foreach (var pm in boundPaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                pm.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);

                model.ButtonPaymentMethodActionNames.Add(actionName);
                model.ButtonPaymentMethodControllerNames.Add(controllerName);
                model.ButtonPaymentMethodRouteValues.Add(routeValues);
            }

            #endregion

            #region Order review data

            if (prepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData.Display = true;

                //billing info
                var billingAddress = _workContext.CurrentCustomer.BillingAddress;
                if (billingAddress != null)
                    model.OrderReviewData.BillingAddress.PrepareModel(billingAddress, false, _addressSettings);

                //shipping info
                if (cart.RequiresShipping())
                {
                    model.OrderReviewData.IsShippable = true;

                    if (_shippingSettings.AllowPickUpInStore)
                    {
                        model.OrderReviewData.SelectedPickUpInStore = _workContext.CurrentCustomer.GetAttribute<bool>(SystemCustomerAttributeNames.SelectedPickUpInStore, _storeContext.CurrentStore.Id);
                    }

                    if (!model.OrderReviewData.SelectedPickUpInStore)
                    {
                        var shippingAddress = _workContext.CurrentCustomer.ShippingAddress;
                        if (shippingAddress != null)
                        {
                            model.OrderReviewData.ShippingAddress.PrepareModel(shippingAddress, false, _addressSettings);
                        }
                    }


                    //selected shipping method
                    var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                    if (shippingOption != null)
                        model.OrderReviewData.ShippingMethod = shippingOption.Name;
                }
                //payment info
                var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                model.OrderReviewData.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : "";
            }
            #endregion
        }


        [NonAction]
        protected virtual void PrepareWishlistModel(WishlistModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
            model.IsEditable = isEditable;
            model.DisplayAddToCart = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);

            if (cart.Count == 0)
                return;

            #region Simple properties

            var customer = cart.FirstOrDefault().Customer;
            model.CustomerGuid = customer.CustomerGuid;
            model.CustomerFullname = customer.GetFullName();
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowProductSku;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Cart items

            foreach (var sci in cart)
            {
                var cartItemModel = new WishlistModel.ShoppingCartItemModel()
                {
                    Id = sci.Id,
                    Sku = sci.Product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    ProductId = sci.Product.Id,
                    ProductName = sci.Product.GetLocalized(x => x.Name),
                    ProductSeName = sci.Product.GetSeName(),
                    Quantity = sci.Quantity,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(sci.Product, sci.AttributesXml),
                };

                //allowed quantities
                var allowedQuantities = sci.Product.ParseAllowedQuatities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new System.Web.Mvc.SelectListItem()
                    {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }


                //recurring info
                if (sci.Product.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), sci.Product.RecurringCycleLength, sci.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                //unit prices
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
                    decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                }
                //subtotal, discount
                if (sci.Product.CallForPrice)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    decimal taxRate = decimal.Zero;
                    decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
                    decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    decimal shoppingCartItemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(sci.Product, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
                    decimal shoppingCartItemDiscountBase = shoppingCartItemSubTotalWithoutDiscountBase - shoppingCartItemSubTotalWithDiscountBase;
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                    }
                }

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                        _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
                }

                //item warnings
                var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                            sci.ShoppingCartType,
                            sci.Product,
                            sci.StoreId,
                            sci.AttributesXml,
                            sci.CustomerEnteredPrice,
                            sci.Quantity, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);

                model.Items.Add(cartItemModel);
            }

            #endregion
        }

         #endregion
        #region cart
        [ActionName("cart")]
        [HttpPost]

        public IHttpActionResult Cart([FromBody]LoginStatus status)
        {
            if (!_customerService.CheckLogin(status.userid,status.logguid))
                return Ok(new { success = false, msg = "登录超时" });
            var currentCustomer = _customerService.GetCustomerById(status.userid);

            var cart = currentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            PrepareShoppingCartModel(model, cart);
            return Ok(model);
        }

        [HttpPost]
        public IHttpActionResult AddProductToCart_Catalog(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Ok();
            }

            if (product.CustomerEntersPrice)
            {
                return Ok();
            }

            var allowedQuantities = product.ParseAllowedQuatities();
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Ok();
            }

            if (product.ProductVariantAttributes.Count > 0)
            {
                //product has some attributes. let a customer see them
                return Ok();
            }

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == cartType)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                product, _storeContext.CurrentStore.Id, string.Empty,
                decimal.Zero, quantityToValidate, false, true, false, false, false);
            if (addToCartWarnings.Count > 0)
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                product, cartType, _storeContext.CurrentStore.Id,
                string.Empty, decimal.Zero, quantity, true);
            if (addToCartWarnings.Count > 0)
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Ok();
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Ok();
                        }
                        else
                        {
                            //display notification message and update appropriate blocks
                            var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                                 _workContext.CurrentCustomer.ShoppingCartItems
                                 .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                                 .LimitPerStore(_storeContext.CurrentStore.Id)
                                 .ToList()
                                 .GetTotalProducts());
                            return Json(new
                            {
                                success = true,
                                message = "收藏夹",
                                updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                            });
                        }
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Ok();
                        }
                        else
                        {
                            return Ok(new {success = true});
                        }
                    }
            }
        }

    /// <summary>
    /// 商品加入购物车
    /// </summary>
    /// <param name="productId">商品id</param>
    /// <param name="shoppingCartTypeId">1.购物车2.收藏夹</param>
    /// <param name="quantity">数量</param>
    /// <param name="login">登录信息</param>
    /// <returns></returns>
    [ActionName("AddToCart")]
    [HttpPost]
        public IHttpActionResult AddProductToCart_Details([FromBody]AddToCart add)
        {
           
            LoginStatus login = add.login;
            int productId = add.productId;
            int quantity = add.quantity;
            int shoppingCartTypeId = add.shoppingCartTypeId;
            //检验是否已登录
            if (!_customerService.CheckLogin(login.userid, login.logguid))
                return Ok(new { success = false, msg = "登录超时" });
            Customer currentCustomer = _customerService.GetCustomerById(login.userid);
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Ok();
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Ok();
            }

            #region Quantity

            #endregion
            //save item
            var addToCartWarnings = new List<string>();
            var cartType = (ShoppingCartType)shoppingCartTypeId;

                //add to the cart
                addToCartWarnings.AddRange(_shoppingCartService.AddToCart(currentCustomer,
                    product, cartType, _storeContext.CurrentStore.Id,
                    "", 0, quantity, true));

            #region Return result

            if (addToCartWarnings.Count > 0)
            {
                //cannot be added to the cart/wishlist
                //let's display warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                           return Ok(new{success=true});
                        }
                        else
                        {
                            //display notification message and update appropriate blocks
                           return Ok(new {success=true}); 
                        }
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Ok(new { success = true });
                        }
                        else
                        {
                            return Ok(new { success = true });
                        }
                    }
            }
            #endregion
        }
        
        /// <summary>
        /// 跟新购物车
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /*
    public IHttpActionResult UpdateCart(FormCollection form)
    {
        if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
            return RedirectToRoute("HomePage");

        var cart = _workContext.CurrentCustomer.ShoppingCartItems
            .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
            .LimitPerStore(_storeContext.CurrentStore.Id)
            .ToList();

        var allIdsToRemove = form["removefromcart"] != null ? form["removefromcart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();

        //current warnings <cart item identifier, warnings>
        var innerWarnings = new Dictionary<int, IList<string>>();
        foreach (var sci in cart)
        {
            bool remove = allIdsToRemove.Contains(sci.Id);
            if (remove)
                _shoppingCartService.DeleteShoppingCartItem(sci, ensureOnlyActiveCheckoutAttributes: true);
            else
            {
                foreach (string formKey in form.AllKeys)
                    if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int newQuantity = sci.Quantity;
                        if (int.TryParse(form[formKey], out newQuantity))
                        {
                            var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                newQuantity, true);
                            innerWarnings.Add(sci.Id, currSciWarnings);
                        }
                        break;
                    }
            }
        }
    }


        */
        /// <summary>
        /// 从购物车中删除
        /// </summary>
    /// <param name="delete">cartId，userid，logguid</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("deletecart")]
        public IHttpActionResult DeleteCartlist([FromBody]DeleteCart delete){
            var login = delete.login;
            var cartId = delete.cartId;
            if (!_customerService.CheckLogin(login.userid, login.logguid))
            {
                return Ok(new { success = false, msg = "登录超时" });
            }
            var currentCustomer = _customerService.GetCustomerById(login.userid);
            var cart = currentCustomer.ShoppingCartItems.Where(w => w.ShoppingCartType == ShoppingCartType.ShoppingCart && w.Id == cartId).LimitPerStore(_storeContext.CurrentStore.Id).FirstOrDefault();
            
            if (cart != null)
            {
                _shoppingCartService.DeleteShoppingCartItem(cart);
                return Ok(new { success = true });
            }
            else {
                return Ok(new { success = false, msg = "所选相已删除或不存在！" });
            }
            
        }
        /// <summary>
        /// 收藏夹删除
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("deletewish")]
        public IHttpActionResult DeleteWishlist([FromBody]DeleteCart delete)
        {
            var login = delete.login;
            var cartId = delete.cartId;
            if (!_customerService.CheckLogin(login.userid, login.logguid))
            {
                return Ok(new { success = false, msg = "登录超时" });
            }
            var currentCustomer = _customerService.GetCustomerById(login.userid);
            var cart = currentCustomer.ShoppingCartItems.Where(w => w.ShoppingCartType == ShoppingCartType.Wishlist && w.Id == cartId).LimitPerStore(_storeContext.CurrentStore.Id).FirstOrDefault();

            if (cart != null)
            {
                _shoppingCartService.DeleteShoppingCartItem(cart);
                return Ok(new { success = true });
            }
            else
            {
                return Ok(new { success = false, msg = "所选相已删除或不存在！" });
            }

        }

        /// <summary>
        /// 跟新购物车数量
        /// </summary>
        /// <param name="cartids"></param>
        /// <param name="quantity"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("UpdateCarts")]
        public IHttpActionResult UpdateCarts(UpdateCart update) {
            if (!_customerService.CheckLogin(update.login.userid, update.login.logguid))
            {
                return Ok(new { success = false, msg = "登录超时" });
            }
            if (string.IsNullOrEmpty(update.cartids) || string.IsNullOrEmpty(update.quantity))
            {
                return Ok(new {success=false });
            }
            var ids = update.cartids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            var quantities = update.quantity.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
            var currentCustomer = _customerService.GetCustomerById(update.login.userid);
            var carts=currentCustomer.ShoppingCartItems.Where(w=>w.ShoppingCartType==ShoppingCartType.ShoppingCart);
            try
            {
                for (int i = 0; i < ids.Count(); i++)
                {
                    foreach (var cart in carts)
                    {
                        if (cart.Id == ids[i])
                        {
                            var newquantity = quantities[i];
                            _shoppingCartService.UpdateShoppingCartItem(currentCustomer, cart.Id, cart.AttributesXml, cart.CustomerEnteredPrice, newquantity, true);
                        }

                    }
                }
            }
            catch (Exception e) {

                return Ok(new { success = false });
            }

            return Ok(new { success = true });
        }
        #endregion
        #region Wishlist
        [ActionName("wishlist")]
        [HttpPost]
        public IHttpActionResult Wishlist([FromBody]LoginStatus status)
        {
            if (!_customerService.CheckLogin(status.userid, status.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(status.userid);
           
            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            PrepareWishlistModel(model, cart, false);
            return Ok(model);
        }
        #endregion

    }
}