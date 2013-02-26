namespace Comdiv.Rules.Config
{
	public interface IServiceCreator
	{
		object CreateService(IServiceDescriptor descriptor);
	}
}