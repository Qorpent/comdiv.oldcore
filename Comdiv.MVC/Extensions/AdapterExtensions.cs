using System.Collections;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Utils;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Extensions{
    public static class AdapterExtensions{
        public static IDictionary AsDictionary(this HttpSessionState session){
            return new SessionAsDictionary(session);
        }

        public static IDictionary GetSession(this IMvcContext descriptor){
            var ishttp = HttpContext.Current.yes();
            var ismonorail = descriptor.Controller is Controller;
            if (!(ishttp || ismonorail)){
                return null;
            }
            if (ismonorail){
                return ((Controller) descriptor.Controller).Context.Session;
            }
            return HttpContext.Current.Session.AsDictionary();
        }
    }
}