using Boo.Lang.Compiler.Steps;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Brail {
	public class InterpolationUnescapeStep : AbstractVisitorCompilerStep {
		public override void Run() {
			Visit(_context.CompileUnit.Modules);
		}

		public override void OnStringLiteralExpression(global::Boo.Lang.Compiler.Ast.StringLiteralExpression node) {
			if (node.Value.hasContent() && node.Value.Contains("_QUOTED_USD_")) {
				var val = node.Value.Replace("_QUOTED_USD_", "$");
				node.Value = val;
			}
			else {
				base.OnStringLiteralExpression(node);
			}
		}
	}
}