using Comdiv.Application;
using Comdiv.Inversion;
using NHibernate;
using InversionExtensions = Comdiv.Inversion.InversionExtensions;

namespace Comdiv.Persistence
{
    public class TemporarySimpleSession :ICanBeCommited
    {
        public ISession Session;
        private ISession oldtemporary;

        public TemporarySimpleSession(string system= null)
            : this(InversionExtensions.get<ISessionFactoryProvider>(myapp.ioc).Get(system))
        {

        }
        public TemporarySimpleSession(ISessionFactory factory)
        {
            this.oldtemporary = AutomativeCurrentSessionContext.temporary;
            Session = AutomativeCurrentSessionContext.temporary = factory.OpenSession();
            Session.FlushMode = FlushMode.Never;
            
        }


        public bool CanBeCommited { get; set; }
        public void Commit()
        {
            this.CanBeCommited = true;
        }


        public void Dispose()
        {
            if (Session.Transaction.IsActive)
            {
                if (CanBeCommited)
                {
                    Session.Flush();
                }
            }
            Session.Close();
            AutomativeCurrentSessionContext.temporary = oldtemporary;
        }
    }

}