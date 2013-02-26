using Comdiv.Application;
using Comdiv.Inversion;
using NHibernate;
using InversionExtensions = Comdiv.Inversion.InversionExtensions;

namespace Comdiv.Persistence
{
    public class TemporaryReadonlySession
    {
        public ISession Session;
        private ISession oldtemporary;

        public TemporaryReadonlySession()
            : this(InversionExtensions.get<ISessionFactoryProvider>((IInversionContainer) myapp.ioc).Get(null))
        {

        }
        public TemporaryReadonlySession(ISessionFactory factory)
        {
            this.oldtemporary = AutomativeCurrentSessionContext.temporary;
            Session = AutomativeCurrentSessionContext.temporary = factory.OpenSession();
            Session.FlushMode = FlushMode.Never;
            Session.BeginTransaction();
        }

       

        public void Dispose()
        {
           
            Session.Transaction.Rollback();
            Session.Clear();
            Session.Close();
            AutomativeCurrentSessionContext.temporary = oldtemporary;
        }
    }
}