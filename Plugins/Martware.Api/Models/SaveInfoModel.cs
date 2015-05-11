using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;

namespace Nop.Plugin.Misc.WebApiServices.Models
{
    public class SaveInfoModel
    {
        private FormDataCollection _form;
        public FormDataCollection form { set { this._form = value; } get { return this._form; } }
    }
}