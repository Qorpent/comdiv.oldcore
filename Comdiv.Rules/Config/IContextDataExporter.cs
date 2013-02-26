using Comdiv.Rules.Context;

namespace Comdiv.Rules.Config
{
	public interface IContextDataExporter
	{
		object ExportData(IRuleContext context, object query);
	}
}