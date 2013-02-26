using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Parser;

namespace Comdiv.Extensibility{
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

        public static void Prepare(Parse result)
        {
#if LIB2
            if(-1!=result.Find(typeof(Parsing))){
                result.InsertAfter(typeof (Parsing), new IncludeAstMacroExpandStep());
            }else if (-1!=result.Find(typeof(WSABooParsingStep))){
                result.InsertAfter(typeof(WSABooParsingStep), new IncludeAstMacroExpandStep());
            }
#else
            if (-1 != result.Find(typeof(BooParsingStep)))
            {
                result.InsertAfter(typeof(BooParsingStep), new IncludeAstMacroExpandStep());
            }
            else if (-1 != result.Find(typeof(WSABooParsingStep)))
            {
                result.InsertAfter(typeof(WSABooParsingStep), new IncludeAstMacroExpandStep());
            }
#endif
        }
    }
}