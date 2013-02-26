#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using Castle.MonoRail.Framework;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Controllers;
using Comdiv.MVC.Report;
using Comdiv.MVC.Security;
using Comdiv.Persistence;
using Comdiv.Reporting;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Useful;
using NHibernate;

    #endregion

namespace Comdiv.Web{

	public class ReportLogger : Filter {
		protected override bool OnBeforeAction(IEngineContext context, IController controller, IControllerContext controllerContext) {
			return start(HttpContext.Current);
		}

		public bool start(HttpContext ctx) {
			try {
				var dict = new Dictionary<string, object>();
				dict["machine"] = Environment.MachineName;
				dict["usr"] = myapp.usrName;
				dict["app"] = ctx.Request.ApplicationPath;
				dict["time"] = ctx.Timestamp + "_" + (ctx.Timestamp - DateTime.Today).TotalMilliseconds;
				dict["url"] = ctx.Request.Url.ToString();
				using (var c = myapp.ioc.getConnection()) {
					c.WellOpen();
					c.ExecuteNonQuery("exec comdiv.report_start_log", dict);
				}
				return true;
			}catch {
				return true;
			}
		}

		protected override void OnAfterRendering(IEngineContext context, IController controller, IControllerContext controllerContext) {
			finish(HttpContext.Current, null);
		}

		public void finish(HttpContext ctx,Exception ex) {
			try {
				var dict = new Dictionary<string, object>();
				dict["machine"] = Environment.MachineName;
				dict["time"] = ctx.Timestamp.ToString() + "_" + (ctx.Timestamp - DateTime.Today).TotalMilliseconds;
				dict["app"] = ctx.Request.ApplicationPath;
				dict["error"] = ex == null ? "" : ex.Message + (ex.InnerException == null ? "" : ":: " + ex.InnerException.Message);
				using (var c = myapp.ioc.getConnection()) {
					c.WellOpen();
					c.ExecuteNonQuery("exec comdiv.report_finish_log", dict);
				}
			}catch {
				
			}
		}
	}

    [ControllerDetails("report", Sessionless = true)]
    [Rescue(typeof (ErrorsController), "Rescue")]
    [Public]
    //[Filter(ExecuteWhen.Always, typeof (ReportOperationLogFilter), ExecutionOrder = -1000)]
	
    public class ReportController : BaseController{
        public IReportControllerExtension ReportHandler { get; set; }
        public ReportDelegate start;
        //public StandardReportController() {
        //    start = (ReportDelegate)reportRender;
        //}

        [ActionDescription(
            ActionRole.User,
            ActionSeverity.NonCritical,
            @"≈сли настроено в расширении выводит форму подготовки отчета"
            )]
        public void Prepare(string reportCode){
            PrepareReportHandler(reportCode);
            ReportHandler.ControlPreparation(this);
        }


        public delegate void ReportDelegate(
            string reportCode, IPrincipal user, IControllerContext controllerContext, IEngineContext engineContext,
            ISession session);

#if !MONO
        [ActionDescription(
            ActionRole.User,
            ActionSeverity.Dangerous,
            @"≈сли настроено в расширении - формирует отчет - опасность метода в том, что<br/>
на уровне отчетной системы не производитс€ никакого дополнительного контрол€ полномочий пользовател€<br/>
по доступу к данным, не контроллируютс€ параметры, reportController должен использоватьс€ только формирование<br/>
reportRequest в защищенном контексте - то есть через субконтроллер, в авторизации<br/>
должен остатьс€ открытм но с контроллем на параметр requestKey"
            )]
        public void EndRender(){
			
			try {
				
				;
				render.EndInvoke(ControllerContext.Async.Result);
				Conversation.Data["report_extension"] = ReportHandler;
				Conversation.Data["report_definition"] = ReportHandler.Definition;
				ReportHandler.CheckReportLive();
				ReportHandler.RenderReport(this);
				Conversation.Finish();
				this.task.Terminate();
				new ReportLogger().finish(ctx, null);
			}catch(Exception ex) {
				new ReportLogger().finish(ctx, ex);
				throw;
			}
        }

