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
using System.Linq;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public partial class ForeachMacro{
        #region Nested type: HeadsMacro

        public class HeadsMacro : CellMacroBase{
            protected override Statement ExpandImpl(MacroStatement macro){
                if (macro.Body != null && !macro.Body.IsEmpty){
                    foreach (ExpressionStatement st in macro.Body.Statements){
                        IEnumerable<Expression> args = st.Expression.expandIfCollection();
                        appendHead(macro, new CellDefinition{Value = args.First(), Attributes = args.Skip(1)});
                    }
                }
                else{
                    foreach (Expression argument in macro.Arguments){
                        IEnumerable<Expression> args = argument.expandIfCollection();
                        appendHead(macro, new CellDefinition{Value = args.First(), Attributes = args.Skip(1)});
                    }
                }
                return null;
            }
        }

        #endregion
    }
}