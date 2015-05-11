using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


using System.Web;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.UI.Captcha;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Plugin.Misc.WebApiServices.Models;

namespace Nop.Plugin.Misc.WebApiServices.Controllers
{
    public class CustomerController : BaseApiController
    {
        #region Fields
        private readonly IAuthenticationService _authenticationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ITaxService _taxService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ForumSettings _forumSettings;
        private readonly OrderSettings _orderSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IForumService _forumService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;

        private readonly MediaSettings _mediaSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;

        #endregion
        #region
        public CustomerController(IAuthenticationService authenticationService,
            IDateTimeHelper dateTimeHelper,
            DateTimeSettings dateTimeSettings, 
            TaxSettings taxSettings,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ICustomerService customerService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            IGenericAttributeService genericAttributeService,
            ICustomerRegistrationService customerRegistrationService,
            ITaxService taxService, RewardPointsSettings rewardPointsSettings,
            CustomerSettings customerSettings,AddressSettings addressSettings, ForumSettings forumSettings,
            OrderSettings orderSettings, IAddressService addressService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IOrderProcessingService orderProcessingService, IOrderService orderService,
            ICurrencyService currencyService, IPriceFormatter priceFormatter,
            IPictureService pictureService, INewsLetterSubscriptionService newsLetterSubscriptionService,
            IForumService forumService, IShoppingCartService shoppingCartService,
            IOpenAuthenticationService openAuthenticationService, 
            IBackInStockSubscriptionService backInStockSubscriptionService, 
            IDownloadService downloadService, IWebHelper webHelper,
            ICustomerActivityService customerActivityService, MediaSettings mediaSettings,
            IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings, ExternalAuthenticationSettings externalAuthenticationSettings)
        {
            this._authenticationService = authenticationService;
            this._dateTimeHelper = dateTimeHelper;
            this._dateTimeSettings = dateTimeSettings;
            this._taxSettings = taxSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._customerService = customerService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerAttributeService = customerAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._customerRegistrationService = customerRegistrationService;
            this._taxService = taxService;
            this._rewardPointsSettings = rewardPointsSettings;
            this._customerSettings = customerSettings;
            this._addressSettings = addressSettings;
            this._forumSettings = forumSettings;
            this._orderSettings = orderSettings;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._orderProcessingService = orderProcessingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._orderService = orderService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._pictureService = pictureService;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._forumService = forumService;
            this._shoppingCartService = shoppingCartService;
            this._openAuthenticationService = openAuthenticationService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._downloadService = downloadService;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;

            this._mediaSettings = mediaSettings;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
        }

        #endregion
        #region utilities

        [NonAction]
        protected virtual void TryAssociateAccountWithExternalAccount(Customer customer)
        {
            var parameters = ExternalAuthorizerHelper.RetrieveParametersFromRoundTrip(true);
            if (parameters == null)
                return;

            if (_openAuthenticationService.AccountExists(parameters))
                return;

            _openAuthenticationService.AssociateExternalAccountWithUser(customer, parameters);
        }

        [NonAction]
        protected virtual void PrepareCustomerRegisterModel(RegisterModel model, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
           
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            //form fields
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;

            //countries and states
           

            #region Custom customer attributes

            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var caModel = new CustomerAttributeModel()
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var caValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                    foreach (var caValue in caValues)
                    {
                        var caValueModel = new CustomerAttributeValueModel()
                        {
                            Id = caValue.Id,
                            Name = caValue.GetLocalized(x => x.Name),
                            IsPreSelected = caValue.IsPreSelected
                        };
                        caModel.Values.Add(caValueModel);
                    }
                }

                model.CustomerAttributes.Add(caModel);
            }

