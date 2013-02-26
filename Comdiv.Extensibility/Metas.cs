using Boo.Lang;
using Boo.Lang.Compiler.Ast;
using Comdiv.Extensibility.Boo.Dsl;

namespace Comdiv.Extensibility
{
    public class Metas
    {
        [Meta]
        public static Statement read (Expression expression){
            Declaration declaraion = new Declaration();
            declaraion.Name = "readedContent";
            Expression arg = null;
            
            if (expression is BinaryExpression){
                var ass = expression as BinaryExpression;
                declaraion.Name=ass.Left.LiftToString();
                arg = ass.Right;
            }else{
                arg = expression;
            }
            MethodInvocationExpression assign = AstUtil.CreateMethodInvocationExpression(AstUtil.CreateReferenceExpression("System.IO.File.ReadAllText"), arg);
            var result =  new DeclarationStatement(declaraion, assign);
            return result;
        }
    }
}
