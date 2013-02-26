namespace Comdiv.Rules.Config
{
	public interface IExpertConfig
	{
		IExpertFactory ExpertFactory { get; }
		IContextFactory ContextFactory { get; }
	}
}