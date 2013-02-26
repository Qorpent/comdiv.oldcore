using Comdiv.Rules.Context;

namespace Comdiv.Rules.Config
{
	public interface IContextDataImporter
	{
		void ImportData(IRuleContext context, object data);
	}
}