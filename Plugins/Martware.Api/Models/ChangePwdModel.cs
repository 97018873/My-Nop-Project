using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.WebApiServices.Models
{
    public class ChangePwdModel : ChangePasswordModel
    {
        private LoginStatus _login;
        public LoginStatus login { set { _login = value; } get { return _login; } }
    }
}