#region

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

#endregion

namespace Comdiv.MVC.Filters{
    public class LogFilter : Filter{
        protected override bool OnBeforeAction(IEngineContext context, IController controller,
                                               IControllerContext controllerContext){
            var mvc = MvcContext.Create((Controller) controller);
            var logname = acl.token(mvc).Replace("/", ".").Substring(1);
            var log = logger.get(logname);
            log.info(() => myapp.usrName + "\t\tBEFORE_ACTION:\t" + acl.token(mvc));
            return true;
        }

        protected override void OnAfterAction(IEngineContext context, IController controller,
                                              IControllerContext controllerContext){
            var mvc = MvcContext.Create((Controller) controller);
            var logname = acl.token(mvc).Replace("/", ".").Substring(1);
            var log = logger.get(logname);
            log.info(() => myapp.usrName + "\t\tAFTER_ACTION:\t" + acl.token(mvc));
        }

        protected override void OnAfterRendering(IEngineContext context, IController controller,
                                                 IControllerContext controllerContext){
            var mvc = MvcContext.Create((Controller) controller);
            var logname = acl.token(mvc).Replace("/", ".").Substring(1);
            var log = logger.get(logname);
            log.info(() => myapp.usrName + "\t\tAFTER_RENDER:\t" + acl.token(mvc));
        }
    }
}