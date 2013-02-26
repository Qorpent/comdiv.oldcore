// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.WindsorExtension;
using Castle.Windsor;
using Comdiv.Application;
using Comdiv.Caching;
using Comdiv.Cfg;
using Comdiv.Conversations;
using Comdiv.Design;
using Comdiv.Extensibility;
using Comdiv.Extensibility.Boo;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Useful;

namespace Comdiv.MVC{
    public class BaseApplication : HttpApplication, IContainerAccessor{
        private static readonly object initLock = new object();
        protected static IWindsorContainer container;
        private static IUniqueStringProvider conversationNames = new RandomStringUniqueStringProvider{Size = 3};
        private static ILog htlog = logger.get("http");
        private static bool initiated;
        
        private static ILog log = logger.get("comdiv.sys");
        public static Exception startError;
        public static object sync;
        private IInversionContainer _container;
        private string connection = null;
        private Type[] overrides = Type.EmptyTypes;
        private string log2name = "";

        static BaseApplication(){
            sync = new object();
        }

        public BaseApplication(){
            BeginRequest += BaseApplication_BeginRequest;
            PostAuthorizeRequest += BaseApplication_PostAuthorizeRequest;
        }

        public IInversionContainer _Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            _Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        #region IContainerAccessor Members

        public IWindsorContainer Container{
            get { return container; }
        }

        #endregion

    	protected virtual void BaseApplication_PostAuthorizeRequest(object sender, EventArgs e){
            lock (sync){
                dolog("pa-s");
                var url = HttpContext.Current.Request.Url.ToString();
                var isstatechecker = url.ToLower().Contains("sys/state.rails");
                myapp.principals.BasePrincipal = null;
                myapp.storage.GetDefault().SetCacheMode(false);
                var directConversation = HttpContext.Current.Request["__"];
                if (!isstatechecker){
                    myapp.registerEnter();
                }
                if (directConversation.hasContent()){
                    myapp.conversation.Enter(myapp.usrName, directConversation);

                }
                else{
                    myapp.conversation.Enter(myapp.usrName, myapp.usrName+conversationNames.New());
                }
                myapp.conversation.Current.needcleanonleave();
                myapp.conversation.Current.Finished = true;
                dolog("pa-f");
            }
        }

        private void dolog(string state){

            var url = HttpContext.Current.Request.Url.ToString().find(@"\w+/\w+\.rails[\s\S]*$").Replace(".rails", "");
            if (htlog.IsDebugEnabled){
                #if CATCH_PROBLEM
                if (log2name.noContent()){
                    log2name = "~/tmp/http2.log".mapPath();

                }
                
                if(true){
#else
                if (!(url.ToLower().Contains("/state.rails") || url.ToLower().Contains("/showerrors.rails"))){
#endif
                    var usr = "NOUSR";
                    if(HttpContext.Current.User!=null){
                        usr = HttpContext.Current.User.Identity.Name;
                    }
                    
                    var message = string.Format("{1} {4} {3}: {0}:{2}:{5}", usr,
                                                HttpContext.Current.Timestamp.ToString("HHmmssFFF"),
                                                url,state,DateTime.Now.ToString("HH:mm:ss"),
                                                HttpContext.Current.Request.UserHostAddress);
#if CATCH_PROBLEM
                    lock (sync){
                        if (File.Exists(log2name)){
                            if (new FileInfo(log2name).Length > 10000000){
                                File.Delete(log2name);
                            }
                        }
                        File.AppendAllText(log2name, message + Environment.NewLine);
                    }
#else


                    htlog.Debug(message);
#endif
                }
            }
        }

        private void BaseApplication_BeginRequest(object sender, EventArgs e){
            dolog("init-s");
			if(null != myapp.QorpentApplication && myapp.QorpentApplication.ApplicationName.noContent()) {
				myapp.QorpentApplication.ApplicationName = HttpContext.Current.Request.ApplicationPath.Replace("/", "");
			}
			
            if (startError != null){
                var message =
                    @"<h1>ѕри старте приложени€ возникла ошибка</h1>
<p>ƒанное соощение на экране свидетельствует о наличии серьезных проблем сервера, обновите страницу, об€зательно сообщите администратору если 
данное сообщение возникнет снова после обновлени€ страницы
</p>
" +
                    startError.ToString().Replace("\r\n", "<br/>");
                HttpContext.Current.Response.Write(message);
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.Close();
                HttpRuntime.UnloadAppDomain();
                return;
            }
        }

        [TestPropose]
        public BaseApplication Start(params Type[] overrides){
            return Start("test_env", overrides);
        }

        [TestPropose]
        public BaseApplication Start(string connection, params Type[] overrides){
            lock (sync){
				startQorpent();

                this.connection = connection;
                startError = null;
                initiated = false;
                container = null;
                this.overrides = overrides ?? Type.EmptyTypes;
                Application_Start_Back();
                if (startError != null){
                    throw startError;
                }
                return this;
            }
        }

        protected void Application_Start_Back(){
            lock (initLock){
                if (!initiated){
                    
                    startIoc();
                    setupServices();
                    setupControllers();
                    applyIoc();
                    doStartUp();


	         
                    initiated = true;
                }
            }
        }

	    protected void startQorpent() {
		    myapp.QorpentApplication = Qorpent.Applications.Application.Current;
			
	    }

	    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e){
            log.Error("UNHANDLED DOMAIN LEVEL EXCEPTION", (Exception) e.ExceptionObject);
            if (null != HttpContext.Current){
                HttpContext.Current.Response.Write(e.ExceptionObject.ToString());
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.Close();
            }
        }

