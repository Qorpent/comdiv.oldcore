// // Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// // Supported by Media Technology LTD 
// //  
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //  
// //      http://www.apache.org/licenses/LICENSE-2.0
// //  
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// // 
// // MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Boo.Lang.Compiler.Steps;
using Comdiv.Extensions;
using Comdiv.Xml;

namespace Comdiv.Booxml{
    public class ApplyGlobalsAndSubstitutionsStep : AbstractCompilerStep{
        public override void Run(){
            if (!Context.Parameters.Defines.ContainsKey("apply_globals")){
                return;
            }
            var globals = new Dictionary<string, string>();

            foreach (var def in Context.Parameters.Defines){
                globals[def.Key] = def.Value;
            }
            var lateglobals = new List<string>();
            var descriptor = Context.Properties["xml"] as XElement;
            prepareGlobals(descriptor, globals, lateglobals);
            applyGlobals(descriptor, globals);

            Context.Properties["xml"] = descriptor;
        }

        public static void applyGlobals(XElement e, IDictionary<string, string> globals){
            foreach (var s in e.Attributes()){
                if (e.Name.LocalName == "global" && (s.Name.LocalName == "id" || s.Name.LocalName == "code")){
                    continue;
                }
                s.Value = resolveGlobals(s.Value, globals);
            }
            foreach (var text in e.Nodes().OfType<XText>()){
                text.Value = resolveGlobals(text.Value, globals);
            }


            foreach (var element in e.Elements()){
                applyGlobals(element, globals);
            }
        }

        public static string resolveGlobals(string value, IDictionary<string, string> globals){
            return value.replace(@"(?-i)@?((\p{Lu}|_)[\p{Lu}\d_]+)",
                                 m =>{
                                     var name = m.Groups[1].Value;
                                     if (globals.ContainsKey(name)){
                                         return globals[name];
                                     }
                                     return m.Value;
                                 });
        }


        public static void prepareGlobals(XElement descriptor, IDictionary<string, string> globals,
                                          IList<string> lateglobals){
            globals.Clear();

            foreach (var element in descriptor.Elements("global")){
                var name_ = element.attr("id").ToUpper();
                if (element.Elements().yes()){
                    lateglobals.Add(name_);
                    continue;
                    ;
                }

                var names = name_.split();

                var value = element.Value;
                foreach (var name in names){
                    if (!globals.ContainsKey(name)){
                        globals[name] = value;
                    }
                }
            }
        }

        public static void prepareLateGlobals(XElement descriptor, IDictionary<string, string> globals,
                                              IList<string> lateglobals){
            foreach (var lateglobal in lateglobals){
                var global = descriptor.XPathSelectElement("global[@id='" + lateglobal + "']");
                globals[lateglobal] = global.Value;
            }
        }

        public static void applyGenerators(XElement descriptor){
            var gens = descriptor.XPathSelectElements(".//generator").ToArray();
            foreach (var gen in gens){
                var cls = gen.attr("id");
                var generator = cls.toType().create<IXmlGenerator>();
                var production = generator.Generate(gen);
                if (production == null){
                    gen.Remove();
                }
                else{
                    gen.ReplaceWith(production);
                }
            }
        }

        public static void resolveSubstitutions(XElement descriptor){
            var substitutions = descriptor.Elements("subst").Select(
                x =>{
                    var result = new Substitution();
                    x.applyTo(result);
                    return result;
                }
                ).ToArray();
            foreach (var substitution in substitutions){
                var targets = descriptor.XPathSelectElements(substitution.From).ToArray();
                foreach (var target in targets){
                    var id = target.attr("id");
                    var newval = target.Value;
                    var xpath = substitution.To + "[@id='" + id + "']";
                    XElement source = null;
                    if (substitution.UseFirst) {
                       source = descriptor.XPathSelectElements(xpath).FirstOrDefault();
                    }
                    else{
                        source =  descriptor.XPathSelectElements(xpath).LastOrDefault();
                    }
                 
                    if (null == source){
                        throw new Exception("cannot substitute " + target.ToString().Replace("<", "&lt;"));
                    }
                    if ((substitution.As.noContent())){
                        substitution.As = source.Name.LocalName;
                    }
                    if (substitution.Elements){
                        target.ReplaceWith(source.Elements());
                    }
                    else{
                        var newe = new XElement(substitution.As);
                        foreach (var attr in source.Attributes()){
                            newe.Add(attr);
                        }
                        foreach (var node in source.Nodes()){
                            newe.Add(node);
                        }
                        if (!String.IsNullOrEmpty(newval)){
                            newe.Value = newval;
                        }
                        foreach (var attribute in target.Attributes()){
                            if (attribute.Name.LocalName.isIn("From", "To", "As", "Elements")){
                                continue;
                            }
                            newe.SetAttributeValue(attribute.Name, attribute.Value);
                        }
                        target.ReplaceWith(newe);
                    }
                }
            }
        }
    }
}