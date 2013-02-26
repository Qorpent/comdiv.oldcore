using System.Collections.Generic;
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
    public abstract class BooxmlMacroVisitorBase : AbstractVisitorCompilerStep {
        public MacroStatement currentMacro;

        public IList<XNode> Xml{
            get{
                if(!currentMacro.ContainsAnnotation("xml")){
                    currentMacro.Annotate("xml",new List<XNode>());
                }
                return currentMacro["xml"] as IList<XNode>;
            }
        }

        public Expression Arg{
            get{
                if(currentMacro.Arguments.Count==0) return null;
                return currentMacro.Arguments[0];
            }
        }

        public IList<Expression> Simples{
            get{
                return
                    currentMacro.Arguments.Where(x => (x is LiteralExpression) || (x is ReferenceExpression) || (x is ExpressionInterpolationExpression)).ToList();
            }
        }

        public XElement Element{
            get{
                if(Xml.Count==0 || !(Xml[0] is XElement)){
                    return null;
                }
                return Xml[0] as XElement;
            }
            set{
                if(Element==null){
                    Xml.Insert(0,value);
                }else{
                    Xml[0] = value;
                }
            }
        }

        public override void Run(){
            if (Context.CompileUnit.Modules.Count != 0){
                if (Context.CompileUnit.Modules[0].Globals != null){
                    Visit(Context.CompileUnit.Modules[0].Globals);
                }
            }
        }

        public override void OnMacroStatement(MacroStatement node)
        {
            this.currentMacro = node;
            innerOnMacro(node);
            base.OnMacroStatement(node);
        }

        protected abstract void innerOnMacro(MacroStatement node);
    }
}