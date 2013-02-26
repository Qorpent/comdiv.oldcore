using Comdiv.Inversion;

namespace Comdiv.MAS {
    public interface IMasSetupProvider {
        void Execute(IInversionContainer container, MasConfiguration config);
    }
}