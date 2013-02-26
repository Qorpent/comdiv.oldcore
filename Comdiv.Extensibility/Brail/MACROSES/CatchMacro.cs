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
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public class CatchMacro : LexicalInfoPreservingMacro{
        protected override Statement ExpandImpl(MacroStatement macro){
            var result = new Block();
            Expression outvar = macro.Arguments.Count == 0 ? new ReferenceExpression("_out") : macro.Arguments[0];
            var tryer = new TryStatement();
            var protectblock = new Block();
            protectblock.add(new MethodInvocationExpression(new ReferenceExpression("_catchoutput")));
            protectblock.add(macro.Body);
            tryer.ProtectedBlock = protectblock;
            tryer.EnsureBlock =
                new Block().add(outvar.assign(new MethodInvocationExpression(new ReferenceExpression("_endcatchoutput"))));
            result.Add(outvar.assign(""));
            result.Add(tryer);
            return result;
        }
    }
}