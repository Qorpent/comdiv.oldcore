using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public interface IRule : IWithParameters<IRule>, IWithParent<IRule>{
        bool Test(IRuleContext context);
        void Execute(IRuleContext context);
    }
}