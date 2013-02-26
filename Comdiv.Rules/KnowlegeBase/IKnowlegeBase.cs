

namespace Comdiv.Rules.KnowlegeBase{
    public interface IKnowlegeBase : IRule{
        IRuleCollection Rules { get; }
        // IRuleResolver Resolver { get; set; }
        //   void Apply(IList<IRule> currentActivations, IRuleContext context);
        //  IList<IRule> Filter(IRuleContext context);
    }
}