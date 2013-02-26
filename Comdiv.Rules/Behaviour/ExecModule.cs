using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public abstract class ExecModule : ITargetedModule<IRule>, IExecModule{
        #region IExecModule Members

        public abstract void Execute(IRuleContext context);

        #endregion

        #region ITargetedModule<IRule> Members

        public IRule Target { get; set; }

        #endregion
    }
}