        protected void Application_Start(object sender, EventArgs e){
            lock (initLock){
                if (!initiated){
                    
                    try{

						startQorpent();

                        var dolog = log.IsInfoEnabled;
                        Action<string> info = s =>{
                                                  if (dolog){
                                                      log.Info(s);
                                                  }
                                              };
                        
                        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                        loadModAssemblies();
                        
                        info("starting ioc");
                        startIoc();
                        info("setupping services");
                        setupServices();
                        info("setupping controllers");
                        setupControllers();
                        info("applying ioc");
                        applyIoc();
                        info("starting application commands");
                        doStartUp();
                        info("launch finished");

                        log2name = "~/tmp/http2.log".mapPath();
                        if(File.Exists(log2name)){
                            File.Delete(log2name);
                        }
                        initiated = true;
                    	myapp.starttime = DateTime.Now;
                    }
                    catch (Exception ex){
                        startError = ex;
                    }
                }
            }
        }

        private void loadModAssemblies(){
            var dir = HttpContext.Current.Server.MapPath("~/mod/bin");
            if(Directory.Exists(dir)){
                foreach (var ass in Directory.GetFiles(dir,"*.dll")){
                    var target = HttpContext.Current.Server.MapPath("~/bin")+"/"+Path.GetFileName(ass);
                    if(!File.Exists(target) || File.GetLastWriteTime(target) < File.GetLastWriteTime(ass)){
                        File.Copy(ass,target,true);
                    }
                }
            }
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name;
            if(!name.EndsWith(".dll")){
                name += ".dll";
            }
            var probe = HttpContext.Current.Server.MapPath("~/mod/bin/" + name);
            if(File.Exists(probe)){
                return Assembly.LoadFrom(probe);
            }
            probe = HttpContext.Current.Server.MapPath("~/bin/" + name);
            if (File.Exists(probe))
            {
                return Assembly.LoadFrom(probe);
            }
            return null;
        }


        protected void Application_EndRequest(){
            lock (sync){
                dolog("end-s");
                if (myapp.conversation.Current != null){
                    myapp.conversation.Leave();
                	if(null!=AutomativeCurrentSessionContext.reached) {
                		if(AutomativeCurrentSessionContext.reached.IsOpen) {
                			AutomativeCurrentSessionContext.reached.Clear();
							AutomativeCurrentSessionContext.reached.Close();
							AutomativeCurrentSessionContext.reached.Dispose();
                			AutomativeCurrentSessionContext.reached = null;
                		}
                	}
                }
                dolog("end-f");
            }
        }

        protected virtual void Application_End(object sender, EventArgs e){

            container.Dispose();
        }

        protected virtual void doStartUp(){
			foreach(var i in _Container.all<IContainerInitializer>())
        	{
        		i.Apply(_Container);
        	}

        
            _Container.get<ISessionFactoryProvider>().Get(null);
            _Container.all<ITask>().Where(t => t.ExecuteWhen == TaskExecuteWhen.OnAppStart)
                .map(t =>{
                         try{
                             t.Execute();
                         }
                         catch (Exception ex){
                             logger.get("comdiv.mvc.init").Error("ќшибка при выполнении задачи " + t.Name, ex);
                         }
                     }
                );
            foreach (var starter in _Container.all<IMvcStarter>()){
                starter.Init();
            }
            
        }

        private void applyIoc(){
            //  ioc.install();
            foreach (var initializer in _Container.all<IContainerInitializer>()){
                initializer.Apply(_Container);
            }
        }

        protected void AddViews(string aname, string nsname){
            var config = MonoRailConfiguration.GetConfig();
            //bypass for testing environment
            if (null == config){
                return;
            }
            config.ViewEngineConfig.AssemblySources.Add(
                new AssemblySourceInfo(aname, nsname));
        }

        protected virtual void startIoc(){
            container = ((WindsoreInversionContainer) myapp.ioc).Container;

            container.AddFacility("monorail", new MonoRailFacility());

            container.LoadDefaultXml();
        }

        protected virtual void setupServices(){
            _Container
                .setupSecurity()
                .setupFilesystem()
                ;
            if (connection.hasContent()){
                _Container.setupHibernate(new NamedConnection(connection, connection));
            }
            else{
                _Container.setupHibernate();
            }
            _Container
                .set<IWebEnvironment, BaseWebEnvironment>(Commons.BaseWebEnvironmentName)
                .set<IDefaultScriptMachine, ClassicDefaultApplicationScriptMachine>()
                .set<IResqueProvider, DefaultRescueProvider>()
                .set<IApplicationCache, DefaultMvcCache>()
                .set<IUniqueStringProvider, RandomStringUniqueStringProvider>()
                .set<DefaultConverstionStatisticsInterceptor>()
                .set<AclModel>()
                ;
        }

        protected virtual void setupControllers() {}

        public BaseApplication EnsureComponent<T>(){
            var name = typeof (T).FullName + ".component";
            if (typeof (IController).IsAssignableFrom(typeof (T))){
                name = typeof (T).FullName + ".controller";
				_Container.set(name, typeof(T));
            }
            if (typeof (ViewComponent).IsAssignableFrom(typeof (T))) {
				name = typeof(T).Name;
            	this.Container.Register(Component.For<T>().LifeStyle.Transient.Named(name));
            }
            
            return this;
        }

        public void CallEndRequest(){
            Application_EndRequest();
        }
    }
}