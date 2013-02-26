using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public class RulePassiveGroup : RuleGroup{
        protected override bool innerTest(IRuleContext context){
            return false;
        }

        protected override bool preTest(IRuleContext context, out bool result){
            result = false;
            return false;
        }
    }
}