
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public interface IRuleActivationChecker{
        RuleActivationState GetActivationState(IRuleContext context, IRule rule);
        int GetVersion(IRuleContext context);
    }
}