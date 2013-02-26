using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public abstract class TestModule : ITargetedModule<IRule>, ITestModule{
        #region ITargetedModule<IRule> Members

        public IRule Target { get; set; }

        #endregion

        #region ITestModule Members

        public abstract bool Test(IRuleContext context);

        #endregion
    }
}