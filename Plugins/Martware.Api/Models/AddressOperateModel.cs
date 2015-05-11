using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Misc.WebApiServices.Models
{
    public class AddressOperateModel
    {
        private LoginStatus _login;
        public int addressId { set; get; }
        public LoginStatus login { set { _login = value; } get { return _login; } }
    }
}