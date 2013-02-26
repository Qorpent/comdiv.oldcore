//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Comdiv.Design;
using Comdiv.Extensibility.ExtensionsCompiler;
using Comdiv.Extensions;
using Comdiv.Inversion;

namespace Comdiv.Application {
    /// <summary>
    /// Базовый шаблон приложения, предполагается что практически вся настройка будет производиться
    /// через расширения (перехватчики) приложения. По сути это аналоги HttpModule по своей природе.
    /// Из собственного функционала базовое приложение может:
    /// а) догружать библиотеки из MOD
    /// б) выдавать сообщение об ошибке загрузки
    /// в) способность загружать и перезагружать расширения при наличии в доступе extensionscompiler.exe
    /// рассматриваются как способность ядра и при этом внешняя утилитная функция
    /// </summary>
    public class WebApplicationBase : HttpApplication, IWithContainer {
        private static Exception initializationException;
        private static bool initialized;

        private static readonly object initlock = new object();
        protected static object sync = new object();
        private IInversionContainer __container;
        [TestPropose] protected string connection;
        private IApplicationLifecycleManager _lifecycleManager;
        [TestPropose] protected Type[] overrides;

        public WebApplicationBase() {
            
            BeginRequest += WebApplicationBase_BeginRequest;
            EndRequest += WebApplicationBase_EndRequest;
            Error += WebApplicationBase_Error;
            PostAuthorizeRequest += WebApplicationBase_PostAuthorizeRequest;

          
        }

        private DateTime last = new DateTime(1900,1,1);
        protected void appendstartlog(string message) {
            DateTime time = DateTime.Now;
            var span = time - last;
            string span_ = span.ToString();
            if(last.Year==1900) {
                span_ = "-";
            }
            File.AppendAllText(startuplogfile, string.Format("{0} {1} {2}\r\n", time.ToString("mm:ss"), span_, message));
            last = time;
        }

        #region IWithContainer Members

        public virtual IInversionContainer Container {
            get {
                lock (this) {
                    if (__container == null) {
                        __container = myapp.ioc;
                    }
                    return __container;
                }
            }
            set {
                lock (this) {
                    __container = value;
                }
            }
        }

        #endregion
        bool dothrow = false;
        protected void Application_Start(object sender, EventArgs args) {
            try {
                lock (initlock) {
                    if (!initialized) {
                        this.startuplogfile = HttpContext.Current.Server.MapPath("~/tmp/startlog.log");
                        File.WriteAllText(startuplogfile, "");
                        appendstartlog("start");
                        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                        checkExtensibility();
                        appendstartlog("extesibility checked");
                        loadExtendedLibraries();
                        appendstartlog("extended libraries loaded");
                        startLongLifeInitializationTasks();
                        appendstartlog("long life started");
                        initializeApplication();
                        appendstartlog("application initialized");
                        finishLongLifeInitializationTasks();
                        appendstartlog("long life finished");
                        prepareApplication();
                        appendstartlog("application prepared");
                        lifecycleManager.OnStartApplication();
                        appendstartlog("lifecycle started");
                        myapp.OnReload += (s, e) => {
                                              startLoadExtensions();
                                              endLoadExtensions();
                                          };
                        initialized = true;
                        appendstartlog("initialization ended");
                    }
                }
            }
            catch (Exception ex) {
                initialized = false;
                initializationException = ex;
                if(dothrow) {
                    throw;
                }
            }
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var dll =
                Directory.EnumerateFiles(HttpContext.Current.Server.MapPath("~/"), args.Name.Split(',')[0].Trim() + ".dll",
                                         SearchOption.AllDirectories)
                    .Where(x => !x.ToLower().Contains("\\extensionslib\\")
                        && !x.ToLower().Contains("\\tmp\\")
                    ).FirstOrDefault();
            if(dll.hasContent()) {
                return Assembly.LoadFrom(dll);
            }
            File.AppendAllText(HttpContext.Current.Server.MapPath("~/tmp/notloaded.txt"), args.Name+"\r\n");
            return null;
        }

        private static string compilerDirectory;
        private void checkExtensibility() {
            UseExtensions = true;
            IsWebContext = HttpContext.Current != null;
            var exefile = "";
            if(IsWebContext) {
                exefile = HttpContext.Current.Server.MapPath("~/bin/extensionscompiler.exe");

            }else {
                exefile = Path.GetFullPath("extensionscompiler.exe");
            }
            if(!File.Exists(exefile)) {
                exefile = Environment.ExpandEnvironmentVariables("%comdivextensionscompiler%");
                if(string.IsNullOrWhiteSpace(exefile)||!File.Exists(exefile)) {
                    UseExtensions = false;
                }
            }
            compilerDirectory = Path.GetDirectoryName(exefile);
        }

        protected static bool UseExtensions { get; set; }
        protected static bool IsWebContext { get; set; }
        private static ApplicationExtensionsLoadTask extensionsloader;
        private string startuplogfile;

        protected virtual void finishLongLifeInitializationTasks() {
            
            endLoadExtensions();
        }

        private static void startLoadExtensions() {
            if(UseExtensions) {
                extensionsloader = extensionsloader ??( new ApplicationExtensionsLoadTask(compilerDirectory,IsWebContext));
                extensionsloader.Start();
            }
        }

        protected void startLongLifeInitializationTasks() {
            startLoadExtensions();
        }