        public void Preview() {
            var c = Conversation;
            Conversation.Data["current_user"] = myapp.usr;
            Conversation.Data["context"] = System.Web.HttpContext.Current;
            this.task = new LongTask
            {
                Comment = Params["tcode"],
                User = myapp.usrName,
                Start = DateTime.Now,
                Type = "preview",
                Context = System.Web.HttpContext.Current,
            }
            ;

            myapp.Longtasks.Add(task);

            myapp.principals.BasePrincipal = c.Data["current_user"] as IPrincipal;
            myapp.conversation.Enter(c.Code);
            IoExtensions.CurrentHttp = (HttpContext)c.Data["context"];
            logger.Debug("enter " + IoExtensions.CurrentHttp.Request.RawUrl);
            myapp.storage.GetDefault().SetCacheMode(true);
            PrepareReportHandler(null);
            logger.Debug("start " + IoExtensions.CurrentHttp.Request.RawUrl);
            ReportHandler.PreviewMode = true;

            ReportHandler.ControlGeneration(this, false);
            logger.Debug("leave " + IoExtensions.CurrentHttp.Request.RawUrl);
            
            Conversation.Data["report_extension"] = ReportHandler;
            Conversation.Data["report_definition"] = ReportHandler.Definition;
            ReportHandler.CheckReportLive();
            ReportHandler.RenderReport(this);
            Conversation.Finish();
            this.task.Terminate();
        }

        private ILog logger = Logging.logger.get("reporttrace");
        private void _render(IConversation state){
            // Thread.Sleep(10000);
            
            var c = state;
            myapp.principals.BasePrincipal = c.Data["current_user"] as IPrincipal;
            myapp.conversation.Enter(c.Code);
            IoExtensions.CurrentHttp = (HttpContext) c.Data["context"];
            logger.Debug("enter "+IoExtensions.CurrentHttp.Request.RawUrl);
            myapp.storage.GetDefault().SetCacheMode(true);
            PrepareReportHandler(null);
            logger.Debug("start " + IoExtensions.CurrentHttp.Request.RawUrl);
            ReportHandler.ControlGeneration(this, false);
            logger.Debug("leave " + IoExtensions.CurrentHttp.Request.RawUrl);
			
            
        }

        private Action<IConversation> render;
        private static IUniqueStringProvider conversationNames = new RandomStringUniqueStringProvider { Size = 3 };
        private LongTask task;
    	private HttpContext ctx;

    	public IAsyncResult BeginRender() {
        	new ReportLogger().start(System.Web.HttpContext.Current);

            render = _render;
            this.task = new LongTask
                            {
                                Comment = Params["tcode"],
                                User = myapp.usrName,
                                Start = DateTime.Now,
                                Type = "report",
                                Context =  System.Web.HttpContext.Current,
                            }
            ;
            
            myapp.Longtasks.Add(task);

            Conversation.Data["current_user"] = myapp.usr;
            Conversation.Data["context"] = System.Web.HttpContext.Current;
        	this.ctx = System.Web.HttpContext.Current;
            return render.BeginInvoke(Conversation, ControllerContext.Async.Callback,
                                      ControllerContext.Async.State);
        }
#else
		[ActionDescription(
            ActionRole.User,
            ActionSeverity.Dangerous,
            @"≈сли настроено в расширении - формирует отчет - опасность метода в том, что<br/>
на уровне отчетной системы не производитс€ никакого дополнительного контрол€ полномочий пользовател€<br/>
по доступу к данным, не контроллируютс€ параметры, reportController должен использоватьс€ только формирование<br/>
reportRequest в защищенном контексте - то есть через субконтроллер, в авторизации<br/>
должен остатьс€ открытм но с контроллем на параметр requestKey"
            )]
        public void Render(){
			Persister.StartLoadCache();
            PrepareReportHandler(null);
            ReportHandler.ControlGeneration(this,false);
			ReportHandler.RenderReport(this);
			Conversation.Finish(false);
        }

#endif

