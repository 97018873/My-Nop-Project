using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.WebApiServices.Security;
using Nop.Services.Topics;
using Nop.Services.Security;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Nop.Plugin.Misc.WebApiServices.Controllers
{
    public class TopicController : BaseApiController
    {
        // GET api/<controller>
        private readonly ITopicService _topicService;
        private readonly IPermissionService _permissionSettings;

        public TopicController(ITopicService topicService,IPermissionService permissionSettings) {
            this._topicService = topicService;
            this._permissionSettings = permissionSettings;
        }

        [ActionName("details")]
        [HttpGet]
        public Topic TopicDetails(int id) {
            return _topicService.GetTopicById(id);
        }
        [ActionName("test")]
        [HttpGet]
        public string Test() {

            return "asdf";
        }
    }
}