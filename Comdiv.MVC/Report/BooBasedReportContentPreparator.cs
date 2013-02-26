using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Parser;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Dom;
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

namespace Comdiv.Web{
    public class BooBasedReportContentPreparator : IReportContentPreparator{
        private readonly IDictionary<string, object> parameters = new Dictionary<string, object>();
        public BooEval Boo { get; set; }

        #region IReportContentPreparator Members

        public IReportControllerExtension Extension { get; set; }

        public IDictionary<string, object> Parameters{
            get { return parameters; }
        }

        public INode BuildContent(IReportControllerExtension extension, Controller controller){
            Extension = extension;
            var content = Stub.Create();
            var boo = Boo = buildEvaluator();


            SetupBooValues(boo, extension, controller, content);

            foreach (var booPath in GetBooPaths(controller, GetBooNames(controller))){
                try{
                    boo.EvaluateFile(booPath);
                }
                catch (BooErrorException ex){
                    writeOutErrors(content, ex);
                    break;
                }
            }
            return content;
        }

        public void SetValue(string name, object value){
            Parameters[name] = value;
            Boo.Interpreter.SetValue(name, value);
        }

        #endregion

        protected void SetupBooValues(BooEval boo, IReportControllerExtension extension, Controller controller,
                                      Stub content){
            SetValue("content", content);
            customBooSetup(boo, controller);
            Extension.CustomContentPreparatorPrepare(this);
        }

        protected virtual void customBooSetup(BooEval boo, Controller controller) {}


        protected virtual string[] GetBooNames(Controller controller){
            return new[]{Extension.ViewName};
        }

        protected virtual string[] GetBooPaths(Controller controller, string[] myBoos){
            var paths = new List<string>();
            foreach (var s in myBoos){
                paths.Add(controller.Context.Server.MapPath("~/views/report/" + s + ".boo"));
            }
            return paths.ToArray();
        }

        protected virtual BooEval buildEvaluator(){
            var result = new BooEval(new WSABooParsingStep()){
                                                                 RootDirectory =
                                                                     Extension.MyController.Context.Server.MapPath(
                                                                     "~/views/report")
                                                             };
            return result;
        }

        protected virtual void writeOutErrors(Stub content, BooErrorException ex){
            content.Head1("При создании отчета возникли ошибки!");
            content.LastNode.Classes.Add("warning");
            foreach (var error in ex.Errors){
                content.Paragraph(error.ToString());
            }
        }
    }
}