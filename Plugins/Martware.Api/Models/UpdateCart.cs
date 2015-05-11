using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Misc.WebApiServices.Models
{
    public class UpdateCart
    {
        private LoginStatus _login;
        public string cartids{set;get;}
        public string quantity { set; get; }
        public LoginStatus login { set { this._login = value; } get { return this._login; } }
    }
}