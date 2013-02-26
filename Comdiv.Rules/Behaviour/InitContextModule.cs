using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public abstract class InitContextModule : ITargetedModule<IRule>, IInitContextModule{
        #region IInitContextModule Members

        public abstract void InitContext(IRuleContext context);

        #endregion

        #region ITargetedModule<IRule> Members

        public IRule Target { get; set; }

        #endregion
    }
}