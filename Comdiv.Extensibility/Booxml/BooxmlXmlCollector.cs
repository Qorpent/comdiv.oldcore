using System;
using System.Xml.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;

using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Booxml{
    public class BooxmlXmlCollector : AbstractVisitorCompilerStep {
        XElement Current
        {
            get;
            set;
        }
        public XElement Target{
            get{
                if(!Context.Properties.ContainsKey("xml")){
                    Context.Properties["xml"] = null;
                }
                return Context.Properties["xml"] as XElement;
            }
            set{
                Context.Properties["xml"] = value;
            }
        }
        public override void Run(){
           
            var result = new XElement("root");
            Visit(Context.CompileUnit.Modules[0]);
        }
        public override void OnModule(Module node)
        {
            var root = "root";
            var name = node.Namespace;
            if(name!=null && (!string.IsNullOrWhiteSpace(name.Name))){
                root = name.Name;
            }
            Target = new XElement(root);
            Current = Target;
            Visit(node.Members);
            Visit(node.Globals);

        }
        public class CurrentHandler:IDisposable{
            private BooxmlXmlCollector collecor;

            public CurrentHandler(BooxmlXmlCollector collector){
                this.current = collector.Current;
                this.collecor = collector;
            }
            XElement current { get; set; }

            public void Dispose(){
                collecor.Current = current;
            }
        }
        public CurrentHandler enter(){
            return new CurrentHandler(this);
        }
        public override void OnMacroStatement(MacroStatement node)
        {
            using (enter()){
                defaultProcess(node);
                base.OnMacroStatement(node);    
            }
        }

        private void defaultProcess(Node n) {
            Current.Add(n.xml());
            var x = n.xml();
            if(null!=x && 1==x.Count() && x.FirstOrDefault() is XElement){
                Current = x.FirstOrDefault() as XElement;
            }
        }
    }
}