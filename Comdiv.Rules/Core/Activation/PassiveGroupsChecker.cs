using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public class PassiveGroupsChecker : IRuleActivationChecker{
        #region IRuleActivationChecker Members

        public RuleActivationState GetActivationState(IRuleContext context, IRule rule){
            if (rule is RulePassiveGroup){
                return RuleActivationState.Never();
            }
            return RuleActivationState.Always();
        }

        public int GetVersion(IRuleContext context){
            return -1;
        }

        #endregion
    }
}