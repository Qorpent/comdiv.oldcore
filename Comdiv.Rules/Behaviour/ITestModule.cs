using Comdiv.Rules.Context;

namespace Comdiv.Rules
{
	public interface ITestModule
	{
		bool Test(IRuleContext context);
	}
}