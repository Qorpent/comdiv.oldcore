using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;

namespace Comdiv.MVC
{
    public class UrlHelper
    {
        public static string Extension = "rails";
        public static string Build(string root, string controller, string action, params string[] query){

            if (!root.EndsWith("/")) root += "/";
            var main = root + controller  +"/" + action + "." + Extension;
 
            if(query.yes()){
                main += "?" + query.concat("&");
            }
            return main;
        }
    }
}
