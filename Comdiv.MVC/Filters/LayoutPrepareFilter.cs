#region

using System;
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
using Comdiv.MVC.Rules;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

#endregion

namespace Comdiv.MVC.Filters{
    public class LayoutPrepareFilter : Filter{
        protected override void OnAfterAction(IEngineContext context, IController controller,
                                              IControllerContext controllerContext){
           
			if(controllerContext.Action.Contains("login")) {
				return;
			}
		    if (context.Response.WasRedirected){
                return;
            }

            var c = controller as Controller;
            if (c == null){
                return;
            }

            var logger = c.Logger;

            if (logger != null){
                logger.Debug("LayoutPrepareFilter.OnAfterAction");
            }

            if (c.Contains("asworkspace")){
                c.LayoutName = "workspace";
            }

            if (Array.IndexOf(c.Params.AllKeys, "setlayout") != -1){
                c.LayoutName = c.Params["setlayout"];
                if (c.LayoutName == "_null_"){
                    c.LayoutName = null;
                    return;
                }
            }

            if (c.LayoutName.hasContent() && c.LayoutName.StartsWith("$")){
                c.LayoutName = c.LayoutName.Substring(1);
                return;
            }

            if (c.LayoutName != null && Array.IndexOf(c.Params.AllKeys, "ajax") != -1)
            {
                if(!c.LayoutName.Contains("ajax")) {
                    c.LayoutName = null;
                }
            }

        }
    }
}