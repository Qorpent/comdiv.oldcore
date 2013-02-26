using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public static class MvcAthorizationExtension{
        private static IInversionContainer _container;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(MvcAthorizationExtension)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        public static bool authorize(this IMvcContext descriptor){
            var implicite = descriptor.impliciteAuthorization();
            if (implicite.HasValue){
                return implicite.Value;
            }
            foreach (var authorizer in Container.all<IAuthorizer>().OrderBy(x => x.Idx)){
                var result = authorizer.Authorize(descriptor);
                if (result.HasValue){
                    return result.Value;
                }
            }
            return acl.get(acl.token(descriptor), "exec", "", descriptor.User, true);
        }

        public static bool? impliciteAuthorization(this IMvcContext descriptor){
            bool isadmin = myapp.roles.IsAdmin(descriptor.User);
            if (isadmin)
            {
                return true;
            }
            
            if (null == descriptor.Controller){
                return null;
            }
            var type = descriptor.Controller.GetType();
            var actionMethod = type.GetMethod(descriptor.Action,
                                              BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public |
                                              BindingFlags.InvokeMethod);

            var rolea = actionMethod.getFirstAttribute<RoleAttribute>()
                    ?? 
                    type.getFirstAttribute<RoleAttribute>();


            if(rolea!=null){
                foreach (var r in rolea.Role.split()){
                    if( myapp.roles.IsInRole(descriptor.User, r)){
                        return true;
                    }
                }
                return false;
                
            }

            IDictionary<string, IDynamicAction> dynamics = null;
            IDynamicAction dynamic = null;
            if (descriptor.Controller is Controller){
                try{
                    dynamics = ((Controller) descriptor.Controller).DynamicActions;
                    dynamic = dynamics.ContainsKey(descriptor.Action) ? dynamics[descriptor.Action] : null;
                }
                catch{
                    //the only cause can be testing context with controller and no controllercontext
                }
            }
            var dynattr = dynamic == null
                              ? null
                              : dynamic.GetType().getFirstAttribute<AccessLevelAttribute>();
            if (dynattr == null){
                dynattr = actionMethod == null ? null : actionMethod.getFirstAttribute<AccessLevelAttribute>();
            }


            var resultAccess = AccessLevel.None;
            var attr = dynattr ?? type.getFirstAttribute<AccessLevelAttribute>();
            if (null != attr){
                resultAccess = attr.Level;
            }
            if (resultAccess == AccessLevel.None){
                return null;
            }
            if (resultAccess == AccessLevel.Public){
                return true;
            }
           
            if (resultAccess == AccessLevel.AdminAlways){
                return null;
            }
            return false;
        }
    }
}