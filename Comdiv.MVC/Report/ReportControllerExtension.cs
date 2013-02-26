#region

using System;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC;
using Comdiv.MVC.Report;
using Comdiv.Persistence;
using Comdiv.Reporting;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Xslt;

//using Comdiv.Xslt;

#endregion

namespace System.IO.Packaging{
}

namespace System.IO.Compression{
}

namespace Comdiv.Web{

    #region

    #endregion

    #region

    #endregion

    //TODO: Общая архитектура:  Делегировать отсюда контроллеру обработку исходящего потока включая параметры формата файла, по идее создаение ZIP и контроль заголовков - дело контроллера, он расширению должен передавать только TextWriter и формат 

    
    public class ReportControllerExtension :  IReportControllerExtension, IControllerAware{
        private const string namespaceBase = "urn://extension/";
        private static readonly object sync = new object();

        private readonly IReportCache cache =null;

        private readonly Type[] parameterTypes = new[]{
                                                          typeof (string), typeof (XPathNodeIterator),
                                                          typeof (XPathNavigator)
                                                      };

        public bool PreviewMode { get; set; }

        public ReportControllerExtension(){
            cache = Container.get<IReportCache>()  ?? new ReportCache();
        }

        private string cachedContent;

        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        private IReportContentPreparator contentPreparator = new BrailBasedReportContentPrepararor();
        protected IReportRequest currentRequest;
        private Type defaultReportDefinitionClass = typeof (ReportDefinitionBase);

        public string FileName;
        private string type;

        public bool UseZipPackage{
            get{
                if (null == MyController.Params["zip"]){
                    return false;
                }
                return MyController.Params["zip"].toBool();
            }
        }


        public string Mode { get; set; }

        public IReportContentPreparator ContentPreparator{
            get{
                contentPreparator.Extension = this;
                return contentPreparator;
            }
            set { contentPreparator = value; }
        }

        public string PreparatorType{
            set{
                if (!value.yes()){
                    return;
                }
                if ("boo" == value){
                    ContentPreparator = new BooBasedReportContentPreparator();
                }
                else if ("brail" == value){
                    ContentPreparator = new BrailBasedReportContentPrepararor();
                }
                else{
                    ContentPreparator = value.toType().create<IReportContentPreparator>();
                }
            }
        }


        public static BooViewEngine Engine { get; set; }

        public Type DefaultReportDefinitionClass{
            get { return defaultReportDefinitionClass; }
            set { defaultReportDefinitionClass = value; }
        }

        #region IControllerAware Members

        public void SetController(IController controller, IControllerContext controllerContext){
            MyController = controller as Controller;
        }

        #endregion

        #region IReportControllerExtension Members

        public Controller MyController { get; set; }

        public string ViewName { get; set; }

        public IReportDefinition Definition { get; set; }


        public string Type{
            get{
                if (type.hasContent()){
                    return type;
                }
                string _param;
                return null != (_param = MyController.Params["type"]) ? _param : string.Empty;
            }
            set { type = value; }
        }

        public string ReportName { get; set; }


        public virtual void ControlPreparation(Controller controller){
            SetController(controller, null);
            if (null == currentRequest){
                currentRequest = Container.get<IReportFactory>().LoadRequest(MyController, DefaultReportDefinitionClass);
            }
            controller.PropertyBag["_form"] = ViewName + "_prepare";
        }

        public virtual void ControlGeneration(Controller controller){
            ControlGeneration(controller, true);
        }
        public void CheckReportLive()
        {
            if (this.Task.HaveToTerminate || !this.Task.Context.Response.IsClientConnected)
            {
                this.Task.Terminate();
                throw new Exception("Выполнение отчета было отменено");
            }
        }
        public virtual void ControlGeneration(Controller controller, bool render){
            SetController(controller, null);
            prepareData(controller);
            if (render){
                RenderReport(controller);
            }
        }

