// Copyright 2007-2009 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// MODIFICATIONS HAVE BEEN MADE TO THIS FILE


using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility{
    public static class BooExtensions{
        public static T get<T>(this Node node, string name)
        {
            if (!node.ContainsAnnotation(name)) return default(T);
            return (T)node[name];
        }
        public static void set(this Node node, string name, object obj)
        {
            node[name] = obj;
        }
        public static MacroStatement findMacroContainer(this MacroStatement statement, string name)
        {
            if (null == statement) return null;
            var s = (Node)statement;
            do
            {
                if (s is MacroStatement && ((MacroStatement)s).Name == name)
                {
                    return (MacroStatement)s;
                }
                s = s.ParentNode;
            } while (s != null);
            return (MacroStatement)s;
        }

        public static Module findModule(this Node node)
        {
            var current = node;
            while (null != current && !(current is Module))
            {
                current = current.ParentNode;
            }
            return (Module)current;
        }

        public static Block extractMethodBody(this MacroStatement macro){
            var result = new Block();
            if(!macro.Body.IsEmpty){
                if(macro.Body.Statements.Count==1 &&
                    ((macro.Body.Statements[0] is ExpressionStatement)||
                    (macro.Body.Statements[0] is MacroStatement && 
                        ((MacroStatement)macro.Body.Statements[0]).Arguments.Count==0
                        &&
                        ((MacroStatement)macro.Body.Statements[0]).Body.Statements.Count==0)
                    )){
                    result.Add(new ReturnStatement(new ReferenceExpression(macro.Body.Statements[0].ToCodeString())));
                }else{
                    foreach (var statement in macro.Body.Statements){
                      result.Add(statement);  
                    }
                }
            }else{
                if(macro.Arguments.Count>0){
                    result.Add(new ReturnStatement(macro.LexicalInfo,macro.Arguments[0]));
                }
            }
            return result;
        }

        public static CastExpression cast(this Expression exp, string reference)
        {
            return new CastExpression(exp, new SimpleTypeReference(reference));
        }
        public static string fromLiteral(this Node node){
            if(node is StringLiteralExpression){
                return ((StringLiteralExpression) node).Value;
            }
            return node.ToCodeString();
        }
        public static Expression toLiteral(this object val)
        {
            Expression value = val as Expression;
            if (null == val)
            {
                value = new NullLiteralExpression();
            }
            else if (val is string)
            {
                if (val.ToString().StartsWith("@"))
                {
                    value = new ReferenceExpression(val.ToString().Substring(1));
                }
                else
                {
                    value = new StringLiteralExpression(val.ToString());
                }
            }
            else if (val is bool)
            {
                value = new BoolLiteralExpression((bool)val);
            }
            else if (val is int)
            {
                value = new IntegerLiteralExpression((int)val);
            }
            return value;
        }
        public static ExpressionInterpolationExpression append(this ExpressionInterpolationExpression expression, string str)
        {
            return expression.append(str.toLiteral());
        }

        public static ExpressionInterpolationExpression append(this ExpressionInterpolationExpression expression,Expression node){
            if(node is ExpressionInterpolationExpression){
                foreach (var ex in ((ExpressionInterpolationExpression)node).Expressions){
                    expression.Expressions.Add(ex);
                }
            }else{
                expression.Expressions.Add(node);
            }
            return expression;
        }

        public static BinaryExpression assign(this Expression exp, object val)
        {
            Expression value = toLiteral(val);
            return new BinaryExpression(BinaryOperatorType.Assign, exp, value);
        }

        public static IEnumerable<Expression> expandIfCollection(this Expression e){
            if(null==e){
                yield break;
                
            }
            if(e is ListLiteralExpression){
                foreach (var item in ((ListLiteralExpression)e).Items){
                    yield return item;
                }
                yield break;
            }
            yield return e;
        }
    }
}