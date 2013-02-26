using System.Collections;
using System.Collections.Generic;
using Comdiv.Rules;

namespace Comdiv.Rules.Config
{
	public interface IRuleTranslator
	{
		IList<IRule> TranslateRules(IEnumerable ruleDefenitions);
	}
}