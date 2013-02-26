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

namespace Comdiv.Web{
    public static class ControllerExtensions{
        public static bool Existed(this Controller controller, string paramName){
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
    }
}