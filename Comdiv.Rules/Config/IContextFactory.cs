using Comdiv.Rules.Context;

namespace Comdiv.Rules.Config
{
	public interface IContextFactory
	{
		IRuleContext CreateEmptyContext();
		void ImportData(IRuleContext context, object someData);
		object ExportData(IRuleContext context, object dataTargetDescriptor);
	}
}