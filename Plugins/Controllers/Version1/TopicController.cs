using System;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Topics;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Topics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Nop.Core.Plugins;
using Nop.Web.Extensions;
using Nop.Web.Framework.Kendoui;


namespace Nop.Plugin.Misc.WebApiServices.Controllers.Version1
{
    public class TopicController :BaseApiController
    {
        [ActionName("test")]
        public string test() {

            return "1231";
        }
        
    }
}