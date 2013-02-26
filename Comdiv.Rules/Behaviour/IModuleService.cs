namespace Comdiv.Rules.Activation
{
	public interface IModuleService
	{
		int Version { get; }
		void ChangeActivation(string moduleName, bool activate);
		bool IsActive(string moduleName);

		string[] GetActiveModules();
	}
}