        public virtual void CustomContentPreparatorPrepare(IReportContentPreparator preparator){
            if (null == currentRequest){
                currentRequest = Container.get<IReportFactory>().LoadRequest(MyController, DefaultReportDefinitionClass);
            }

            foreach (var ext in this.Definition.Extensions){
                var e = Container.get(ext.Value);
                if(e is IReportDefinitionExtension){
                    ((IReportDefinitionExtension)e).Contextualize(this.Definition);
                }
                Definition.Parameters[ext.Key] = e;
                preparator.SetValue(ext.Key,e);
            }
            preparator.SetValue("this", this);
            preparator.SetValue("definition",this.Definition);
            preparator.SetValue("controller", MyController);
            preparator.SetValue("type", Type);
			if(MyController.Context.UnderlyingContext!=null) {
				preparator.SetValue("siteroot", MyController.Context.UnderlyingContext.Request.ApplicationPath);	
			}else {
				preparator.SetValue("siteroot", "/zeta");	
			}
            
            Func<string, bool> isset = s => null != MyController.Params[s];
            preparator.SetValue("isset", isset);
            Func<string, string> val = s => MyController.Params[s];
            preparator.SetValue("val", val);
            Func<string, bool, bool> setdef = (s, d) => isset(s) ? true : d;
            Action<string, object> setval = (n, v) => MyController.PropertyBag[n] = v;
            Action bindAllToView = () =>{
                                       foreach (var valuePair in preparator.Parameters){
                                           MyController.PropertyBag[valuePair.Key] = valuePair.Value;
                                       }
                                   };
            preparator.SetValue("setval", setval);
            preparator.SetValue("setdef", setdef);
            preparator.SetValue("bindall", bindAllToView);

            Func<object, int> toint = x => x.toInt();
            Func<object, string, string> decstr2 = (x, f) => x.toDecimal().ToString(f, CultureInfo.InvariantCulture);
            Func<object, string> decstr = x => x.toDecimal().ToString("#,0.##", CultureInfo.InvariantCulture);
            Func<object, bool> tobool = x => x.toBool();
            Func<object, decimal> todec = x => x.toDecimal();
            Func<object, string> tostring = x => x.toStr();
            preparator.SetValue("toint", toint);
            preparator.SetValue("todec", todec);
            preparator.SetValue("tobool", tobool);
            preparator.SetValue("tostring", tostring);
            preparator.SetValue("decstr", decstr);
            preparator.SetValue("numformat", decstr2);
            preparator.SetValue("usr", myapp.usr);


            //TODO : Refactor for new Report system
            bindCurrentRequest(preparator);
            if (null != Definition && Definition.Parameters.ContainsKey("generatorname")){
                ViewName = (string) Definition.Parameters["generatorname"];
            }
            if (ViewName.noContent()){
                ViewName = DefaultViewName;
            }
            if (Definition.yes()){
                foreach (var c in Definition.Parameters){
                    preparator.SetValue(c.Key, c.Value);
                }
            }
            preparator.SetValue("report", Definition);
        }

        public string DefaultViewName { get; set; }
        public LongTask Task { get; set; }

        public virtual void RenderReport(Controller controller, TextWriter writer = null){
			if (null != controller) {
				prepareContentType(controller);
			}
        	var report = "";
            if (cachedContent.hasContent()){
                report = File.ReadAllText(cachedContent);
            }
            else{
                report = PrepareContentToOut(controller);
                if (Definition.Parameters.ContainsKey("postprocess")){
                    report = postprocess(report);
                }
                cache.Set(Definition, report);
            }
			if (controller != null) {
				WriteOutContent(controller, report);
			}
			if(writer!=null) {
				writer.Write(report);
			}
        }

        #endregion

        private void bindCurrentRequest(IReportContentPreparator preparator){
            if (null != currentRequest){
                foreach (
                    var pair in currentRequest.Parameters
                    ){
                    if (pair.Key == "definition"){
                        continue;
                    }
                    preparator.SetValue(pair.Key, pair.Value);
                }
            }
        }


        private void extractDefinition(){
            if (null != currentRequest){
                foreach (
                    var pair in currentRequest.Parameters
                    ){
                    if (pair.Key == "definition"){
                        Definition = (IReportDefinition) pair.Value;
                        Definition.Task = this.Task;
                        Definition.PreviewMode = this.PreviewMode;
                        Definition.ControllerInstance = this.MyController;
                    }
                    break;
                }
            }
            if (null == Definition && null != currentRequest.ReportDefinition){
                Definition = currentRequest.ReportDefinition;
                Definition.Task = this.Task;
                Definition.PreviewMode = this.PreviewMode;
                Definition.ControllerInstance = this.MyController;
            }
        }


