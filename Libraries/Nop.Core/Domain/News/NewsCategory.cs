using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.News
{
    public class NewsCategory:BaseEntity
    {
  
        public string Name{set;get;}
        public int Fid { set; get; }
        public int Status { set; get; }
        public string DomId { set; get; }

    }
}
