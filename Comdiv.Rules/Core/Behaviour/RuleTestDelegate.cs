using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public delegate bool RuleTestDelegate(IRule rule, IRuleContext context);
}