        protected virtual string postprocess(string report){
            var replaces = Definition.Parameters["postprocess"].ToString().split(false, false, '#');
            foreach (var replace in replaces){
                var r = replace.Split(new[]{"=="}, StringSplitOptions.None);
                var regex = r[0];
                var repl = r[1];
                report = report.replace(regex, repl);
            }
            return report;
        }

        protected virtual void prepareData(Controller controller){
            if (null == currentRequest) {
            	var f = Container.get<IReportFactory>();
				if(null!=f) {
					currentRequest =f.LoadRequest(MyController, DefaultReportDefinitionClass);	
				}else {
					currentRequest = new ReportRequest();
					currentRequest.ReportDefinition = this.Definition;
					currentRequest.RequestId = new ReportRequestIdentity { Uid = Guid.NewGuid().ToString() };
				}

				 
                
            }
            extractDefinition();
            if (Definition != null ){
                cachedContent = cache.Get(Definition);
            }
            if (cachedContent.noContent()){
                controller.PropertyBag["content"] = ContentPreparator.BuildContent(this, controller).CreateNavigator();
            }
        }

        public string PerformXsltTransformation(){
            return PerformXsltTransformation(GetXsltPath());
        }

        public string PerformXsltTransformation(string path){
            var sw = new StringWriter();
            PerformXsltTransformation(path, sw);
            return sw.ToString();
        }

        private void PerformXsltTransformation(string path, TextWriter sw){
            var xslt = LoadXslt(path);
            var args = GetArgs();
            var doc = getInputDocument();

            xslt.Transform(doc, args, sw);
        }

        protected virtual XsltArgumentList GetArgs(){
            var args = new XsltArgumentList();
            XsltStandardExtension.PrepareArgs(args);

            MoveControllerDataToXslt(args);
            if (Definition != null){
                foreach (var pair in Definition.Parameters){
                    try{
                        if (null == args.GetParam(pair.Key, "")){
                            args.AddParam(pair.Key, "", pair.Value);
                        }
                    }
                    catch(Exception){
                        throw new Exception("Неверное имя для параметра: "+pair.Key);
                    }
                }
                args.AddParam("pageTitle", "", Definition.PageTitle);
            }
            args.AddExtensionObject("http://comdiv/xslt/brail",
                                    new BrailXsltExtension{Controller = MyController, ReportControllerExtension = this});
            return args;
        }

        protected void MoveControllerDataToXslt(XsltArgumentList args){
            foreach (string o in MyController.PropertyBag.Keys){
                var value = MyController.PropertyBag[o];
                if (null != value){
                    var asParameter = false;
                    if (value.GetType().IsValueType || value is string){
                        asParameter = true;
                    }
                    else{
                        foreach (var type in parameterTypes){
                            if (type.IsAssignableFrom(value.GetType())){
                                asParameter = true;
                            }
                        }
                    }
                    if (asParameter){
                        args.AddParam(o, string.Empty, value);
                    }
                    else{
                        args.AddExtensionObject(namespaceBase + o, value);
                    }
                }
            }
        }

        protected XslCompiledTransform LoadXslt(){
            return LoadXslt(GetXsltPath());
        }

        protected virtual XslCompiledTransform LoadXslt(string path){
            var xslt = new XslCompiledTransform();
            xslt.Load(path, XsltSettings.TrustedXslt, new XmlUrlResolver());
            return xslt;
        }

        protected virtual IXPathNavigable getInputDocument(){
            return "<xml/>".asXPathNavigable();
        }

        private string GetXsltPath(){
            var vpath = "~/" + MyController.ViewFolder + "/" + ViewName + ".xslt";
            vpath = vpath.Replace("//", "/");
            return MyController.Context.Server.MapPath(vpath);
        }

