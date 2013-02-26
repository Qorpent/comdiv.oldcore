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
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Extensions{
    public static class ControllerExtensions{
        public static bool existed(this Controller controller, string paramName){
            if (null == controller){
                return false;
            }
            return paramName.isIn(controller.Params.AllKeys);
        }

        public static bool NonEmpty(this Controller controller, string paramName){
            if (null == controller){
                return false;
            }
            return controller.Params[paramName].hasContent();
        }


        public static Controller bind<T>(this Controller controller, string paramName){
            return bind(controller, paramName, default(T));
        }

        public static Controller bind(this Controller controller, string paramName){
            return bind(controller, paramName, "");
        }


        public static Controller bind<T>(this Controller controller, string paramName, T defValue){
            if (!controller.PropertyBag.Contains(paramName)){
                var value = get<T>(controller, paramName);
                if (Equals(default(T), value)){
                    value = defValue;
                }
                controller.PropertyBag[paramName] = value;
            }
            return controller;
        }

        public static T get<T>(this Controller controller, string paramName){
            if (controller.Params.AllKeys.Contains(paramName)){
                return controller.Params[paramName].to<T>();
            }
            return default(T);
        }
    }
}