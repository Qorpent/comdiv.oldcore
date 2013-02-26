using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Descriptors;
using Castle.MonoRail.Framework.Providers;
using Castle.MonoRail.Framework.Services.Utils;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Core;
using Comdiv.Persistence;
using Comdiv.Reporting;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Useful;

namespace Comdiv.MVC.Controllers{
    [Admin]
    [Filter(ExecuteWhen.AfterAction, typeof (ExternalPartitionsFilter))]
    public class SysController : BaseController{
      

        [Public]
        public void state(){
            PropertyBag["restart"] = AppRestartTimer.Started;
            PropertyBag["time"] = AppRestartTimer.RestartTime - DateTime.Now;
        }


        [Admin]
        public void viewstats() {

            var boove = BooViewEngine.FirstInstance;
            var list = boove.Statistics.GetAll().ToList();
            list.Add(boove.Statistics.GetTotal());
            PropertyBag["stats"] = list.ToArray();
        }

        [Admin]
        public void getroles(string  usr) {
            RenderText(myapp.roles.GetRoles(usr.toPrincipal()).concat(", "));
        }

        [Admin]
        public void isinrole(string usr,string role)
        {
            RenderText(myapp.roles.IsInRole(usr.toPrincipal(),role).toStr());
        }

        [Admin]
        public void tasks(string view, int id) {
            PropertyBag["tasks"] = myapp.Longtasks.OrderByDescending(x=>x.Id).ToArray();
            PropertyBag["id"] = id;
            PropertyBag["task"] = myapp.Longtasks.FirstOrDefault(x => x.Id == id);
            if(view.hasContent()) {
                SelectedViewName = view;
            }
        }

        [Admin]
        public void cleanuptasks()
        {
            foreach (var longTask in myapp.Longtasks.ToArray()) {
                if(longTask.Terminated) {
                    myapp.Longtasks.Remove(longTask);
                }
                else if(longTask.HaveToTerminate && (DateTime.Now - longTask.Start).TotalMinutes > 10) {
                    myapp.Longtasks.Remove(longTask);
                }
            
            }
            RenderText("OK");
        }

        [Admin]
        public void terminate(int id) {
            var task = myapp.Longtasks.FirstOrDefault(x => x.Id == id);
            task.HaveToTerminate = true;
            RenderText("OK");
        }


        public void echo(){
            PropertyBag["params"] = Request.Params.AllKeys.Select(x => new Entity(x, Request.Params[x]));
            PropertyBag["headers"] = Request.Headers.AllKeys.Select(x => new Entity(x, Request.Headers[x]));
            PropertyBag["url"] = Request.Url;
            RenderView("/sys/echo");
        }

        [Admin]
        [ActionDescription(ActionRole.Admin, ActionSeverity.SecurityLack, "Выводит перечень расширений системы")]
        public void Extensions(){
            PropertyBag["reg"] = ScriptMachine.Registry;
        }

        [Admin]
        [ActionDescription(ActionRole.Admin, ActionSeverity.DataLoss, "Перегружает систему")]
        public void  Restart(int minutes){
            CancelView();
            if (0 == minutes){
                HttpRuntime.UnloadAppDomain();
            }
            else{
                AppRestartTimer.Start(minutes);
            }
        }

        [Admin]
        [ActionDescription(ActionRole.Admin, ActionSeverity.DataLoss, "Перегружает систему")]
        public void CancelRestart(){
            CancelView();
            AppRestartTimer.Stop();
        }

        [Admin]
        [ActionDescription(ActionRole.Admin, ActionSeverity.DataLoss, "Очищает кэши")]
        public void Clear(){
            myapp.reload();
            RenderText("<h1>Состояние системы сброшено</h1>");
          //  GC.Collect();
        }

		[Admin]
		[ActionDescription(ActionRole.Admin, ActionSeverity.DataLoss, "Очищает кэши всех приложений")]
		public void ClearGlobal()
		{
			myapp.reload();
			myapp.SetGlobalLastRefresh();
			myapp.SetGlobalLastDataRefresh();
			RenderText("<h1>Состояние системы сброшено</h1>");
			//  GC.Collect();
		}
        

        [Admin]
        [ActionDescription(ActionRole.Admin, ActionSeverity.DataLoss, "Очищает кэши")]
        public void ClearReportCache(){
            Container.get<IReportCache>().Clear();
            RenderText("<h1>Состояние системы сброшено</h1>");
        }

        [Admin]
        [ActionDescription(ActionRole.Admin, ActionSeverity.SecurityLack, "Выводит список сессий")]
        public void Sessions(){
            PropertyBag["sessions"] = myapp.conversation.GetActiveSnapshot();
        }

        [Admin]
        [ActionDescription(ActionRole.Admin, ActionSeverity.SecurityLack, "Выводит список активных пользователей")]
        public void ActiveUsers(int minutes){
            if(0==minutes){
                minutes = 10;
            }
            Func<string,int> formcount = u=> myapp.conversation.GetActiveSnapshot().Where(
                                                 x => x.Code.Contains("zeta.form") && x.Owner.ToLower() == u.ToLower()).
                                                 Count();
            PropertyBag["items"] = myapp.getActiveUsers(minutes);
            PropertyBag["formcount"] = formcount;
            
        }
      
       


        /// <summary>
        /// Выводит список контроллеров приложения
        /// </summary>
        [Admin]
        [ActionDescription(
            ActionRole.Admin,
            ActionSeverity.SecurityLack,
            "Выводит перечень контроллеров системы с указанием всех действий")]
        public void Controllers(){
            var controllers =
                Container.all<Controller>().Select(c => ControllerInspectionUtil.Inspect(c.GetType())).OrderBy(
                    c => c.Name);
            var descb = new DefaultControllerDescriptorProvider();
            Func<ControllerDescriptor, ControllerMetaDescriptor> desc =
                c =>
                ((IControllerDescriptorProvider) Context.GetService(typeof (IControllerDescriptorProvider))).
                    BuildDescriptor(
                    c.ControllerType);
            Func<MethodInfo, string> getsyg = i =>
                                              i.toSygnatureString();
            Func<MethodInfo, ActionDescriptionAttribute> getmeta = i =>{
                                                                       var descs =
                                                                           i.GetCustomAttributes(
                                                                               typeof (ActionDescriptionAttribute),
                                                                               true)
                                                                               .Cast
                                                                               <ActionDescriptionAttribute>().ToList
                                                                               ();
                                                                       if (descs.yes()){
                                                                           return descs[0];
                                                                       }
                                                                       return
                                                                           new ActionDescriptionAttribute(
                                                                               "нет данных");
                                                                   };
            Func<IDynamicActionHandler, ActionDescriptionAttribute> getvmeta = i =>{
                                                                                   var descs =
                                                                                       i.GetType().
                                                                                           GetCustomAttributes(
                                                                                           typeof (
                                                                                               ActionDescriptionAttribute
                                                                                               ), true)
                                                                                           .Cast
                                                                                           <
                                                                                               ActionDescriptionAttribute
                                                                                               >().ToList();
                                                                                   if (descs.yes()){
                                                                                       return descs[0];
                                                                                   }
                                                                                   return
                                                                                       new ActionDescriptionAttribute
                                                                                           ("нет данных");
                                                                               };
            PropertyBag["controllers"] = controllers;
            PropertyBag["dynamics"] =
                Container.all<IDynamicActionHandler>();
            PropertyBag["desc"] = desc;
            PropertyBag["getsyg"] = getsyg;
            PropertyBag["getmeta"] = getmeta;
            PropertyBag["getvmeta"] = getvmeta;
        }
    }
}