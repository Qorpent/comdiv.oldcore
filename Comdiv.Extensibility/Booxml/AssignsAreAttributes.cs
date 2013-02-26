using System;
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
    public class AssignsAreAttributes : BooxmlMacroVisitorBase
    {
        protected override void innerOnMacro(MacroStatement node)
        {
            Visit(node.Arguments.OfType<BinaryExpression>().ToArray());
            Visit(node.Body.Statements.OfType<ExpressionStatement>().ToArray());
        }
        public override void OnExpressionStatement(ExpressionStatement node)
        {
            var e = node as ExpressionStatement;
            if(null!=e && e.Expression is BinaryExpression){
                Visit(e.Expression as BinaryExpression);
            }
        }

        public override void OnBinaryExpression(BinaryExpression node)
        {
            if (node.ContainsAnnotation("AssignsAreAttributes_visited"))
                return;
            if (node.Operator == BinaryOperatorType.Assign)
            {
                var name = node.Left.expand();
                var value = node.Right.expand();

                Element.SetAttributeValue(name,value.Replace("_QUOTED_USD_","$"));
            }
            else{
                base.OnBinaryExpression(node);
            }
            node.Annotate("AssignsAreAttributes_visited");
        }


        
    }
}