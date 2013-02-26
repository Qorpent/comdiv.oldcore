using Comdiv.Rules.Context;

namespace Comdiv.Rules
{
	public interface IExecModule
	{
		void Execute(IRuleContext context);
	}
}