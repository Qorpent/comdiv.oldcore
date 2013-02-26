using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Comdiv.Extensibility.Boo.Dsl;
using Attribute = Boo.Lang.Compiler.Ast.Attribute;

namespace Comdiv.Framework.ComdivExMacro
{
	public class quickAttribute:AbstractAstAttribute
	{
		public override void Apply(Node targetNode) {
			targetNode.GetEnclosingModule().Imports.Add(new Import(this.LexicalInfo, "Comdiv.Framework"));
			targetNode.GetEnclosingModule().Imports.Add(new Import(this.LexicalInfo, "Comdiv.Framework.Quick"));
			
			tryClass(targetNode);
			tryMethod(targetNode);
		}

		private void tryMethod(Node targetNode) {
			var method = targetNode as Method;
			if (method == null)
			{
				return;
			}
			var processmethod = method.Clone() as Method;
			processmethod.Name = "process";
			processmethod.Modifiers = TypeMemberModifiers.Public | TypeMemberModifiers.Override;
			processmethod.Parameters.Clear();
			//processmethod.Parameters.Add(new ParameterDeclaration("context", new SimpleTypeReference("Comdiv.Framework.Quick.QWebContext")));
			processmethod.ReturnType = new SimpleTypeReference("System.Object");
			var name = method.Name;
			var cls = new ClassDefinition();
			cls.Name = name;
			cls.Members.Add(processmethod);
			method.ParentNode.Replace(method, cls);
			tryClass(cls);
		}

		private void tryClass(Node targetNode) {
			var cls = targetNode as ClassDefinition;
			if (cls == null)
			{
				return;
			}
			if (cls.BaseTypes.Count == 0)
			{
				cls.BaseTypes.Insert(0, new SimpleTypeReference("Comdiv.Framework.Quick.QuickBase"));
			}
			var attr = new Attribute("Comdiv.Framework.Quick.actionAttribute");
			attr.Arguments.Add(new StringLiteralExpression(cls.Name.Replace("_", ".")));
			cls.Attributes.Add(attr);

			var body = targetNode.GetEnclosingModule()["regmethod"] as Block;
			var registrycall = new SlicingExpression(new ReferenceExpression("registry"), new StringLiteralExpression(cls.Name.Replace("_", ".") + ".quick"));
			var assign = new ReferenceExpression(cls.Name);
			var aexpr = new BinaryExpression(BinaryOperatorType.Assign, registrycall, assign);
			body.Add(aexpr);
		}
	}
}