        private void PrepareReportHandler(string reportCode){
            
            var reportName = reportCode + ".report";
            IReportControllerExtension extension = null;
            try{
                extension = Container.get<IReportControllerExtension>(reportName);
            }
            catch (Exception){
                extension = Container.get<IReportControllerExtension>("default.report");
            }

            var def = ReportSetHelper.GetAll().ToArray().FirstOrDefault(rd => reportCode == rd.Code);
            extension.Task = this.task;

            extension.Definition = def;


            ReportHandler = extension;
            if (null == ReportHandler){
                throw new Exception("Ќет отчета с именем " + reportCode);
            }

            if (null != ReportHandler){
                Helpers.Add("Report", ReportHandler);
            }
        }

        
        public void getparamhelp(string code) {
            var content = myapp.files.Read("doc/reportparameters/" + code + ".html");
            content = makeparamWiki(content);
            PropertyBag["content"] = content;
            PropertyBag["code"] = code;
            PropertyBag["parameter"] = new ReportParametersRepository().Get(code);
        }

        private string makeparamWiki(string content) {
            return content.replace(@"\[\[(?<code>[\w\d]+)\]\]", m =>
                                                                    {
                                                                        var code = m.Groups["code"].Value;
                                                                        var parameter =
                                                                            new ReportParametersRepository().Get(code);
                                                                        if(parameter.HasHelp()) {
                                                                            return
                                                                                string.Format(
                                                                                    @"<span class=""showlink"" title=""ќткрыть справку"" onclick=""zeta.report.getparamdoc('{0}')"">{0} ({1})</span>",
                                                                                    code, parameter.Name);
                                                                                
                                                                        }else {
                                                                            if(myapp.roles.IsInRole("ADMIN") || myapp.roles.IsInRole("DOCWRITER")) {
                                                                               return string.Format(
                                                                                    @"<span class=""createlink"" title=""—оздать справку"" onclick=""zeta.report.editparamdoc('{0}')"">{0} ({1})</span>",
                                                                                    code, parameter.Name);
                                                                            }else {
                                                                              return  string.Format(
                                                                                    @"<span class=""notexisted"" title=""—правка на данный момент отсутствует"">{0} ({1})</span>",
                                                                                    code, parameter.Name);
                                                                            }
                                                                        }
                                                                    }, RegexOptions.Compiled);
        }

        [Role("ADMIN,DOCWRITER")]
        public void setparamhelp(string code,string  content) {
            var p = new ReportParametersRepository().Get(code);
            myapp.files.Write("~/"+p.Level+"/doc/reportparameters/" + code + ".html", content);
            RenderText("OK");
        }
        [Role("ADMIN,DOCWRITER")]
        public void paramhelpeditor(string code) {
            PropertyBag["content"] = myapp.files.Read("doc/reportparameters/" + code + ".html");
            PropertyBag["code"] = code;
            PropertyBag["parameter"] = new ReportParametersRepository().Get(code);
            PropertyBag["advancedScripts"] = new[]{
                    "yui/utilities/utilities",
                    "yui/container/container",
                    "yui/menu/menu",
                    "yui/button/button",
                    "yui/editor/editor-beta",
                    "editor"};
            PropertyBag["advancedCss"] = new[]{
                    "scripts/yui/fonts/fonts-min",
                    "scripts/yui/container/assets/skins/sam/container",
                    "scripts/yui/button/assets/skins/sam/button",
                    "scripts/yui/editor/assets/skins/sam/editor",
                    "scripts/yui/yahoo-dom-event/yahoo-dom-event",
                    "scripts/yui/menu/assets/skins/sam/menu"};
        }


        [Layout("workspace")]
        public void savedlist(bool onlymy, string tags){
            WorkbenchControllerExtension.PrepareWorkbench(this);
            var all = Db.All<ISavedReport>()
                .Where(x=> (x.Usr.ToUpper()==myapp.usrName.ToUpper() || x.Shared)
                           ||
                           (myapp.roles.IsAdmin() &&  !onlymy)).ToList();
            if (tags.hasContent()) {
                var ts = tags.split(false, true, ' ');
                all = all.Where(x => {
                    bool proceed = true;
                    foreach (var t in ts) {
                        if (!x.Tag.like(t,System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
                            proceed = false;
                            break;
                        }
                    }
                    return proceed;
                }).ToList();
            }
            PropertyBag["items"] = all;
            PropertyBag["title"] = "ѕеречень отчетов";
        }

    }
}