using Comdiv.Rules.Context;

namespace Comdiv.Rules.Support
{
	public class RuleContextPartametersContainer : ParametersContainer<IRuleContext>
	{
		public RuleContextPartametersContainer(IRuleContext target){
			Target = target;
		}
	}
}