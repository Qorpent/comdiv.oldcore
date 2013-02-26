using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;
using Comdiv.Rules.Engine;

namespace Comdiv.Rules{
    public class RuleTriggerGroup : RulePassiveGroup, ITriggerCollection{
        #region ITriggerCollection Members

        public void ExecuteTriggers(IRuleContext context, RuleExecutionPhase phase){
            //@"context".contract_NotNull(context);
            foreach (var rule in Rules){
                if (rule.IsTriggerForPhase(phase)){
                    if (rule.Test(context)){
                        rule.Execute(context);
                    }
                }
            }
        }

        #endregion
    }
}