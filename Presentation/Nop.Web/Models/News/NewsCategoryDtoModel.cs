using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.News
{
    public class NewsCategoryDtoModel : BaseNopModel
    {
  
        public string Name{set;get;}
        public int Fid { set; get; }
        public int Status { set; get; }

    }
}
