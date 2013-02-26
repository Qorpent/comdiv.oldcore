using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Controllers;
using Comdiv.MVC.Extensions;
using Comdiv.MVC.Resources;
using Comdiv.Persistence;
using Comdiv.Reporting;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Xml;
using Qorpent.Security;

namespace Comdiv.MVC.Report{
    using RetLoadFunc = Func<IMvcContext, string, IDictionary<string, object>, IReportRequestLoader>;
    using RetInitFunc = Func<IMvcContext, IDictionary<string, object>, IReportRequestInitiator>;

    public class ReportAclListSource : IAclSource{
        #region IAclSource Members

        public AclList Get(IPrincipal principal){
            lock (this){


                var result = new AclList();
                result.Name = "Отчеты";
                foreach (var item in new ReportFactory().GetAvailableReportTemplates()){
                    var def = item.Tag as IReportDefinition;
                    var e = new Entity();
                    e.Code = def.Code;
                    e.Name = def.Name;
                    e.Comment = acl.token(def);
                    e.Action = acl.get(def, "access", null, principal).ToString();
                    result.Items.Add(e);
                }
                return result;
            }
        }

        #endregion
    }

    public class ReportFactory : IReportFactory{
        protected static readonly object sync = new object();
        private readonly IList<RetInitFunc> initFunctions = new List<RetInitFunc>();
        private readonly IList<RetLoadFunc> loaderFunctions = new List<RetLoadFunc>();
        private readonly ILog log = logger.get("zreport");

        private readonly IDictionary<string, IList<Entity>> ReportListCache =
            new Dictionary<string, IList<Entity>>();

        private Type defaultDefinitionType;
        private Type defaultRequestType;

        public ReportFactory(){
            PrincipalSource = myapp.principals;
            PathResolver = myapp.files;
            myapp.OnReload += (s, a) => ReportListCache.Clear();
        }

        public IFilePathResolver PathResolver { get; set; }

        public IPrincipalSource PrincipalSource { get; set; }


        

        #region IReportFactory Members

        public Type DefaultRequestType{
            get{
                lock (this){
                    return defaultRequestType ?? typeof(ReportRequest);    
                }
                
            }
            set { defaultRequestType = value; }
        }

        public Type DefaultDefinitionType{
            get{
                lock (this){
                    return defaultDefinitionType ?? typeof (ReportDefinitionBase);
                }
            }
            set { defaultDefinitionType = value; }
        }

        public IList<RetLoadFunc> LoaderFunctions{
            get { return loaderFunctions; }
        }

        public IList<RetInitFunc> InitFunctions{
            get { return initFunctions; }
        }

        public IReportRequestLoader GetDefaultRequestLoader(){
            return new CacheRequestStorage();
        }

        public IReportRequestInitiator GetDefaultRequestInitiator(){
            return new CacheRequestStorage();
        }

        public IReportRequest CreateEmptyRequest(){
            return DefaultRequestType.create<IReportRequest>();
        }

        public IReportRequestLoader GetLoader(IMvcContext context, string uid,
                                              IDictionary<string, object> parameters){
            //TODO: переписать на поиск лоадера в IOC
            return GetDefaultRequestLoader();
        }

        public IReportDefinition CreateEmptyDefiniton(){
            return DefaultDefinitionType.create<IReportDefinition>();
        }


        public IReportRequest LoadRequest(Controller controller){
            lock (sync){
                return LoadRequest(controller, null);
            }
        }

        public IReportRequest LoadRequest(Controller controller, Type definitionType){
            lock (sync){
                var safedRequest = controller.Params[RequestParameterNames.Default.Report_RequestKey];
                var templateRequest = controller.Params["tcode"];
                var context = MvcContext.Create(controller);
                if (safedRequest.hasContent()){
                    return LoadRequest(context, safedRequest, null);
                }
                if (templateRequest.hasContent()){
                    return LoadTemplate(context, templateRequest, definitionType, controller);
                }
                return null;
            }
        }


