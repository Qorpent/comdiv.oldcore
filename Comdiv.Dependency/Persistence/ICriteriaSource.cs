#region



#endregion

using NHibernate;

namespace Comdiv.Data
{
	public interface ICriteriaSource
	{
		ICriteria GetCriteria(string prefix);
	}
}