#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Comdiv.MVC.Filters;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Qorpent.Log;
using Qorpent.Mvc;

#endregion

namespace Comdiv.MVC.Controllers{
    [Public]
    [ControllerDetails("errors", Area = "sys")]
    [Filter(ExecuteWhen.Always, typeof (LogFilter), ExecutionOrder = 1000)]
    public class ErrorsController : SmartDispatcherController, IRescueController{
        private readonly ILog log = logger.get(typeof (ErrorsController));
		public ErrorsController () {
			this.QorpentLog = myapp.QorpentApplication.LogManager.GetLog(this.GetType().FullName + ";comdiv.mvc;MvcHandler", this);
		}

	    protected IUserLog QorpentLog { get; set; }

	    #region IRescueController Members

        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.NonCritical,
            "вывод сообщения об ошибке внутри вида"
            )]
        public void Rescue(Exception exception, IController controller, IControllerContext controllerContext) {
	        writeQorpentLog(exception,controller,controllerContext);
            if(exception.InnerException.Message.Contains("noerrc")) {
				


                CancelView();
                CancelLayout();
                ((WindsoreInversionContainer)myapp.ioc).Container.Release(this);
                ((WindsoreInversionContainer)myapp.ioc).Container.Release(controller);
                return;
            }

            var c = controller as Controller;
            var parameters = string.Join("&", new[]{c.Form.ToString(), c.Query.ToString()});

            PrepareCommon(controllerContext.AreaName, controllerContext.Name, controllerContext.Action, parameters,
                          myapp.usrName);
            PropertyBag["exception"] = exception;
        }

		private void writeQorpentLog(Exception exception, IController controller, IControllerContext controllerContext)
		{
			var logmessage = new LogMessage();
			logmessage.ApplicationName = myapp.QorpentApplication.ApplicationName;
			var dict = new Dictionary<string, string>();
			foreach (var param in this.Params.AllKeys)
			{
				dict[param] = this.Params[param];
			}
			var actionref = (controllerContext.AreaName.hasContent()? (controllerContext.AreaName+ "."):"") + controllerContext.Name + "." + controllerContext.Action;
			logmessage.MvcCallInfo = new MvcCallInfo { ActionName = actionref, Parameters = dict, RenderName = "monorail",
				 Url = Context.Request.Uri.ToString() };
			logmessage.Server = Environment.MachineName;
			logmessage.HostObject = controller;
			logmessage.Level = LogLevel.Error;
			logmessage.Message = "Ошибка при выполнении " + actionref;
			logmessage.Time = DateTime.Now;
			logmessage.Error = exception;
			logmessage.User = myapp.usrName;

			QorpentLog.Error("", logmessage, controller);
	    }

	    #endregion

        public static void RedirectToAuthorizeError(IEngineContext context, IController controller,
                                                    IControllerContext controllerContext){
            RedirectToAuthorizeError(context, controller, controllerContext, "");
        }


        public static void RedirectToAuthorizeError(IEngineContext context, IController controller,
                                                    IControllerContext controllerContext, string message){
            var c = controller as Controller;
            var parameters = new NameValueCollection{};
            parameters["area"] = c.AreaName;
            parameters["controller"] = c.Name;
            parameters["action"] = c.Action;
            parameters["message"] = message;
            var p = string.Join("&", new[]{c.Form.ToString(), c.Query.ToString()});
            if (p.StartsWith("&")){
                p = p.Remove(0, 1);
            }
            parameters["parameters"] = p;
            parameters["user"] = myapp.usrName;
            c.Redirect("sys", "errors", "unauthorized", parameters);
        }

        [Public]
        public void ShowErrors(){
            PropertyBag["errors"] = myapp.errors.Errors.ToArray();
        }
        [Public]
        public void RemoveError(int idx){
            myapp.errors.RemoveError(idx);
            RedirectToAction("ShowErrors");
        }

        public static void RedirectToException(IController controller, Exception exception){
            RedirectToException(((Controller) controller).Context, controller, exception);
        }

        public static void RedirectToException(IEngineContext context, IController controller, Exception exception){
            var c = controller as Controller;
            c.Flash["exception"] = exception;
            var parameters = new NameValueCollection{};
            parameters["area"] = c.AreaName;
            parameters["controller"] = c.Name;
            parameters["action"] = c.Action;
            var p = string.Join("&", new[]{c.Form.ToString(), c.Query.ToString()});
            if (p.StartsWith("&")){
                p = p.Remove(0, 1);
            }
            parameters["parameters"] = p;
            parameters["user"] = myapp.usrName;

            c.Redirect("sys", "errors", "exception", parameters);
        }

        /// <summary>
        /// Выводит стандартное сообщение о неавторизовнном доступе с ссылкой "повторить"
        /// </summary>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="parameters"></param>
        /// <param name="user"></param>
        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.NonCritical,
            "Сообщение об ошибке авторизации"
            )]
        public void Unauthorized(string area, string controller, string action, string parameters, string user){
            log.Warn("User {0} not authorized ConvertTo {1}/{2}/{3}?{4}", user, area, controller, action,
                     parameters);
            PrepareCommon(area, controller, action, parameters, user);
        }

        private void PrepareCommon(string area, string controller, string action, string parameters, string user){
            PropertyBag["area"] = area;
            PropertyBag["controller"] = controller;
            PropertyBag["action"] = action;
            PropertyBag["parameters"] = parameters;
            PropertyBag["user"] = user;
        }

        [ActionDescription(
            ActionRole.Public,
            ActionSeverity.NonCritical,
            "Сообщение об исключении в приложении"
            )]
        public void Exception(string area, string controller, string action, string parameters, string user){
            log.Error(
                string.Format("Exception occured  {1}/{2}/{3}?{4} ({0})", user, area, controller, action, parameters),
                Flash["exception"] as Exception);
            PrepareCommon(area, controller, action, parameters, user);
            PropertyBag["exception"] = Flash["exception"];
        }
    }
}