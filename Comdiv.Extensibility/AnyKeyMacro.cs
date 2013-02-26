using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility{
    public class AnykeyMacro : AbstractAstMacro
    {
        public override Statement Expand(MacroStatement macro){
            var result = new Block();
            result.Statements.Add(
                new ExpressionStatement(
                    new MethodInvocationExpression(
                        AstUtil.CreateReferenceExpression("System.Console.WriteLine"),
                        new StringLiteralExpression("Press any key...")
                        )
                    )
                );
            result.Statements.Add(
                new ExpressionStatement(
                    new MethodInvocationExpression(
                        AstUtil.CreateReferenceExpression("System.Console.ReadKey")
                        )
                    )
                );
            return result;
        }
    }
}