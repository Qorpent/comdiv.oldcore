using Comdiv.MAS;

namespace Comdiv.Wiki {
	///<summary>
	///</summary>
	public interface IWikiBuilderTaskExecutor {
		void Execute(IWikiRepository repository, WikiBuilderTask task, IConsoleLogHost logger);
	}
}