
using NHibernate.Criterion;

namespace Comdiv.Data
{
	public interface ICriterionSource
	{
		ICriterion GetCriterion(string prefix);
	}
}