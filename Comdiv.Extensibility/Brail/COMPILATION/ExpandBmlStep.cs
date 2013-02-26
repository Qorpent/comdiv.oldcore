// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
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
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public class ExpandBmlStep : AbstractTransformerCompilerStep{
        private bool allowall;
        private IList<string> exceptions;
        IList<MacroStatement> bmlelements = new List<MacroStatement>();

        public override void Run(){
            
            Visit(Context.CompileUnit.Modules);
            
        }

        private void collectTemplates(){
            foreach (var bml in bmlelements){
                var templates = GetTemplates(bml);
                foreach (var key in templates.Keys.ToArray()){
                    var clone = templates[key].CloneNode();
                    templates[key].ReplaceBy(null);
                    templates[key] = clone;
                }
            }
        }

        public override void OnModule(Module node){
            bmlelements = new List<MacroStatement>();
            exceptions = null;
            allowall = false;
            Visit(node.Members);
            Visit(node.Globals.Statements);
            collectTemplates();
        }

        public override void OnMethod(Method node){
            Visit(node.Body);
        }

        public override void OnMacroStatement(MacroStatement node){
            exceptions = exceptions ?? new List<string>();
            MacroStatement x = node;

            var name = node.Name;
            var simplename = name.Replace("_","");
            var _bml = node.findMacroContainer("bml"); 
            var bml =  _bml!= null;
            //set templates as exceptions
            if (bml && node.Name == "template"){
                var templatename = node.Arguments[0].ToCodeString();
                exceptions.Add(templatename);
                var templates = GetTemplates(_bml);                      
                templates[templatename] = node;
                //node.ReplaceBy(null);
            }
            else if (bml &&
                     (allowall && (!exceptions.Contains(simplename)))
                     ||
                     (!allowall && exceptions.Contains(simplename)))
            {
                //if (node.findMacroContainer("template") == null){
                    x = new MacroStatement("bmlelement");
                    x.Arguments.Add(new ReferenceExpression(simplename));
                    if(name==simplename||name.EndsWith("_")){
                        x.Arguments.Add(new ReferenceExpression("___start"));
                    }
                    if(name==simplename||name.StartsWith("_")){
                        x.Arguments.Add(new ReferenceExpression("___end"));
                    }
                    foreach (Expression argument in node.Arguments){
                        x.Arguments.Add(argument.CloneNode());
                    }

                    x.Body = node.Body.CloneNode();

                Visit(x.Body);

                    gradeReferencesAndStringsToBeWriteout(x.Body);
                    node.ReplaceBy(x);
                    
                //}
            }
            else if (node.Name == "bml"){
                bmlelements.Add(node);
                allowall = false;
                exceptions = new List<string>(BmlMacro.SupportedElements);
                if (null != node.Arguments.OfType<ReferenceExpression>().FirstOrDefault(n => n.ToCodeString() == "all")){
                    allowall = true;
                    exceptions = null;
                }
                BinaryExpression _exc_def = null;
                if (null !=
                    (_exc_def =
                     node.Arguments.OfType<BinaryExpression>().FirstOrDefault(n => n.Left.ToCodeString() == "ex"))){
                    var _ex = _exc_def.Right as ArrayLiteralExpression;
                    IList<string> _newexceptions = new List<string>();
                    foreach (Expression item in _ex.Items){
                        _newexceptions.Add(item.ToCodeString());
                    }
                    exceptions = _newexceptions.ToList();
                }
            }
            Visit(x.Body.Statements);
        }

        private IDictionary<string, MacroStatement> GetTemplates(MacroStatement _bml) {
            if (!_bml.ContainsAnnotation("templates"))
            {
                _bml["templates"] = new Dictionary<string, MacroStatement>();
            }
            return _bml.get<IDictionary<string, MacroStatement>>("templates");
        }

        

        private void gradeReferencesAndStringsToBeWriteout(Block block){
            
            if (block == null){
                return;
            }
            foreach (Statement statement in block.Statements){
                if (statement is ExpressionStatement){
                    Expression expr = ((ExpressionStatement) statement).Expression;
                    if (expr is BinaryExpression){
                        continue;
                       
                    }
                    if (expr is UnaryExpression) {
                        continue;
                    }
                    
                    if (expr is MethodInvocationExpression){
                        if(expr.ToCodeString().StartsWith("@")){
                            var method = expr as MethodInvocationExpression;
                            var oldtarget = method.Target;
                            method.Target = AstUtil.CreateReferenceExpression(oldtarget.ToCodeString().Substring(1));
                            
                        }else{
                            continue;
                        }
                     }

                    
                    //impossible by parsing notation situation
                    //if (expr is ReferenceExpression && expr.ToCodeString().StartsWith("@")){
                    //    expr = new ReferenceExpression(expr.ToCodeString().Substring(1));
                    //}

                    
                        statement.ReplaceBy(new ExpressionStatement(expr.writeOut()));
                   
                    continue;
                }
                if (statement is MacroStatement){
                    string name = ((MacroStatement) statement).Name;
                    if (name.StartsWith("@")){
                        statement.ReplaceBy(
                            new ExpressionStatement(new ReferenceExpression(name.Substring(1)).writeOut()));
                        continue;
                    }
                    if(name=="BODY"){
                        continue;
                        
                    }
                    if (block.Statements.Count == 1){
                        var m = ((MacroStatement) statement);
                        if (m.Arguments.Count == 0 && (m.Body == null || m.Body.IsEmpty)){
                            
                            statement.ReplaceBy(
                                new ExpressionStatement(new ReferenceExpression(name).writeOut()));
                            continue;
                        }
                       
                    }
                }
                if (statement is MacroStatement){
                    gradeReferencesAndStringsToBeWriteout(((MacroStatement) statement).Body);
                }
                if (statement is IfStatement){
                    gradeReferencesAndStringsToBeWriteout(((IfStatement) statement).TrueBlock);
                    gradeReferencesAndStringsToBeWriteout(((IfStatement) statement).FalseBlock);
                }
                if (statement is ForStatement){
                    gradeReferencesAndStringsToBeWriteout(((ForStatement) statement).Block);
                }
                if (statement is TryStatement){
                    gradeReferencesAndStringsToBeWriteout(((TryStatement) statement).ProtectedBlock);
                    gradeReferencesAndStringsToBeWriteout(((TryStatement) statement).EnsureBlock);
                    gradeReferencesAndStringsToBeWriteout(((TryStatement) statement).FailureBlock);
                }
            }
            /*
            if (x.Body.Statements.Count == 1)
            {
                if (x.Body.Statements[0] is MacroStatement)
                {
                    var refcand = (MacroStatement)x.Body.Statements[0];
                    if (refcand.Arguments.Count == 0 && (refcand.Body == null || !refcand.Body.HasStatements))
                    {
                        refcand.ReplaceBy(new ExpressionStatement(new ReferenceExpression(refcand.Name)));
                    }
                }
            }
             */
        }
    }
}