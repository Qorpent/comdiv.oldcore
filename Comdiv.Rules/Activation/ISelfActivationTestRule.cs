
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public interface ISelfActivationTestRule{
        RuleActivationState GetActivationTest(IRuleContext context);
    }
}