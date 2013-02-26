using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Comdiv.Extensibility.Boo.Dsl;

namespace Comdiv.Framework.ComdivExMacro
{
    public class ControllerAttribute : AbstractAstAttribute
    {
        public override void Apply(Node targetNode) {
            var cls = targetNode as ClassDefinition;
            if(cls==null) {
                this.Context.Errors.Add(new CompilerError(this.LexicalInfo,
                                                          "ControllerAttribute is applyable only to classes"));
                return;
            }
            targetNode.GetEnclosingModule().Imports.Add(new Import(this.LexicalInfo,"Comdiv.Controllers"));
             cls.BaseTypes.Clear();
             cls.BaseTypes.Insert(0, new SimpleTypeReference("Comdiv.Controllers.ExtensionBaseController"));
            if(!cls.Name.EndsWith("Controller")) {
                cls.Name = cls.Name + "Controller";
            }
            var body = targetNode.GetEnclosingModule()["regmethod"] as Block;
            var registrycall = new SlicingExpression(new ReferenceExpression("registry"), new StringLiteralExpression(cls.Name.ToLower()));
            var assign = new ReferenceExpression(cls.Name);
            var aexpr = new BinaryExpression(BinaryOperatorType.Assign, registrycall, assign);
            body.Add(aexpr);
        }
    }
}
