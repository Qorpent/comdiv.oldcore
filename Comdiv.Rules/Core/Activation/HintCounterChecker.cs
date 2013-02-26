using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public class HintCounterChecker : IRuleActivationChecker{
        #region IRuleActivationChecker Members

        public RuleActivationState GetActivationState(IRuleContext context, IRule rule){
            if (!rule.IsWithCountHints()){
                return RuleActivationState.Always();
            }
            if (-1 != rule.GetMaxExecCountHint()){
                if (context.execCount(rule) == rule.GetMaxExecCountHint()){
                    return RuleActivationState.Never();
                }
            }

            if (-1 != rule.GetMaxBadTestCountHint()){
                if (context.badCount(rule) == rule.GetMaxBadTestCountHint()){
                    return RuleActivationState.Never();
                }
            }
            return new RuleActivationState(RuleActivationStateType.Active);
        }

        public int GetVersion(IRuleContext context){
            return -1;
        }

        #endregion
    }
}