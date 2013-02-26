using Comdiv.Application;
using Comdiv.Inversion;
using NHibernate;
using InversionExtensions = Comdiv.Inversion.InversionExtensions;

namespace Comdiv.Persistence
{
    public class TemporaryTransactionSession: ICanBeCommited{
        public ISession Session;
        private ISession oldtemporary;

        public TemporaryTransactionSession():this(myapp.ioc.get<ISessionFactoryProvider>().Get(null)){
            
        }
        public TemporaryTransactionSession(ISessionFactory factory){
            this.oldtemporary = AutomativeCurrentSessionContext.temporary;
            Session = AutomativeCurrentSessionContext.temporary = factory.OpenSession();
            Session.FlushMode = FlushMode.Never;
            Session.BeginTransaction();
        }

        public bool CanBeCommited { get; set; }
        public void Commit(){
            this.CanBeCommited = true;
        }

        public void Dispose(){
            if(Session.Transaction.IsActive){
                if (CanBeCommited)
                {
                    Session.Transaction.Commit();
                }
                else{
                    Session.Transaction.Rollback();
                }
            }
            AutomativeCurrentSessionContext.temporary = oldtemporary;
        }
    }
}