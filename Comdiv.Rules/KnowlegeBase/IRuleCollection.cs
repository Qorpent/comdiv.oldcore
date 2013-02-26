using System.Collections.Generic;


namespace Comdiv.Rules.KnowlegeBase{
    public interface IRuleCollection : IList<IRule>{
        int Version { get; }
        IRule ContainingRule { get; }
    }
}