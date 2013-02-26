using Comdiv.Audit;

namespace Comdiv.MAS
{
    ///<summary>
    ///</summary>
    public interface IMasAuditTestProvider : IAuditTestProvider
    {
        void Contextualize(TestProvider provider, IConsoleLogHost logger);
    }
}