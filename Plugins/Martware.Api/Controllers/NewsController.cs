using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.UI.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.News;


namespace Nop.Plugin.Misc.WebApiServices.Controllers
{
    public class NewsController : BaseApiController
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreMappingService _storeMappingService;

        private readonly MediaSettings _mediaSettings;
        private readonly NewsSettings _newsSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public NewsController(INewsService newsService,
            IWorkContext workContext, IStoreContext storeContext,
            IPictureService pictureService, ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IWorkflowMessageService workflowMessageService, IWebHelper webHelper,
            ICacheManager cacheManager, ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            MediaSettings mediaSettings, NewsSettings newsSettings,
            LocalizationSettings localizationSettings, CustomerSettings customerSettings,
            CaptchaSettings captchaSettings)
        {
            this._newsService = newsService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._workflowMessageService = workflowMessageService;
            this._webHelper = webHelper;
            this._cacheManager = cacheManager;
            this._customerActivityService = customerActivityService;
            this._storeMappingService = storeMappingService;

            this._mediaSettings = mediaSettings;
            this._newsSettings = newsSettings;
            this._localizationSettings = localizationSettings;
            this._customerSettings = customerSettings;
            this._captchaSettings = captchaSettings;
        }

        #endregion
        #region Utilities

        [NonAction]
        protected virtual void PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem, bool prepareComments)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = newsItem.Id;
            model.MetaTitle = newsItem.MetaTitle;
            model.MetaDescription = newsItem.MetaDescription;
            model.MetaKeywords = newsItem.MetaKeywords;
            model.SeName = newsItem.GetSeName(newsItem.LanguageId, ensureTwoPublishedLanguages: false);
            model.Title = newsItem.Title;
            model.Category = newsItem.Category;
            model.Short = newsItem.Short;
            model.Full = newsItem.Full;
            model.AllowComments = newsItem.AllowComments;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.CreatedOnUtc, DateTimeKind.Utc);
            model.NumberOfComments = newsItem.CommentCount;
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnNewsCommentPage;
            if (prepareComments)
            {
                var newsComments = newsItem.NewsComments.OrderBy(pr => pr.CreatedOnUtc);
                foreach (var nc in newsComments)
                {
                    var commentModel = new NewsCommentModel()
                    {
                        Id = nc.Id,
                        CustomerId = nc.CustomerId,
                        CustomerName = nc.Customer.FormatUserName(),
                        CommentTitle = nc.CommentTitle,
                        CommentText = nc.CommentText,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(nc.CreatedOnUtc, DateTimeKind.Utc),
                        AllowViewingProfiles = _customerSettings.AllowViewingProfiles && nc.Customer != null && !nc.Customer.IsGuest(),
                    };
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        commentModel.CustomerAvatarUrl = _pictureService.GetPictureUrl(
                            nc.Customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId),
                            _mediaSettings.AvatarPictureSize,
                            _customerSettings.DefaultAvatarEnabled,
                            defaultPictureType: PictureType.Avatar);
                    }
                    model.Comments.Add(commentModel);
                }
            }
        }

        #endregion
        [ActionName("homepage")]
        [HttpGet]
        public IHttpActionResult HomePageNews()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return null;

            var cacheKey = string.Format(ModelCacheEventConsumer.HOMEPAGE_NEWSMODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var newsItems = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id, 0, _newsSettings.MainPageNewsCount);
                return new HomePageNewsItemsModel()
                {
                    WorkingLanguageId = _workContext.WorkingLanguage.Id,
                    NewsItems = newsItems
                        .Select(x =>
                                    {
                                        var newsModel = new NewsItemModel();
                                        PrepareNewsItemModel(newsModel, x, true);
                                        return newsModel;
                                    })
                        .ToList()
                };
            });
            var model = (HomePageNewsItemsModel)cachedModel.Clone();
           // foreach (var newsItemModel in model.NewsItems)
            //    newsItemModel.Comments.Clear();
            return Ok(model);
        }

        [ActionName("list")]
        [HttpGet]
        public IHttpActionResult List(int id=0,[FromUri]int pageSize=0,[FromUri]int pageNumber=0) {
            int category = id;

            if (!_newsSettings.Enabled)
                return null;

            var model = new NewsItemListModel();
            var newsItem = new List<NewsItem>();
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;

            if (pageSize <= 0) pageSize = _newsSettings.NewsArchivePageSize;
            if (pageNumber <= 0) pageNumber = 1;
/*
            var newsItems = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id,
                pageNumber - 1, pageSize);
            model.PagingFilteringContext.LoadPagedList(newsItems);
            if (category != 0)
            {
                newsItem = newsItems.Where(w => w.Category == category).ToList();
            }
            else
            {
                newsItem = newsItems.ToList();
            }
 */
            var newsItems = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id,
                       pageNumber - 1, pageSize);
            if (category != 0)
            {
                newsItems = _newsService.GetNewsByCategory(pageNumber - 1, pageSize, category);
            }
            newsItem = newsItems.ToList();
            model.PagingFilteringContext.LoadPagedList(newsItems);
            model.NewsItems = newsItem
                .Select(x =>
                {
                    var newsModel = new NewsItemModel();
                    PrepareNewsItemModel(newsModel, x, true);
                    return newsModel;
                })
                .ToList();

            return Ok(model);
        
        }
        [HttpGet]
        [ActionName("details")]
        public IHttpActionResult details([FromUri]int id) {
            var news=_newsService.GetNewsById(id);
            if (news == null)
                return Ok(new { success = false, msg = "文章不存在" });

            var model = new NewsItemModel();
            PrepareNewsItemModel(model, news, true);

            
            return Ok(model);
        }
    }
}
