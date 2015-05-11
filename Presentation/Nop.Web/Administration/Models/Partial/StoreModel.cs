using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Stores
{
    public partial class StoreModel
    {
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.AppVersion")]
        public string AppVersion { set; get; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.UpdateUrl")]
        public string UpdateUrl { set; get; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Details")]
        public string Details { set; get; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.IsForce")]
        public bool IsForce { set; get; }

    }
 
}