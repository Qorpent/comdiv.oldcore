using System.Text;
using System.Xml.Linq;
using Boo.Lang.Compiler.Ast;
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
    public class AssignsLiteralsAsTextStep : BooxmlMacroVisitorBase
    {
        protected override void innerOnMacro(MacroStatement node)
        {
            this.CurrentText = new StringBuilder();
            Visit(node.Body.Statements.OfType<ExpressionStatement>().ToArray());
            var value = CurrentText.ToString().Trim();
            if ((!string.IsNullOrWhiteSpace(value))) {
                value = value.Replace("_QUOTED_USD_", "$");
                if (value.Contains("<")){
                    Element.Add(new XCData(value));
                }
                else{
                    Element.SetValue(value);
                }
            }
        }

        protected StringBuilder CurrentText { get; set; }

        public override void OnExpressionStatement(ExpressionStatement node)
        {
            if (node.Expression is LiteralExpression)
            {
                CurrentText.AppendLine( node.Expression.expand());
            }
            else if(node.Expression is UnaryExpression){
                var u = node.Expression as UnaryExpression;
                if(u.Operator==UnaryOperatorType.UnaryNegation){
                    CurrentText.Append("-");
                    CurrentText.Append(u.Operand.ToCodeString());
                }
            }else if(node.Expression is ExpressionInterpolationExpression){
                CurrentText.Append(node.Expression.expand());
            }
            else{
                base.OnExpressionStatement(node);
            }
        }


    }
}