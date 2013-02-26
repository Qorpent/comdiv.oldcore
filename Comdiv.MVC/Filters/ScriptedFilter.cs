using System;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensibility;
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

namespace Comdiv.Web{
    public class ScriptedFilter : Filter{
        protected override bool OnBeforeAction(IEngineContext context, IController controller,
                                               IControllerContext controllerContext){
            if (!(controller is IWithScriptExtensions)){
                return true;
            }
            var c = controller as Controller;
            var extc = (IWithScriptExtensions) controller;
            var controllerResult = new FilterState();
            try{
                //extc.ScriptMachine.Reload();
                extc.ExecuteExtenders("(?i)^{0}.filter.onbeforeaction", controllerResult);
                if (controllerResult.Executed && !controllerResult.ReturnValue){
                    return false;
                }
                extc.ExecuteExtenders(string.Format("(?i)^{{0}}.action.{0}.before.", c.Action));
                return true;
            }
            catch (Exception ex){
                ErrorsController.RedirectToException(context, controller, ex);
                return false;
            }
        }

        protected override void OnAfterAction(IEngineContext context, IController controller,
                                              IControllerContext controllerContext){
            if (context.Response.WasRedirected){
                return;
            }
            var c = controller as Controller;
            if (!(controller is IWithScriptExtensions)){
                return;
            }
            var extc = (IWithScriptExtensions) controller;
            try{
                extc.ExecuteExtenders(string.Format("(?i)^{{0}}.action.{0}.after.", c.Action));
                extc.ExecuteExtenders("^{0}.filter.onafteraction");
            }
            catch (Exception ex){
                ErrorsController.RedirectToException(context, controller, ex);
            }
        }
    }
}