using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Dom;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Web{
    public class BrailBasedReportContentPrepararor : IReportContentPreparator{
        private static readonly object sync = new object();
        private readonly IDictionary<string, object> parameters = new Dictionary<string, object>();

        private static BooViewEngine Engine { get; set; }

        #region IReportContentPreparator Members

        public IDictionary<string, object> Parameters{
            get { return parameters; }
        }

        public INode BuildContent(IReportControllerExtension extension, Controller controller){
            var content = Stub.Create();
            SetValue("content", content);
            Extension = extension;
            Extension.CustomContentPreparatorPrepare(this);
            CustomPrepare();
            processBrail(controller);
            return content;
        }

        public IReportControllerExtension Extension { get; set; }

        public void SetValue(string name, object value){
            Parameters[name] = value;
        }

        #endregion

        public virtual void CustomPrepare() {}


        protected BooViewEngine getBrail(Controller controller){
            if (null == Engine){
                var eng = new BooViewEngine(); eng.Initialize();
                eng.Options.SaveToDisk = false;
				eng.Options.BaseType = typeof(BrailBase).FullName;
                eng.Options.CommonScriptsDirectory = "CommonScripts";
                eng.Options.SaveDirectory = controller.Context.Server.MapPath("~/tmp/ioc.getreport");
                eng.ViewFactory = new MONORAILBrailTypeFactory(new BrailSourceResolver {Identity = "reportpreparator"},eng.Options);

                var source = new FileAssemblyViewSourceLoader(controller.Context.Server.MapPath("~/usr/views"));

                source.AddPathSource(controller.Context.Server.MapPath("~/mod/views"));
                source.AddPathSource(controller.Context.Server.MapPath("~/sys/views"));


                eng.SetViewSourceLoader(source);
                eng.Initialize();

                source.ViewChanged += _ViewChanged;
                Engine = eng;
            }
            return Engine;
        }

        private static void _ViewChanged(object sender, FileSystemEventArgs e){
            lock (sync){
                Thread.Sleep(1000);
                Engine.Initialize();
            }
        }

        protected void processBrail(Controller controller){
            var eng = getBrail(controller);
            var v = new StringWriter();
            var path = "";
            if(Extension.ViewName.EndsWith("/prepare")){
                path = "report/" + Extension.ViewName;
            }else{
                path = string.Format("report/{0}/{1}_prepare", controller.AreaName,
                              Extension.ViewName);
            }

            
                
            eng.Process(path, null, v, Parameters);
        }
    }
}