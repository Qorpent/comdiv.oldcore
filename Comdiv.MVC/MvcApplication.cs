using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Facilities.Logging;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;
using Comdiv.Controllers;
using Comdiv.Conversations;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Messaging;
using Comdiv.Model;
using Comdiv.Model.Dictionaries;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Lookup;
using Comdiv.Model.Scaffolding.Model;
using Comdiv.MVC.Controllers;
using Comdiv.MVC.Report;
using Comdiv.MVC.Reporting.Security;
using Comdiv.MVC.Rules;
using Comdiv.MVC.Scaffolding;
using Comdiv.MVC.Security;
using Comdiv.MVC.Wiki;
using Comdiv.Persistence;
using Comdiv.Reporting;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Web;
using Comdiv.Wiki;

namespace Comdiv.MVC{
    public class
        MvcApplication : BaseApplication{
        protected override void setupControllers(){
            base.setupControllers();
            
                _Container
                .set<PublicController>()
                .set<SysJsController>()
                .set<SysController>()
                .set<SessionController>()
                .set<MetalinkController>()
                .set<ConfigurationController>()
                .set<ResourceResolverController>()
                .set<ImpersonationController>()
                .set<HealthController>()
                .set<ErrorsController>()
                .set<WorkspaceController>()
                .set<WikiController>()
                .set<DatabaseController>()
                .set<ProfileController>()
                .set<AclController>()
                .set<UniversalObjectManagerController>()
                .set<ReportController>()
                .set<FileController>()
                .set<MessageController>()
                .set<IReportViewFactory,DefaultReportViewFactory>()
                .set<ReportAclListSource>()
                    //preprocessor
                .setAll<MvcApplication, IBrailPreprocessor>()
                .set<MvcTokenProvider>()
                .set<ReportTokenProvider>()
                .set<IReportDefinitionCacheStringProvider, ReportDefinitionCacheStringProvider>()
                .set<IReportCache, ReportCache>()

                ;
            var t = Type.GetType("Comdiv.Messaging.Model.MessageModel, Comdiv.Messaging.Model");
            if (null != t){
                _Container
                    .set("Comdiv.Messaging.Model.MessageModel, Comdiv.Messaging.Model".toType());
                ;
            }
            _Container.AddTransient(null, typeof(DataBasedReportCacheLeaseProvider));
                _Container.set("ioc.getrewriter", new MvcTokenByParameterRewriter("database/object/.*", "type"));
                EnsureComponent<Scaffolder>()
                    .EnsureComponent<LookupListComponent>()
                    .EnsureComponent<DBLookupModel>();   
            
            
            
        }

        protected override void doStartUp(){
            base.doStartUp();
            //   acl.repository.deny("/app/database/object/****/ioc.getcls", "");
        }

        protected override void setupServices(){
            base.setupServices();
           
                _Container
                .set<IAuthorizeService, DefaultAuthorizeService>()
                .set<IPartitionsSource, DefaultPartitionsSource>()
                .set<ControllerRuleStorage>()
                .set<IParametersBinder, DefaultParametersBinder>()
                .set<IMetadataHelper, MetadataHelper>()
                .set<ClsModel>()
                .set<DictionaryModel>()
                .set<LogModel>()
                .set<IWikiRepository,WikiRepository>()
                .set<IWikiRenderService,WikiRender>()
                .set<IWikiPersistenceProvider,WikiPersistenceProvider>()
                .AddTransient("wiki.usuallink",typeof(WikiPageLinkRender))
                .AddTransient("wiki.paramlink", typeof(WikiReportParameterLinkRender))
                .set<IMessageRepository, MessageRepository>("transient.message.repository")
                .set<IMessageQueryExecutor, DefaultMessageQueryExecutor>("transient.message.repository.executor")
                ;
#if !LIB2
                //AddViews("Comdiv.Web", "Comdiv.Web.ScaffolderDefaultViews");
                AddViews("Comdiv.MVC", "Comdiv.MVC.repos.Comdiv.MVC.sys.views");
                var loggerFile = enumerateLogConfigs().FirstOrDefault();

                if (null != loggerFile)
                {
                    container.AddFacility("logging",
                                          new LoggingFacility(LoggerImplementation.Log4net,
                                                              loggerFile.mapPath()));
                }
#endif

        }

        private IEnumerable<string> enumerateLogConfigs(){
            foreach (var s in new[]{"~/usr", "~/mod", "~/sys"}){
                var path = s.mapPath();
                if (!Directory.Exists(path)){
                    continue;
                }

                foreach (var file in Directory.GetFiles(path, "log4net.config", SearchOption.AllDirectories)){
                    yield return file;
                }
            }
        }
        }
}