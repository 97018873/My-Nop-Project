using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Misc.WebApiServices.Models
{
    public class AddToCart
    {
        private LoginStatus _login;
        public int productId { set; get; }
        public int shoppingCartTypeId { set; get; }
        public int quantity { set; get; }
        public LoginStatus login { set{_login=value;} get{return _login;} }
    }
}