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
using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    /// <summary>
    /// NVelocity - like templated foreach operator
    /// </summary>
    /// <remarks>
    /// syntax :
    /// foreach [VARNAME=]COLLECTION[,INDEXVAR] :
    ///     [part,[part...]]
    ///     [snippet,[snippet...]]
    ///     [body] 
    /// if not VARNAME provided, "i" used , if no INDEXVAR provided, "_idx" used
    /// NOTE: if collection provided as literal ex.: (1,2,3), always use VARNAME, otherwise 
    /// NOTE: parser would parse foreach (1,2,3) as MethodCall, not macro, foreach i=(1,2,3) - work!
    ///
    /// 
    /// PARTS SYNTAX:
    ///     a) using string attribute:
    ///             partname "arg"
    ///     b) using body:
    ///             partname:
    ///                 output "arg"
    ///                 output "something else"
    ///                 call_to_database()
    /// 
    /// ALLOWED PARTS:
    ///     a) beforeall - code, executing before first element of collection
    ///     b) beforeach - code, executing before each element of collection
    ///     c) onitem - code, executing on every element (if not seted, default body of foreach macro used
    ///     d) onerror - code, executing if "onitem" code fails
    ///     e) aftereach - code, executing after each element of collection
    ///     f) between - code, executed between every two elements
    ///     g) afterall - code, executed after last element of collection
    ///     h) onempty - code, executing if collection is null or empty (by default where is no processing for such collections)
    /// full syntax see on WIKI
    /// </remarks>
    public partial class ForeachMacro : LexicalInfoPreservingMacro{
        protected override Statement ExpandImpl(MacroStatement macro){
            if (macro.Arguments.Count == 0){
                throw new Exception("need at least one IEnumerable parameter");
            }

            var result = new Block();
            Expression src = macro.Arguments[0];

            var idx = new ReferenceExpression("_idx");
            Expression item = new ReferenceExpression("i");
            var col = new ReferenceExpression("current_collection");
            if (src is BinaryExpression){
                item = ((BinaryExpression) src).Left;
                src = ((BinaryExpression) src).Right;
            }

            if (macro.Arguments.Count > 1){
                idx = (ReferenceExpression) macro.Arguments[1];
            }

            string prefix = macro.get<string>("_prefix") ?? "";
            string suffix = macro.get<string>("_suffix") ?? "";


            Func<string, MacroStatement> findmacro = s =>{
                                                         var r = macro.get<Node>(s);
                                                         if (null == r){
                                                             return null;
                                                         }
                                                         if (!(r is MacroStatement)){
                                                             var m = new MacroStatement("stub");
                                                             m.Body = new Block().add(r);
                                                             r = m;
                                                         }

                                                         return (MacroStatement) r;
                                                     };
            Func<MacroStatement, Block> extract = m =>{
                                                      if (null == m){
                                                          return null;
                                                      }
                                                      if (m.Body != null && !m.Body.IsEmpty){
                                                          if (m.Body.Statements.Count == 1){
                                                              if (m.Body.Statements[0] is ExpressionStatement){
                                                                  Expression _ex =
                                                                      ((ExpressionStatement) m.Body.Statements[0]).
                                                                          Expression;
                                                                  if (_ex is LiteralExpression ||
                                                                      !_ex.ToCodeString().ToLower().StartsWith("out")){
                                                                      return new Block().add(_ex.writeOut());
                                                                  }
                                                              }
                                                          }
                                                          return m.Body;
                                                      }
                                                      if (m.Arguments.Count == 0){
                                                          return null;
                                                      }
                                                      var r = new Block();
                                                      if (!(m.Arguments[0] is MethodInvocationExpression)){
                                                          r.Add(BrailBuildingHelper.WriteOut(m.Arguments[0]));
                                                      }
                                                      else{
                                                          r.Add(m.Arguments[0]);
                                                      }

                                                      return r;
                                                  };

            MacroStatement beforeall_macro = findmacro("beforeall");
            MacroStatement onitem_macro = findmacro("onitem") ?? macro;
            MacroStatement onerror_macro = findmacro("onerror");
            ;
            MacroStatement between_macro = findmacro("between");
            ;
            MacroStatement afterall_macro = findmacro("afterall");
            ;
            MacroStatement onempty_macro = findmacro("onempty");
            ;
            MacroStatement beforeeach_macro = findmacro("beforeeach");
            ;
            MacroStatement aftereach_macro = findmacro("aftereach");

            MacroStatement prepare_macro = findmacro("prepare");
            ;

            Block beforeall = extract(beforeall_macro);
            Block onitem = extractMainItemBlock(macro, extract, onitem_macro, item, prefix, suffix);
            Block onerror = extract(onerror_macro);
            Block between = extract(between_macro);
            Block afterall = extract(afterall_macro);
            Block onempty = null;

            bool proceed_on_empty = false;
            if (onempty_macro != null && onempty_macro.Arguments.Count != 0 &&
                onempty_macro.Arguments[0].ToCodeString() == "proceed"){
                proceed_on_empty = true;
            }
            else{
                onempty = extract(onempty_macro);
            }
            Block beforeeach = extract(beforeeach_macro);
            Block aftereach = extract(aftereach_macro);


//          _idx = 0

            Statement betweener = getBetweener(idx, between);
            result.Add(col.assign(new MethodInvocationExpression(new ReferenceExpression("_wrapcollection"), src)));
            result.Add(new ReferenceExpression("___proceed").assign(new BoolLiteralExpression(proceed_on_empty)));
            var mainblock = new Block();
            mainblock.Add(idx.assign(0));
            mainblock.Add(new IfStatement(
                              new BinaryExpression(BinaryOperatorType.Equality, new NullLiteralExpression(), col),
                              new Block().add(new BinaryExpression(BinaryOperatorType.Assign, col,
                                                                   new MethodInvocationExpression(
                                                                       new ReferenceExpression("_wrapcollection"),
                                                                       new ArrayLiteralExpression()))),
                              null
                              ));
            if (beforeall != null){
                mainblock.Add(beforeall);
            }
            var maincycle = new ForStatement();
            maincycle.Iterator = col;
            string declname = item.ToCodeString();
            TypeReference decltype = null;
            if (item is TryCastExpression){
                declname = ((TryCastExpression) item).Target.ToCodeString();
                decltype = ((TryCastExpression) item).Type;
            }
            maincycle.Declarations.Add(new Declaration(maincycle.LexicalInfo, declname, decltype));
            maincycle.Block = new Block();
            maincycle.ThenBlock = afterall;

            if (betweener != null){
                maincycle.Block.Add(betweener);
            }
            if (null != prepare_macro){
                maincycle.Block.Add(prepare_macro.Body);
            }
            if (null != beforeeach){
                maincycle.Block.Add(beforeeach);
            }
            if (onerror == null){
                maincycle.Block.Add(onitem);
            }
            else{
                var trycatch = new TryStatement();
                var exchandler = new ExceptionHandler();
                exchandler.Block = onerror;
                exchandler.Declaration = new Declaration("_ex", new SimpleTypeReference("System.Exception"));
                trycatch.ProtectedBlock = onitem;
                trycatch.ExceptionHandlers.Add(exchandler);
                maincycle.Block.Add(trycatch);
            }
            if (null != aftereach){
                maincycle.Block.Add(aftereach);
            }
            maincycle.Block.Add(new UnaryExpression(UnaryOperatorType.Increment, idx));

            mainblock.Add(maincycle);

            result.Add(
                new IfStatement(
                    getMainCondition(col),
                    mainblock,
                    onempty
                    )
                );

//          if null!=items and (items as IEnumerable).Cast[of System.Object]().Count() != 0:

            return result;
        }

        private Expression getMainCondition(Expression src){
            return new BinaryExpression(
                BinaryOperatorType.Or,
                new ReferenceExpression("___proceed"),
                new UnaryExpression(UnaryOperatorType.LogicalNot, getisempty(src))
                );
        }

        private Expression getisempty(Expression src){
            return new MethodInvocationExpression(AstUtil.CreateReferenceExpression("isempty"), src);
        }

        private Statement getBetweener(ReferenceExpression idx, Block between){
            if (null != between){
                return new IfStatement(
                    new BinaryExpression(BinaryOperatorType.GreaterThan, idx, 0.toLiteral()),
                    between, null
                    );
            }
            return null;
        }
    }
}