        public IList<Entity> GetAvailableReportTemplates(){
            lock (sync){
                var key = PrincipalSource.CurrentUser.Identity.Name;

                ReportListCache.Clear();
                return ReportListCache.get(key,
                                           () => (from f in PathResolver.ResolveAll("reports/templates", "*.xml")
                                                  where !f.EndsWith(".cached") && !f.Contains("svn")
                                                  let def =
                                                      LoadDefinition(Path.GetFileNameWithoutExtension(f),
                                                                     typeof (ReportDefinitionBase))
                                                  where acl.get<IReportDefinition>(def.Code)
                                                  select new Entity{
                                                                       Code = def.Code,
                                                                       Name = def.Name,
                                                                       Comment = def.Comment,
                                                                       Area = def.Area,
                                                                       Controller = def.Controller,
                                                                       Action = def.Code,
                                                                       Tag = def
                                                                   }).ToList(), true);
            }
        }

        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = ioc.Container;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public virtual IReportDefinition LoadDefinition(string code, Type definitionType){
            lock (sync){
                
                var xmlPath = allReportTemplates().FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == code);
                if(xmlPath.noContent()) {
                    xmlPath = allReportTemplates().FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == code+"Aa");
                }
                if (xmlPath.noContent()){
                    throw new Exception("Не могу найти шаблона отчета с кодом " + code);
                }
                var definition = definitionType.create<IReportDefinition>();
                using (var r = IncludeAwareXmlReader.Create(xmlPath)){
                    var doc = XElement.Load(r);
                    definition.ReadFromXml(doc);
                }
                if (!definition.Code.hasContent()){
                    definition.Code = code;
                }
                return definition;
            }
        }

        public IReportRequest InitRequest(Controller controller, Dictionary<string, object> parameters){
            lock (sync){
                var d = MvcContext.Create(controller);
                return GetInitiator(d, parameters).InitRequest(d, parameters);
            }
        }

        #endregion

        private IReportRequest LoadTemplate(MvcContext context, string templateCode, Type definitionType,
                                            Controller controller){
            log.debug(() => "start load template");
            var definition = LoadDefinition(templateCode, definitionType).Clone();
            definition.CleanupParameters(myapp.usr);

            if (acl.get(definition)){
                var result = new ReportRequest();
                var savedcode = controller.Request.Params["srcode"];
                if(savedcode.hasContent()){
                    definition.TemplateParameters.SavedReport = myapp.storage.Get<ISavedReport>().Load(savedcode);
                    if(!definition.TemplateParameters.SavedReport.Authorize(myapp.usr)) {
                        throw new SecurityException("попытка вывода недоступного хранимого отчета с кодом "+savedcode);
                    }
                }else {
					definition.TemplateParameters.SavedReport = myapp.storage.Get<ISavedReport>().Load(templateCode+"_default");
                }
                if (!controller.existed("notempalate")){
                    
                    definition.LoadParameters(controller.Request.Params);
                }

                result.ReportDefinition = definition;
                result.RequestId = new ReportRequestIdentity{Uid = Guid.NewGuid().ToString()};
                log.debug(() => "end load template");
                return result;
            }
            else{
                throw new SecurityException("Вы не имеете права использовать шаблон отчета с кодом " + templateCode);
            }
        }


        private IEnumerable<string> allReportTemplates(){
            foreach (var root in new[]{"usr/", "mod/", "sys/"}){
                string dir = myapp.files.Resolve(root + "reports/templates",false);
                if (Directory.Exists(dir)){
                    foreach (var file in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)){
                        yield return file;
                    }
                }
            }
        }

        private IReportRequest LoadRequest(IMvcContext mvcContext, string key,
                                           IDictionary<string, object> parameters){
            return GetLoader(mvcContext, key, parameters).Load(key, parameters, mvcContext);
        }

        private IReportRequestInitiator GetInitiator(MvcContext context, Dictionary<string, object> parameters){
            //TODO: переписать на поиск инициатора в IoC
            return GetDefaultRequestInitiator();
        }
    }
}