        private static void endLoadExtensions() {
            if(UseExtensions) {
                var result = extensionsloader.Finish();
                if(null==result) {
                    throw new Exception("extensions loader not return assembly");
                }
                var dict = new ExtensionsLoader().GetRegistry(result);
                foreach (var e in dict) {
                    myapp.ioc.set(e.Key, e.Value);
                }
                extensionsloader = null;
            }
        }

        private void loadExtendedLibraries() {
            var bins = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/"), "*.dll",SearchOption.AllDirectories);
            var tempdir = HttpContext.Current.Server.MapPath("~/tmp/dlls/");
            Directory.CreateDirectory(tempdir);
          //  Directory.Delete(tempdir, true);
          //  Directory.CreateDirectory(tempdir);
            
            foreach (var bin in bins) {
                var file = bin.ToLower().Replace("\\","/");
                if (file.Contains("/bin/")) continue;
                if (file.Contains("/tmp/")) continue;
                if (file.Contains("/extensionslib/")) continue;
                if (null!=myapp.files.Resolve("~/bin/"+Path.GetFileName(file),true)) {
                    continue;
                    // prevent to recache libraries that are already in bin directory
                }
                var newfile = Path.Combine(tempdir, Path.GetFileName(file));
				try {
					File.Copy(file, newfile, true);
				}catch(Exception) {
					
				}

            }
            foreach (var file in Directory.GetFiles(tempdir,"*.dll"))
            {
                ReflectionExtensions.LoadAssemblyFromFile(file, tempdir);
            }
        }

        protected void Application_End(object sender, EventArgs args) {
            lock (sync) {
                selfOnEndApplicationBeforeCustom();
                lifecycleManager.OnFinishApplication();
                selfOnEndApplicationAfterCustom();
            }
        }

        protected virtual void selfOnEndApplicationAfterCustom() {
        }


        protected virtual void selfOnEndApplicationBeforeCustom() {
        }

        protected virtual void prepareApplication() {
            foreach (var initializer in Container.all<IContainerInitializer>()) {
                initializer.Apply(Container);
            }
           
        }

        protected IApplicationLifecycleManager lifecycleManager {
            get {
                return _lifecycleManager ??
                       (_lifecycleManager =
                        Container.get<IApplicationLifecycleManager>() ?? new ApplicationLifecycleManager());
            }
        }
        

        protected virtual void initializeApplication() {
        }

        private void WebApplicationBase_PostAuthorizeRequest(object sender, EventArgs e) {
            lock (sync) {
                selfPostAuthorizeRequestBeforeCustom();
                lifecycleManager.OnPostAuthorizeRequest();
                selfPostAuthorizeRequestAfterCustom();
            }
        }

        protected virtual void selfPostAuthorizeRequestBeforeCustom() {         
            myapp.principals.BasePrincipal = null;
            try {
                myapp.storage.GetDefault().SetCacheMode(false);
            }catch(NotSupportedException) {
                
            }


        }

        protected virtual void selfPostAuthorizeRequestAfterCustom() {
        }

        private void WebApplicationBase_Error(object sender, EventArgs e) {
            selfOnErrorRequestBeforeCustom();
            lifecycleManager.OnErrorRequest();
            selfOnErrorRequestAfterCustom();
        }

        protected virtual void selfOnErrorRequestBeforeCustom() {
        }

        protected virtual void selfOnErrorRequestAfterCustom() {
        }

        private void WebApplicationBase_EndRequest(object sender, EventArgs e) {
            selfOnEndRequestBeforeCustom();
            lifecycleManager.OnFinishRequest();
            selfOnEndRequestAfterCustom();
        }

        protected virtual void selfOnEndRequestBeforeCustom() {
        }

        protected virtual void selfOnEndRequestAfterCustom() {
        }

        private void WebApplicationBase_BeginRequest(object sender, EventArgs e) {
            selfOnBeginRequestBeforeCustom();
            lifecycleManager.OnStartRequest();
            selfOnBeginRequestAfterCustom();
        }

        protected virtual void selfOnBeginRequestBeforeCustom() {
            checkWellStart();
        }

        protected virtual void selfOnBeginRequestAfterCustom() {
        }

        private void checkWellStart() {
            if (!initialized) {
                if (initializationException != null) {
                    var message =
                        @"<h1>При старте приложения возникла ошибка</h1>
<p>Данное соощение на экране свидетельствует о наличии серьезных проблем сервера, обновите страницу, обязательно сообщите администратору если 
данное сообщение возникнет снова после обновления страницы
</p>
" +
                        initializationException.ToString().Replace("\r\n", "<br/>");
                    HttpContext.Current.Response.Write(message);
                    HttpContext.Current.Response.Flush();
                    HttpContext.Current.Response.Close();
                    HttpRuntime.UnloadAppDomain();
                    return;
                }
            }
        }

        [TestPropose]
        public WebApplicationBase Start(params Type[] overrides) {
            return Start("test_env", overrides);
        }

        [TestPropose]
        public WebApplicationBase Start(string connection, params Type[] overrides) {
            dothrow = true;
            lock (sync) {
                preparestart(connection, overrides);
                Application_Start(null, EventArgs.Empty);
                if (initializationException != null) {
                    throw initializationException;
                }
                return this;
            }
        }

        [TestPropose]
        protected virtual void preparestart(string connection, params Type[] overrides) {
            this.connection = connection;
            initializationException = null;
            initialized = false;
            this.overrides = overrides ?? Type.EmptyTypes;
        }
    }
}