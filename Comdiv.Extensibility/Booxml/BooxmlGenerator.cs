//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System.Collections.Generic;
using System.Xml.Linq;
using Boo.Lang.Compiler.Ast;
using Comdiv.Extensions;

namespace Comdiv.Booxml {
    public class BooxmlGenerator {
        public string Generate(XElement xml) {
            var m = new Module {
                                   Namespace = new NamespaceDeclaration(xml.Name.LocalName)
                               };
            foreach (var element in xml.Elements()) {
                generateElement(element, m.Globals);
            }
            return m.ToCodeString();
        }

        private Expression getSimplified(string value) {
            if (value.like(@"^\d+$")) {
                return new IntegerLiteralExpression(value.toInt());
            }else
            if (value.like(@"^\s*\d\D")) {
                return new StringLiteralExpression(value.toStr());
            }else
            if (value.like(@"(^[a-zA-Zà-ÿÀ-ß][\w\d_]*$)")) {
                return new ReferenceExpression(value);
            }
            else 
             return new StringLiteralExpression(value);
        }

        private void generateElement(XElement element, Block block) {
            var m = new MacroStatement(element.Name.LocalName);

            block.Add(m);

            //if ((!string.IsNullOrWhiteSpace(element.Value))) {
            //    generateText(element.Value, m.Body);
            //}

            IList<string> skips = new List<string>();
            skips.Add("_line");
            skips.Add("_file");
            if (element.Attribute("id") != null) {
                skips.Add("id");
                if(element.attr("code")==element.attr("id")) {
                    skips.Add("code");
                }
                m.Arguments.Add(getSimplified(element.attr("id")));
                if (element.Attribute("name") != null) {
                    m.Arguments.Add(getSimplified(element.attr("name")));
                    skips.Add("name");
                }
            }

            foreach (var attribute in element.Attributes()) {
                if (!skips.Contains(attribute.Name.LocalName)) {
                    generateAttribute(attribute, m.Body);
                }
            }
            foreach (var e in element.Nodes()) {
                if (e is XElement) {
                    generateElement((XElement)e, m.Body);
                }
                else if(e is XText){
                    generateText(((XText)e).Value,m.Body);
                }
            }
        }

        private void generateAttribute(XAttribute attribute, Block block) {
            var exp = new BinaryExpression(BinaryOperatorType.Assign, new ReferenceExpression(attribute.Name.LocalName),
                                           getSimplified(attribute.Value));
            if (attribute.Name.LocalName == "type" || attribute.Name.LocalName == "code") {
                ((MacroStatement) block.ParentNode).Arguments.Add(exp);
            }
            else {
                block.Add(exp);
            }
        }

        private void generateText(string value, Block block) {
            block.Add(new StringLiteralExpression(value));
        }
    }
}