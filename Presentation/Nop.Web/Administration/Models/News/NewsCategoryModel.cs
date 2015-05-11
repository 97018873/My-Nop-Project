using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.News;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.News
{
    [Validator(typeof(NewsCategoryValidator))]
    public class NewsCategoryModel : BaseNopEntityModel
    {
        public NewsCategoryModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategoryItems.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategoryItems.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategoryItems.Fields.ParentName")]
        public int ParentCategoryId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategoryItems.Fields.ParentName")]
        public string ParentCategoryName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategoryItems.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategoryItems.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsCategoryItems.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
    }
}