using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public delegate IList<IRule> GetActivationsDelegate(IList<IRule> collection, IRuleContext context);
}