#region



#endregion

using NHibernate;

namespace Comdiv.Data
{
	public interface IAliasSetup
	{
		void SetupAliases(ICriteria criteria, string prefix, string selfName);
	}
}