using Comdiv.Rules.KnowlegeBase;

namespace Comdiv.Rules.Config
{
	public interface IRuleProvider
	{
		void SetupRules(IKnowlegeBase parent);
	}
}