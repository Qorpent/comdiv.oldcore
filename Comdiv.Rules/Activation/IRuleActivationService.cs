using System.Collections.Generic;

using Comdiv.Rules.KnowlegeBase;

namespace Comdiv.Rules.Activation{
    public interface IRuleActivationService : IList<IRuleActivationChecker>, IRuleFilter {}
}