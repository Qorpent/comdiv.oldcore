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
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.WindsorExtension;
using Castle.Windsor;
using Comdiv.Authorization;
using Comdiv.Controllers;
using Comdiv.Framework.Quick;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Useful;
using Comdiv.Wiki;
using Qorpent.Serialization;
using WikiRender = Comdiv.Wiki.WikiRender;

namespace Comdiv.Application {

	public class JsonTransformer : IJsonTransformer {
		public string ToJson(object val) {
			
			return new JsSerializer().Serialize("result",val);
		}
	}
    public class MonoRailApplication : WebApplicationBase, IContainerAccessor {
        protected static IWindsorContainer container;
        private string connection;

        public MonoRailApplication():base() {
            UseExtensions = true;
        }

        #region IContainerAccessor Members

        IWindsorContainer IContainerAccessor.Container {
            get { return container; }
        }

        #endregion

        protected override void preparestart(string connection, params Type[] overrides) {
            base.preparestart(connection, overrides);
            container = null;
        }


        protected override void initializeApplication() {
            base.initializeApplication();
			QuickInstaller.PrepareQWeb(ioc.Container);
            appendstartlog("qweb prepared");
			setupServices();
            appendstartlog("services setuped");
            if (ioc.Container is WindsoreInversionContainer) {
                container = ((WindsoreInversionContainer) myapp.ioc).Container;
                container.AddFacility("monorail", new MonoRailFacility());
                container.LoadDefaultXml();
                appendstartlog("ioc started");
            }
            Container
                .setupSecurity(typeof (XmlRoleProvider<BxlApplicationXmlReader>))
                .setupFilesystem()
                .set<IWidgetRepository, BxlWidgetRepository>()
                .set<IUniqueStringProvider, RandomStringUniqueStringProvider>()
                ;
            appendstartlog("default services seted to ioc");

            var dir = AppDomain.CurrentDomain.BaseDirectory;
            if( File.Exists(dir + "\\NHibernate.dll")) {
                Container.setupHibernate(string.IsNullOrWhiteSpace(connection)
                                             ? (IConnectionsSource)null
                                             : new NamedConnection(connection, connection));
            }
            appendstartlog("hibernate setted up");

            Container
                .set<FirstRunController>()
                .set<RestartController>()
                .set<EchoController>()
                .set<SysInfoController>()
                .set<RescueController>()
                .set<ProfileController>()
                .set<ExtInstallerController>()
                .set<RoleController>()
                .set<ImpersonateController>()
                .set<NotAuthorizedController>()
                .set<FileManagerController>()

                //wiki support
                .set<IWikiRepository, WikiRepository>()
                .set<IWikiRenderService, WikiRender>()
                .set<IWikiPersistenceProvider, WikiPersistenceProvider>()
                .AddTransient("wiki.usuallink", typeof(WikiPageLinkRender))
                .set<WikiController>()
                ;

            appendstartlog("container setted up");
            configureViews();
            appendstartlog("views configured");
        }

        private void configureViews() {
            try {
                var cfg = MonoRailConfiguration.GetConfig().ViewEngineConfig;
                
                var roots = new[]{
                                     "~/usr/extensions/", "~/usr/views/",
                                     "~/mod/extensions/", "~/mod/views/",
                                     "~/sys/extensions/", "~/sys/views/",
                                     "~/extensions/", "~/views/",
                                 };
                bool first = true;
                foreach (var root in roots) {
                    var r = myapp.files.Resolve(root, false);
                    if (Directory.Exists(r)) {
                        if (first) {
                            first = false;
                            cfg.ViewPathRoot = r;
                            
                        }
                        else {
                            cfg.PathSources.Add(r);
                        }
                    }
                }

                cfg.ConfigureDefaultViewEngine();
            }catch(Exception e) {
                Logging.logger.get("http").Warn("cannot configure views",e);
            }
        }

        protected override void prepareApplication() {
            base.prepareApplication();
            doStartUp();
        }


        protected override void selfOnEndApplicationAfterCustom() {
            base.selfOnEndApplicationAfterCustom();
            container.Dispose();
        }


        protected virtual void doStartUp() {
             var dir = AppDomain.CurrentDomain.BaseDirectory;
             if (File.Exists(dir + "\\NHibernate.dll")) {
                 prepareSession();
             }
        }

        private void prepareSession() {
            var sfp = Container.get<ISessionFactoryProvider>();
            if (sfp != null) sfp.Get(null);
        }

        protected virtual void startIoc() {
        }

        protected virtual void setupServices() {
        	Container.set<IDefaultProfileRepository, ProfileRepository>("profilerepository.transient");
			Container.set<IJsonTransformer, JsonTransformer>("jsontransformer.transient");
        }

        protected virtual void setupControllers() {
        }
    }
}