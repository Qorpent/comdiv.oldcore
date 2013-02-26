using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Activation;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public class ModuleActivationChecker : IRuleActivationChecker{
        private readonly IList<string> alwaysActive = new List<string>();
        private readonly IList<string> alwaysPassive = new List<string>();

        public IList<string> AlwaysActive{
            get { return alwaysActive; }
        }

        public IList<string> AlwaysPassive{
            get { return alwaysPassive; }
        }

        #region IRuleActivationChecker Members

        public RuleActivationState GetActivationState(IRuleContext context, IRule rule){
            if (AlwaysPassive.Contains(rule.Module())){
                return RuleActivationState.Never();
            }
            if (AlwaysActive.Contains(rule.Module())){
                return RuleActivationState.Always();
            }
            var modules = context.modules();
            if (null == modules){
                if (rule.Module() == "default"){
                    return RuleActivationState.Always();
                }
                else{
                    return RuleActivationState.Never();
                }
            }
            else{
                if (modules.IsActive(rule.Module())){
                    return RuleActivationState.ActiveVersion(GetVersion(context));
                }
                else{
                    return RuleActivationState.PassiveVersion(GetVersion(context));
                }
            }
        }

        public int GetVersion(IRuleContext context){
            var modules = context.modules();
            if (null == modules){
                return -1;
            }
            else{
                return modules.Version;
            }
        }

        #endregion
    }
}