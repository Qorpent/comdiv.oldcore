// Copyright 2007-2009 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler.Ast;
using Comdiv.Extensions;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public class TemplateBindVisitor : DepthFirstVisitor{
        private List<Expression> args;
        private IDictionary<string, Expression> namedargs;
        private Statement block;

        public TemplateBindVisitor(IEnumerable<Expression> templatesource, IDictionary<string, Expression> namedsubsts,  Statement block)
        {
            args = templatesource.ToList();
            this.namedargs = namedsubsts;
            this.block = block;
        }

        public override void OnMacroStatement(MacroStatement node)
        {
            if (node.Name == "BODY")
            {
                node.ReplaceBy(this.block);
            }
            else{
                base.OnMacroStatement(node);
            }
        }
		public override void OnStringLiteralExpression(global::Boo.Lang.Compiler.Ast.StringLiteralExpression node)
		{
			if (node.Value.hasContent() && node.Value.Contains("_QUOTED_USD_"))
			{
				var val = node.Value.Replace("_QUOTED_USD_", "$");
				node.Value = val;
			}
			else
			{
				base.OnStringLiteralExpression(node);
			}
		}

        public override void  OnArrayLiteralExpression(ArrayLiteralExpression node)
        {
            var need_expansion = false;
            if(node.Items.Count==2 && node.Items[0].ToCodeString().StartsWith("@_")){
                need_expansion = true;
            }
            base.OnArrayLiteralExpression(node);
            if(need_expansion){
                if(node.Items[0] is NullLiteralExpression){
                    node.ParentNode.Replace(node, node.Items[1].CloneNode());
                }
                else{
                    node.ParentNode.Replace(node, node.Items[0].CloneNode());
                }
            }
        }
        public override void OnReferenceExpression(ReferenceExpression node){
            if(checkAll(node)){
                return;
            }
            
            if(checkSubstitution(node)){
                return;
            }

            //else{
                base.OnReferenceExpression(node);
            //}
        }

        private bool checkSubstitution(ReferenceExpression node) {
            Match m = Regex.Match(node.ToCodeString(), @"^@_([\d\w]+?)(_)?$", RegexOptions.Compiled);
            if (m.Success){

                int idx = 0;
                var isordered = int.TryParse(m.Groups[1].Value, out idx);
                bool accomodate_to_strings = m.Groups[2].Value != "";
                Expression exp = new NullLiteralExpression();
                if (isordered){
                    idx = idx - 1;    
                    if (idx < args.Count){
                        exp = args[idx];
                    }
                }else{
                    var name = m.Groups[1].Value;
                    if(namedargs.ContainsKey(name)){
                        exp = namedargs[name];

                    }

                }
                
                if (accomodate_to_strings && (exp is ReferenceExpression))
                {
                    if (exp.ToCodeString().StartsWith("@"))
                    {
                        exp = new ReferenceExpression(exp.ToCodeString().Substring(1));
                    }
                    else
                    {
                        exp = new StringLiteralExpression(exp.ToCodeString());
                    }
                }
                node.ParentNode.Replace(node, exp);
                return true;
            }
            return false;
        }

        private bool checkAll(ReferenceExpression node){
            if(node.ToCodeString()=="@__"){
                var list = new HashLiteralExpression();
                foreach (var namedarg in namedargs){
                    list.Items.Add(
                        new ExpressionPair(
                            namedarg.Key.toLiteral(),
                            namedarg.Value.CloneNode()
                            ));
                        
                }
                node.ParentNode.Replace(node, list);
                return true;
            }
            return false;
        }
    }
}