using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Parser;
using Castle.MonoRail.Views.Brail;
using MvcContrib.Comdiv.Extensibility.BooLang;

namespace MvcContrib.Comdiv.Extensibility.BooLang{
    public class IncludeAstMacroExpandStep : DepthFirstVisitor, ICompilerStep{
        private CompileUnit root;

        public override void OnMacroStatement(MacroStatement node)
        {
            if (node.Name == "includeast")
            {
                var expander = new IncludeastMacro();
                var statements = expander.ExpandGenerator(node);
                var block = new Block();
                foreach (var statement in statements){
                    block.Statements.Add((Statement)statement);
                }
                node.ReplaceBy(block);
            }
            else{
                base.OnMacroStatement(node);
            }
        }

        public void Dispose(){
            
        }

        public void Initialize(CompilerContext context){
            this.root = context.CompileUnit;
        }

        public void Run(){
            Visit(root);
        }

        public static void Prepare(Boo.Lang.Compiler.Pipelines.Parse result)
        {
            if(-1!=result.Find(typeof(BooParsingStep))){
                result.InsertAfter(typeof (BooParsingStep), new IncludeAstMacroExpandStep());
            }else if (-1!=result.Find(typeof(WSABooParsingStep))){
                result.InsertAfter(typeof(WSABooParsingStep), new IncludeAstMacroExpandStep());
            }
        }
    }
}