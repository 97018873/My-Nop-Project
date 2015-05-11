using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Core.Domain.News;

namespace Nop.Web.Models.Catalog
{
    public partial class TopMenuModel : BaseNopModel
    {
        public TopMenuModel()
        {
            Categories = new List<CategorySimpleModel>();
        }

        public IList<CategorySimpleModel> Categories { get; set; }
        public IList<NewsCategory> NewsCategories { get; set; }
        public bool BlogEnabled { get; set; }
        public bool RecentlyAddedProductsEnabled { get; set; }
        public bool ForumEnabled { get; set; }
    }
}