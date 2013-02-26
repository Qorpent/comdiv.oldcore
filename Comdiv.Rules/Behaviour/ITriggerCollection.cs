using Comdiv.Rules.Context;
using Comdiv.Rules.Engine;

namespace Comdiv.Rules
{
	public interface ITriggerCollection
	{
		void ExecuteTriggers(IRuleContext context, RuleExecutionPhase phase);
	}
}