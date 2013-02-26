using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Controllers;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Security{
    public class AuthorizeFilter : Filter{
        public static readonly IDictionary<string, bool> cache = new Dictionary<string, bool>();
        private static bool reloadSubscribed;

        public AuthorizeFilter(){
            if (!reloadSubscribed){
                subscribe();
            }
        }

        private static void subscribe(){
            myapp.OnReload += (s, a) => cache.Clear();
            reloadSubscribed = true;
        }


        protected override bool OnBeforeAction(IEngineContext context, IController controller,
                                               IControllerContext controllerContext){
            if (myapp.roles.IsAdmin()){
                return true;
            }
            if (GetAuthorized(context, controllerContext, controller)){
                return true;
            }
            ErrorsController.RedirectToAuthorizeError(context, controller, controllerContext);
            return false;
        }

        public bool GetAuthorized(IEngineContext context, IControllerContext controllerContext, IController controller){
            var mvc = MvcContext.Create((Controller) controller);
            return GetAuthorized(mvc);
        }

        public bool GetAuthorized(IMvcContext descriptor){
            var key = descriptor.ToString();
            return cache.get(key, () => descriptor.authorize(), true);
        }
    }
}