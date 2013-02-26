using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace Comdiv.Extensibility.Brail
#endif
{
    public class OutputlMacro : LexicalInfoPreservingMacro
    {
        
        protected override Statement ExpandImpl(MacroStatement macro)
        {
            var result = new MacroStatement("output");
            foreach (var argument in macro.Arguments){
                result.Arguments.Add(argument);
                
            }
            result.Arguments.Add(new StringLiteralExpression(Environment.NewLine));
            return result;
        }
    }
}