        protected void prepareContentType(Controller controller){
            if (Type.hasContent() || UseZipPackage){
                var contentType = string.Empty;
                contentType = DefaultContentType();
                controller.Response.ContentType = contentType;
                if (UseZipPackage){
                    controller.Response.ContentType = MediaTypeNames.Application.Zip;
                }

                if (FileName.noContent()){
                    FileName = Definition.Name;
                }
                if (FileName.hasContent()){
                    controller.Response.AppendHeader("Content-Disposition",
                                                     "filename=\"" + FileName.Replace("\"", "'") +
                                                     (UseZipPackage ? ".zip" : (Type.hasContent() ? "." + Type : ".xml")) +
                                                     "\"");
                }
            }
        }

        public string DefaultContentType(){
            if ("doc" == Type){
                return "Application/vnd.ms-word";
            }
            if ("xls" == Type){
                return "Application/vnd.ms-excel";
            }
            if ("svg" == Type){
                return "image/svg+xml";
            }
            if(this.Type.Contains("/")) {
                return this.Type;
            }
            return MediaTypeNames.Text.Html;
        }

        protected void WriteOutContent(Controller controller, string report){
            if (!UseZipPackage){
                controller.RenderText(report);
            }
            else{
                if (FileName.noContent()){
                    FileName = Definition.Name;
                }
                var fileName = Path.GetTempFileName();
                var p = Package.Open(fileName, FileMode.Create);
                var u = new Uri("/" + FileName.toSystemName() + (Type.yes() ? "." + Type : ".html"),
                                UriKind.Relative);
                var file = p.CreatePart(u, DefaultContentType(), CompressionOption.Normal);

                var s = file.GetStream(FileMode.Create);
                using (var w = new StreamWriter(s)){
                    w.Write(report);
                    w.Flush();
                }
                p.Close();
                controller.CancelView();
                controller.Response.WriteFile(fileName);
            }
        }

        public static BooViewEngine GetBrail(Controller controller){
            if (null == Engine){
                var eng = new BooViewEngine();
                var source = new FileAssemblyViewSourceLoader(controller.Context.Server.MapPath("~/usr/views"));
                source.AddPathSource(controller.Context.Server.MapPath("~/mod/views"));
                source.AddPathSource(controller.Context.Server.MapPath("~/sys/views"));
                eng.SetViewSourceLoader(source);
                eng.Options.BaseType = typeof (MyBrailBase).FullName;
                eng.Options.SaveToDisk = false;
                eng.Options.SaveDirectory = controller.Context.Server.MapPath("~/tmp/Container.getbrail");
                eng.Initialize();
                source.ViewChanged += _getViewChanged;
                Engine = eng;
            }

            return Engine;
        }

        private static void _getViewChanged(object sender, FileSystemEventArgs e){
            lock (sync){
                Thread.Sleep(1000);

                Engine.Initialize();
            }
        }

        protected string getBrailResult(Controller controller){
            var eng = GetBrail(controller);
            string report;
            var v = new StringWriter();
            var path = "";
            if (this.Definition != null && ((IReportDefinitionDynamicView)this.Definition).RenderView.hasContent()
                /*&& ((IReportDefinitionDynamicView)this.Definition).RenderView.EndsWith("/render")*/)
            {
                path = "report/" + ((IReportDefinitionDynamicView)this.Definition).RenderView;
            }
            else{
                path =
                    string.Format("report/{0}/{1}", controller.AreaName,
                                  ViewName);
            }
            eng.Process(path, v, MyController.Context, MyController, MyController.ControllerContext);
            report = v.ToString();
            return report;
        }

        protected string PrepareContentToOut(Controller controller){
            var report = string.Empty;

            //TODO: на практике дефолтная мода - brail
            if (Mode.noContent() || Mode == "xslt"){
                report = PerformXsltTransformation(GetXsltPath(controller, GetXsltName(controller)));
            }
            else if (Mode == "brail"){
                report = getBrailResult(controller);
            }
            return report;
        }


        protected virtual string GetXsltName(Controller controller){
            return ViewName;
        }

        protected virtual string GetXsltPath(Controller controller, string myView){
            var viewName = string.Format("views/report/{0}{1}.xslt", myView, Type);
            return Container.get<IFilePathResolver>().Resolve(viewName);
        }
    }
}