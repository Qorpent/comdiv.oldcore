using System.Collections;
using System.Collections.Generic;
using Comdiv.Rules;

namespace Comdiv.Rules.Config
{
	public interface IExpertFactory
	{
		IRule CreatePredefinedExpert();
		IRule GetOptimizedVersion(IRule rule);
		IList<IRule> TranslateToRules(IEnumerable ruleSources);
	}
}