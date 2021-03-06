using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Parser;

namespace Castle.MonoRail.Views.Brail{
    //namespaceholder
}

namespace MvcContrib.Comdiv.Extensibility.BooLang
{
    public class IncludeastMacro : LexicalInfoPreservingGeneratorMacro
    {
        

        protected override IEnumerable<Node> ExpandGeneratorImpl(MacroStatement macro){
            string typename = macro.Arguments[0].ToCodeString() + ", " + macro.Arguments[1].ToCodeString();
            //for normalizing nested classes parsed as add operators
            typename = typename.Replace(" ", "");
            var type = Type.GetType(typename);
            var generator = (IAstIncludeSourceMVC)type.GetConstructor(Type.EmptyTypes).Invoke(null);
            return generator.Substitute(macro);
        }
    }

    public interface IAstIncludeSourceMVC{
        IEnumerable<Node> Substitute(MacroStatement callPointMacro);
    }

    public class BooCodeBasedAstIncludeSourceMvcMvc:IAstIncludeSourceMVC{
       
        protected string Code { get; set; }

        public IEnumerable<Node> Substitute(MacroStatement callPointMacro){
            var compiler = new BooCompiler();
            compiler.Parameters.Pipeline = new CompilerPipeline();
            compiler.Parameters.Pipeline.Add(new BooParsingStep());
            compiler.Parameters.Input.Add(new StringInput("_code_", Code));
            var compileresult = compiler.Run();
            
            Node target = callPointMacro.ParentNode;
            while(!(target==null||(target is TypeDefinition))){
                target = target.ParentNode;
            }
            if (null != target){
                foreach (var member in compileresult.CompileUnit.Modules[0].Members){
                    
                        ((TypeDefinition)target).Members.Add(member);
                    
                }
            }
            var result = new Block(callPointMacro.LexicalInfo);
            foreach (var statement in compileresult.CompileUnit.Modules[0].Globals.Statements)
            {
                yield return statement;
            }
        }
    }
}