            #endregion
        }
        [NonAction]
        protected virtual CustomerNavigationModel GetCustomerNavigationModel(Customer customer)
        {
            var model = new CustomerNavigationModel();
            model.HideAvatar = !_customerSettings.AllowCustomersToUploadAvatars;
            model.HideRewardPoints = !_rewardPointsSettings.Enabled;
            model.HideForumSubscriptions = !_forumSettings.ForumsEnabled || !_forumSettings.AllowCustomersToManageSubscriptions;
            model.HideReturnRequests = !_orderSettings.ReturnRequestsEnabled ||
                _orderService.SearchReturnRequests(_storeContext.CurrentStore.Id, customer.Id, 0, null, 0, 1).Count == 0;
            model.HideDownloadableProducts = _customerSettings.HideDownloadableProductsTab;
            model.HideBackInStockSubscriptions = _customerSettings.HideBackInStockSubscriptionsTab;
            return model;
        }

        [NonAction]
        protected virtual void PrepareCustomerInfoModel(CustomerInfoModel model, Customer customer, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (customer == null)
                throw new ArgumentNullException("customer");

            model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
                model.AvailableTimeZones.Add(new System.Web.Mvc.SelectListItem() { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == _dateTimeHelper.CurrentTimeZone.Id) });

            if (!excludeProperties)
            {
                model.VatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
                model.FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
                model.LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);
                model.Gender = customer.GetAttribute<string>(SystemCustomerAttributeNames.Gender);
                var dateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                if (dateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = dateOfBirth.Value.Day;
                    model.DateOfBirthMonth = dateOfBirth.Value.Month;
                    model.DateOfBirthYear = dateOfBirth.Value.Year;
                }
                model.Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company);
                model.StreetAddress = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress);
                model.StreetAddress2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2);
                model.ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode);
                model.City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City);
                model.CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
                model.StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId);
                model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
                model.Fax = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);

                //newsletter
                var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                model.Newsletter = newsletter != null && newsletter.Active;

                model.Signature = customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature);

                model.Email = customer.Email;
                model.Username = customer.Username;
            }
            else
            {
                if (_customerSettings.UsernamesEnabled && !_customerSettings.AllowUsersToChangeUsernames)
                    model.Username = customer.Username;
            }

            //countries and states
            if (_customerSettings.CountryEnabled)
            {
                model.AvailableCountries.Add(new System.Web.Mvc.SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in _countryService.GetAllCountries())
                {
                    model.AvailableCountries.Add(new System.Web.Mvc.SelectListItem()
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (_customerSettings.StateProvinceEnabled)
                {
                    //states
                    var states = _stateProvinceService.GetStateProvincesByCountryId(model.CountryId).ToList();
                    if (states.Count > 0)
                    {
                        foreach (var s in states)
                            model.AvailableStates.Add(new System.Web.Mvc.SelectListItem() { Text = s.GetLocalized(x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                    }
                    else
                        model.AvailableStates.Add(new System.Web.Mvc.SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

                }
            }
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.VatNumberStatusNote = ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId))
                .GetLocalizedEnum(_localizationService, _workContext);
            model.GenderEnabled = _customerSettings.GenderEnabled;
            model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
            model.CompanyEnabled = _customerSettings.CompanyEnabled;
            model.CompanyRequired = _customerSettings.CompanyRequired;
            model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
            model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
            model.CityEnabled = _customerSettings.CityEnabled;
            model.CityRequired = _customerSettings.CityRequired;
            model.CountryEnabled = _customerSettings.CountryEnabled;
            model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
            model.PhoneEnabled = _customerSettings.PhoneEnabled;
            model.PhoneRequired = _customerSettings.PhoneRequired;
            model.FaxEnabled = _customerSettings.FaxEnabled;
            model.FaxRequired = _customerSettings.FaxRequired;
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _customerSettings.AllowUsersToChangeUsernames;
            model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
            model.SignatureEnabled = _forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled;

            //external authentication
            model.NumberOfExternalAuthenticationProviders = _openAuthenticationService
                .LoadActiveExternalAuthenticationMethods(_storeContext.CurrentStore.Id)
                .Count;
            foreach (var ear in _openAuthenticationService.GetExternalIdentifiersFor(customer))
            {
                var authMethod = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(ear.ProviderSystemName);
                if (authMethod == null || !authMethod.IsMethodActive(_externalAuthenticationSettings))
                    continue;

                model.AssociatedExternalAuthRecords.Add(new CustomerInfoModel.AssociatedExternalAuthModel()
                {
                    Id = ear.Id,
                    Email = ear.Email,
                    ExternalIdentifier = ear.ExternalIdentifier,
                    AuthMethodName = authMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id)
                });
            }

            #region Custom customer attributes

            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                var caModel = new CustomerAttributeModel()
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var caValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                    foreach (var caValue in caValues)
                    {
                        var caValueModel = new CustomerAttributeValueModel()
                        {
                            Id = caValue.Id,
                            Name = caValue.GetLocalized(x => x.Name),
                            IsPreSelected = caValue.IsPreSelected
                        };
                        caModel.Values.Add(caValueModel);
                    }
                }

                //set already selected attributes
                string selectedCustomerAttributes = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes, _genericAttributeService);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedCustomerAttributes))
                            {
                                //clear default selection
                                foreach (var item in caModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedCaValues = _customerAttributeParser.ParseCustomerAttributeValues(selectedCustomerAttributes);
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
                            if (!String.IsNullOrEmpty(selectedCustomerAttributes))
                            {
                                var enteredText = _customerAttributeParser.ParseValues(selectedCustomerAttributes, attribute.Id);
                                if (enteredText.Count > 0)
                                    caModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                model.CustomerAttributes.Add(caModel);
            }

            #endregion

            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Info;
        }
        [NonAction]
        protected virtual CustomerOrderListModel PrepareCustomerOrderListModel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var model = new CustomerOrderListModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Orders;
            var orders = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: customer.Id);
            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderListModel.OrderDetailsModel()
                {
                    Id = order.Id,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order)
                };
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

                model.Orders.Add(orderModel);
            }

            var recurringPayments = _orderService.SearchRecurringPayments(_storeContext.CurrentStore.Id,
                customer.Id, 0, null, 0, int.MaxValue);
            foreach (var recurringPayment in recurringPayments)
            {
                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel()
                {
                    Id = recurringPayment.Id,
                    StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
                    CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, _workContext)),
                    NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = recurringPayment.CyclesRemaining,
                    InitialOrderId = recurringPayment.InitialOrder.Id,
                    CanCancel = _orderProcessingService.CanCancelRecurringPayment(customer, recurringPayment),
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;
        }

        [NonAction]
        protected virtual string ParseCustomCustomerAttributes(Customer customer,IDictionary<string,string> form)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (form == null)
                throw new ArgumentNullException("form");

            string selectedAttributes = "";
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in customerAttributes)
            {
                string controlId = string.Format("customer_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    selectedAttributes = _customerAttributeParser.AddCustomerAttribute(selectedAttributes,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        selectedAttributes = _customerAttributeParser.AddCustomerAttribute(selectedAttributes,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var cvaValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in cvaValues
                                .Where(pvav => pvav.IsPreSelected)
                                .Select(pvav => pvav.Id)
                                .ToList())
                            {
                                selectedAttributes = _customerAttributeParser.AddCustomerAttribute(selectedAttributes,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                selectedAttributes = _customerAttributeParser.AddCustomerAttribute(selectedAttributes,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return selectedAttributes;
        }


        #endregion
        #region method

        //password email
        [HttpPost]
        [ActionName("login")]
        public IHttpActionResult Login([FromBody]LoginModel model)
        {
            //validate CAPTCHA
            Customer customer=new Customer();
            string msg = string.Empty;

            if (ModelState.IsValid)
            {
                var loginResult = _customerRegistrationService.ValidateCustomer(model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                             customer = _customerService.GetCustomerByEmail(model.Email);
                             customer.logstatus = 1;
                             customer.logguid = System.Guid.NewGuid().ToString();

                            //migrate shopping cart
                            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                            //sign in new customer
                           _authenticationService.SignIn(customer, model.RememberMe);
                           customer.Addresses.Clear();
                           customer.BillingAddress = null;
                           customer.ShippingAddress = null;
                           customer.CustomerRoles.Clear();
                           customer.ShoppingCartItems.Clear();
                            //activity log
                            _customerActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);
                           
                            return Ok(new{ flag=true,customer=customer,msg=msg});
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        msg=_localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist");
                        break;
                    case CustomerLoginResults.Deleted:
                       msg=_localizationService.GetResource("Account.Login.WrongCredentials.Deleted");
                        break;
                    case CustomerLoginResults.NotActive:
                       msg=_localizationService.GetResource("Account.Login.WrongCredentials.NotActive");
                        break;
                    case CustomerLoginResults.NotRegistered:
                        msg=_localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered");
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        msg= _localizationService.GetResource("Account.Login.WrongCredentials");
                        break;
                }
            }

            //If we got this far, something failed, redisplay form
            model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage;
            return Ok(new {flag=false,customer=customer,msg=msg});
        }

        [HttpPost]
        [ActionName("Register")]
        public IHttpActionResult Register(RegisterModel model)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer. 
                _authenticationService.SignOut();

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;

            //validate CAPTCHA
           

            //custom customer attributes
            var customerAttributes = "";
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributes);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer, model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password, _customerSettings.DefaultPasswordFormat, isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);

                        string vatName = "";
                        string vatAddress = "";
                        var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out vatName, out vatAddress);
                        _genericAttributeService.SaveAttribute(customer,
                            SystemCustomerAttributeNames.VatNumberStatusId,
                            (int)vatNumberStatus);
                        //send VAT number admin notification
                        if (!String.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);

                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = null;
                        try
                        {
                            dateOfBirth = new DateTime(model.DateOfBirthYear.Value, model.DateOfBirthMonth.Value, model.DateOfBirthDay.Value);
                        }
                        catch { }
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            //else
                            //{
                            //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                            //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                            //}
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription()
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    //save customer attributes
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributes);

                    //login customer now
                    if (isApproved)
                        _authenticationService.SignIn(customer, true);

                    //associated with external account (if possible)
                    TryAssociateAccountWithExternalAccount(customer);

                    //insert default address (if possible)
                    var defaultAddress = new Address()
                    {
                        FirstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                        LastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                        Email = customer.Email,
                        Company = customer.GetAttribute<string>(SystemCustomerAttributeNames.Company),
                        CountryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) > 0 ?
                            (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId) : null,
                        StateProvinceId = customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) > 0 ?
                            (int?)customer.GetAttribute<int>(SystemCustomerAttributeNames.StateProvinceId) : null,
                        City = customer.GetAttribute<string>(SystemCustomerAttributeNames.City),
                        Address1 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                        Address2 = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress2),
                        ZipPostalCode = customer.GetAttribute<string>(SystemCustomerAttributeNames.ZipPostalCode),
                        PhoneNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                        FaxNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax),
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (this._addressService.IsAddressValid(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        customer.Addresses.Add(defaultAddress);
                        customer.BillingAddress = defaultAddress;
                        customer.ShippingAddress = defaultAddress;
                        _customerService.UpdateCustomer(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer, _localizationSettings.DefaultAdminLanguageId);

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            {
                                //email validation message
                                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                                _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                //result
                                return Ok(new { result = UserRegistrationType.EmailValidation });
                            }
                        case UserRegistrationType.AdminApproval:
                            {
                                return Ok(new { result = UserRegistrationType.AdminApproval });
                            }
                        case UserRegistrationType.Standard:
                            {
                                //send customer welcome message
                                _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);


                                return Ok(new { result = UserRegistrationType.Standard });
                            }
                        default:
                            {
                                return Ok(new { result = UserRegistrationType.Standard });
                            }
                    }
                }
                else
                {
                    foreach (var error in registrationResult.Errors)
                        ModelState.AddModelError("", error);
                }
            }

            //If we got this far, something failed, redisplay form
            PrepareCustomerRegisterModel(model, true);
            return Ok(model);
        }


        #endregion


        #region MyAccount
        [HttpPost]
        [ActionName("info")]
        public IHttpActionResult Info([FromBody]LoginStatus login)
        {
            if (!_customerService.CheckLogin(login.userid, login.logguid)) {
                return Ok(new { success = false, msg = "登录超时" });
            }
            var customer = _customerService.GetCustomerById(login.userid);
            var model = new CustomerInfoModel();
            PrepareCustomerInfoModel(model, customer, false);
            //时区去除
            model.AvailableTimeZones.Clear();
            return Ok(model);
        }


        [HttpPost]
        [ActionName("saveinfo")]
        public IHttpActionResult Info(System.Net.Http.Formatting.FormDataCollection formdata)
        {
           
            IDictionary<string, string> form = new Dictionary<string, string>();
            foreach (var t in formdata) {
                form.Add(t.Key, t.Value);
            }
            var login = new LoginStatus();
            login.userid = int.Parse(form["userid"]);
            login.logguid = form["logguid"];

            if (!_customerService.CheckLogin(login.userid,login.logguid))
                return Ok(new { success = false, msg = "登录超时" });
            var model = new CustomerInfoModel() { };
  
            if(form.ContainsKey("Username"))
                model.Username = form["Username"];
            if(form.ContainsKey("Email"))
                model.Email = form["Email"];
            if(form.ContainsKey("TimeZoneId"))
                model.TimeZoneId = form["TimeZoneId "];
            if(form.ContainsKey("VatNumber"))
                model.VatNumber = form["VatNumber "];
            if(form.ContainsKey("Gender"))
                model.Gender = form["Gender"];
            if(form.ContainsKey("FirstName"))
                model.FirstName = form["FirstName"];
            if(form.ContainsKey("LastName"))            
                model.LastName = form["LastName"];
             if(form.ContainsKey("DateOfBirthYear"))      
                 model.DateOfBirthYear = Convert.ToInt32(form["DateOfBirthYear"]);     
              if(form.ContainsKey("DateOfBirthMonth"))           
               model.DateOfBirthMonth = Convert.ToInt32(form["DateOfBirthMonth"]);  
             if(form.ContainsKey("DateOfBirthDay"))                         
               model.DateOfBirthDay = Convert.ToInt32(form["DateOfBirthDay"]);
            if(form.ContainsKey("Company"))  
              model.Company = form["Company"];
            if (form.ContainsKey("StreetAddress")) 
                model.StreetAddress = form["StreetAddress"];
            if (form.ContainsKey("StreetAddress2"))  
                model.StreetAddress2 = form["StreetAddress2"];
            if (form.ContainsKey("ZipPostalCode"))  
                 model.ZipPostalCode = form["ZipPostalCode"];
            if (form.ContainsKey("City"))  
                 model.City = form["City"];
            if (form.ContainsKey("CountryId"))  
               model.CountryId = Convert.ToInt32(form["CountryId"]);
            if (form.ContainsKey("StateProvinceId"))  
               model.StateProvinceId = Convert.ToInt32(form["StateProvinceId"]);
            if (form.ContainsKey("Phone"))  
                model.Phone = form["Phone"];
            if (form.ContainsKey("Fax"))  
                model.Fax = form["Fax"];
            if (form.ContainsKey("Newsletter"))  
                 model.Newsletter = bool.Parse(form["Newsletter"]);
            if (form.ContainsKey("Signature"))
                 model.Signature = form["Signature"];                          
            var customer = _customerService.GetCustomerById(login.userid);

            try
            {
                //custom customer attributes
                var customerAttributes = ParseCustomCustomerAttributes(customer, form);
                var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributes);
                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }

                if (ModelState.IsValid)
                {
                    //username 
                    if (_customerSettings.UsernamesEnabled && this._customerSettings.AllowUsersToChangeUsernames)
                    {
                        if (!customer.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //change username
                            _customerRegistrationService.SetUsername(customer, model.Username.Trim());
                            //re-authenticate
                            _authenticationService.SignIn(customer, true);
                        }
                    }
                    //email
                    if (!customer.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        _customerRegistrationService.SetEmail(customer, model.Email.Trim());
                        //re-authenticate (if usernames are disabled)
                        if (!_customerSettings.UsernamesEnabled)
                        {
                            _authenticationService.SignIn(customer, true);
                        }
                    }

                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.TimeZoneId, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        var prevVatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
                        if (prevVatNumber != model.VatNumber)
                        {
                            string vatName = "";
                            string vatAddress = "";
                            var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out vatName, out vatAddress);
                            _genericAttributeService.SaveAttribute(customer,
                                    SystemCustomerAttributeNames.VatNumberStatusId,
                                    (int)vatNumberStatus);
                            //send VAT number admin notification
                            if (!String.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        DateTime? dateOfBirth = null;
                        try
                        {
                            dateOfBirth = new DateTime(model.DateOfBirthYear.Value, model.DateOfBirthMonth.Value, model.DateOfBirthDay.Value);
                        }
                        catch { }
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(customer.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);
                            }
                            else
                                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription()
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                        _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Signature, model.Signature);

                    //save customer attributes
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.CustomCustomerAttributes, customerAttributes);

                    return Ok(new {success=true });
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", exc.Message);
                return Ok(new { success = false });
            }


            //If we got this far, something failed, redisplay form
            PrepareCustomerInfoModel(model, customer, true);
            return Ok(model);
        }
        #endregion

        #region Address
        [HttpPost]
        [ActionName("Addresses")]
        public IHttpActionResult Addresses([FromBody]LoginStatus login)
        {
            if (!_customerService.CheckLogin(login.userid,login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(login.userid);

            var model = new CustomerAddressListModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;
            var addresses = customer.Addresses
                //enabled for the current store
                .Where(a => a.Country == null || _storeMappingService.Authorize(a.Country))
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                addressModel.PrepareModel(address, false, _addressSettings, _localizationService,
                    _stateProvinceService, () => _countryService.GetAllCountries());
                model.Addresses.Add(addressModel);
            }
            return Ok(model);
        }
        [HttpPost]
        [ActionName("AddressDelete")]
        public IHttpActionResult AddressDelete([FromBody]AddressOperateModel model)
        {
            if (!_customerService.CheckLogin(model.login.userid,model.login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(model.login.userid);

            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == model.addressId);
            if (address != null)
            {
                customer.RemoveAddress(address);
                _customerService.UpdateCustomer(customer);
                //now delete the address record
                _addressService.DeleteAddress(address);
            }

            return Ok(new { success = true });
        }
        /// <summary>
        /// 新增地址页面信息加载
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddressAddInfo")]
        public IHttpActionResult AddressAddInfo([FromBody]LoginStatus login)
        {
            if (!_customerService.CheckLogin(login.userid,login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(login.userid);

            var model = new CustomerAddressEditModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;
            model.Address.PrepareModel(null, false, _addressSettings, _localizationService,
                    _stateProvinceService, () => _countryService.GetAllCountries());

            return Ok(model);
        }
        /// <summary>
        /// 增加地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddressAdd")]
        public IHttpActionResult AddressAdd(AddressEditModel model)
        {
            if (!_customerService.CheckLogin(model.login.userid,model.login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(model.login.userid);


            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                customer.Addresses.Add(address);
                _customerService.UpdateCustomer(customer);

                return Ok(new {success=true});
            }

            //If we got this far, something failed, redisplay form
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;
            model.Address.PrepareModel(null, true, _addressSettings, _localizationService,
                    _stateProvinceService, () => _countryService.GetAllCountries());

            return Ok(model);
        }
        /// <summary>
        /// 编辑地址信息
        /// </summary>
        /// <param name="addressmodel"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddressEditInfo")]
        public IHttpActionResult AddressEditInfo([FromBody]AddressOperateModel addressmodel)
        {
            if (!_customerService.CheckLogin(addressmodel.login.userid, addressmodel.login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(addressmodel.login.userid);
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == addressmodel.addressId);
            if (address == null)
                //address is not found
                return Ok();

            var model = new CustomerAddressEditModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;
            model.Address.PrepareModel(address, false, _addressSettings, _localizationService,
                    _stateProvinceService, () => _countryService.GetAllCountries());

            return Ok(model);
        }
        /// <summary>
        /// 跟新地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddressEdit")]
        public IHttpActionResult AddressEdit([FromBody]AddressEditModel model)
        {
            if (!_customerService.CheckLogin(model.login.userid,model.login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(model.login.userid);
            //find address (ensure that it belongs to the current customer)
            var address = customer.Addresses.FirstOrDefault(a => a.Id == model.addressId);
            if (address == null)
                //address is not found
                return Ok(new {success=false });
            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                _addressService.UpdateAddress(address);

                return Ok(new { success = true });
            }

            //If we got this far, something failed, redisplay form
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.Addresses;
            model.Address.PrepareModel(address, true, _addressSettings, _localizationService,
                    _stateProvinceService, () => _countryService.GetAllCountries());
            return Ok(model);
        }

        #endregion
        #region order
        [HttpPost]
        [ActionName("Orders")]
        public IHttpActionResult Orders([FromBody]LoginStatus login)
        {
            if (!_customerService.CheckLogin(login.userid, login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            var customer = _customerService.GetCustomerById(login.userid);
            var model = PrepareCustomerOrderListModel(customer);
            return Ok(model);
        }

        #endregion

        [ActionName("ChangePassword")]
        [HttpPost]
        public IHttpActionResult ChangePassword([FromBody]ChangePwdModel model) {
            if (!_customerService.CheckLogin(model.login.userid,model.login.logguid))
                return Ok(new {success=false,msg="登录超时" });
            var customer = _customerService.GetCustomerById(model.login.userid);
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.ChangePassword;

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = _customerRegistrationService.ChangePassword(changePasswordRequest);
                if (changePasswordResult.Success)
                {
                    customer.logstatus = 0;
                    customer.logguid = string.Empty;
                    _customerService.UpdateCustomer(customer);
                    return Ok(new {success=true });
                }
                else
                {
                    foreach (var error in changePasswordResult.Errors)
                        ModelState.AddModelError("", error);
                }
            }
            //If we got this far, something failed, redisplay form
            return Ok(model);
        }
        [HttpPost]
        [ActionName("RewardPoints")]
        public IHttpActionResult RewardPoints(LoginStatus login)
        {
            if (!_customerService.CheckLogin(login.userid,login.logguid))
                return Ok(new { success = false, msg = "登录超时" });

            if (!_rewardPointsSettings.Enabled)
                return Ok(new {success=false,msg="未开启积分功能"});

            var customer = _customerService.GetCustomerById(login.userid);

            var model = new CustomerRewardPointsModel();
            model.NavigationModel = GetCustomerNavigationModel(customer);
            model.NavigationModel.SelectedTab = CustomerNavigationEnum.RewardPoints;
            foreach (var rph in customer.RewardPointsHistory.OrderByDescending(rph => rph.CreatedOnUtc).ThenByDescending(rph => rph.Id))
            {
                model.RewardPoints.Add(new CustomerRewardPointsModel.RewardPointsHistoryModel()
                {
                    Points = rph.Points,
                    PointsBalance = rph.PointsBalance,
                    Message = rph.Message,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            //current amount/balance
            int rewardPointsBalance = customer.GetRewardPointsBalance();
            decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
            decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);
            model.RewardPointsBalance = rewardPointsBalance;
            model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
            //minimum amount/balance
            int minimumRewardPointsBalance = _rewardPointsSettings.MinimumRewardPointsToUse;
            decimal minimumRewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(minimumRewardPointsBalance);
            decimal minimumRewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(minimumRewardPointsAmountBase, _workContext.WorkingCurrency);
            model.MinimumRewardPointsBalance = minimumRewardPointsBalance;
            model.MinimumRewardPointsAmount = _priceFormatter.FormatPrice(minimumRewardPointsAmount, true, false);
            return Ok(model);
        }


    }
}