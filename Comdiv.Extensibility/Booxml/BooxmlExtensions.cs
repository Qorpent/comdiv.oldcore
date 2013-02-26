using System.Collections.Generic;
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
    public static class BooxmlExtensions{
        public static IEnumerable<XNode> xml(this Node node){
            if (node.ContainsAnnotation("xml")){
                var result = node["xml"];
                if(result is XNode){
                    return new XNode[]{result as XNode};
                }
                return result as IEnumerable<XNode>;
            }
            return new XNode[]{};
        }


        public static bool isname(this Expression expression){
            if(null==expression) return false;
            if(expression is StringLiteralExpression) return true;
            if (expression is ReferenceExpression) return true;
            return false;
        }
        
        public static string expand(this Expression expression){
            if (null == expression) return "";
            if (expression is ExpressionInterpolationExpression) {
                var str = "";
                foreach (var e in ((ExpressionInterpolationExpression)expression).Expressions) {
                    if(e is LiteralExpression) {
                        str += ((LiteralExpression) e).ValueObject.toStr();
                    }else {
                        str += "${" + e.ToCodeString() + "}";
                    }
                }
                return str;
            }
            if (expression is StringLiteralExpression) return ((LiteralExpression) expression).ValueObject.ToString();
            if (expression is LiteralExpression) return ((LiteralExpression) expression).ValueObject.ToString().ToLower();
            if (expression is UnaryExpression){
                var u = expression as UnaryExpression;
                if(u.Operator==UnaryOperatorType.UnaryNegation){
                    return "-" + u.Operand.ToCodeString();
                }
            }
            return expression.ToCodeString();
        }
    }
}