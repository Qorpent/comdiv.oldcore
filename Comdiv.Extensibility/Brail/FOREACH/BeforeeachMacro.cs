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

using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public partial class ForeachMacro{
        private Block extractMainItemBlock(MacroStatement macro, Func<MacroStatement, Block> extract,
                                           MacroStatement onitem_macro, Expression item, string prefix, string suffix){
            Block onitem = null;
            if (onitem_macro == macro && (macro.Body == null || macro.Body.IsEmpty)){
                Expression outer = item;
                if (prefix != "" || suffix != ""){
                    outer = new ExpressionInterpolationExpression()
                        .append(prefix)
                        .append(item)
                        .append(suffix);
                }
                onitem = new Block().add(BrailBuildingHelper.WriteOut(outer));
            }
            else{
                onitem = extract(onitem_macro);
            }

            return onitem;
        }

        #region Nested type: BeforeeachMacro

        public class BeforeeachMacro : LexicalInfoPreservingMacro{
            protected override Statement ExpandImpl(MacroStatement macro){
                macro.findMacroContainer("foreach").set("beforeeach", macro);
                return null;
            }
        }

        #endregion
    }
}