using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Models.Customer;
using Nop.Plugin.Misc.WebApiServices.Models;
namespace Nop.Plugin.Misc.WebApiServices.Controllers
{
    public class AddressEditModel : CustomerAddressEditModel
    {
        private LoginStatus _login;
        public int addressId{set;get;}
        public LoginStatus login { set { _login = value; } get { return _login; } }
    }
}