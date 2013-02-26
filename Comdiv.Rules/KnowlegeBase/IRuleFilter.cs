using System.Collections.Generic;

using Comdiv.Rules.Context;

namespace Comdiv.Rules.KnowlegeBase{
    public interface IRuleFilter{
        IList<IRule> Filter(IRuleContext context, IList<IRule> ruleGroup);
    }
}