using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using Comdiv.Inversion;
using NHibernate;

namespace Comdiv.Persistence
{
    public static class HibernateInversionExtensions
    {
        public static ISession getSession(this IInversionContainer container){
            return getSession(container, "");
        }

        public static ISession getSession(this IInversionContainer container, string id){
            return container.get<ISessionFactoryProvider>().Get(id).GetCurrentSession();
        }

        public static ISession newSession(this IInversionContainer container)
        {
            return getSession(container, "");
        }

        public static ISession newSession(this IInversionContainer container, string id)
        {
            return container.get<ISessionFactoryProvider>().Get(id).OpenSession();
        }

        public static IDbConnection getConnection(this IInversionContainer container)
        {
            return getConnection(container, "");
        }

        public static IDbConnection getConnection(this IInversionContainer container, string id)
        {
            return container.get<IConnectionsSource>().Get(id).CreateConnection();
        }

        public static string getConnectionString(this IInversionContainer container)
        {
            return getConnectionString(container, "");
        }

        public static string getConnectionString(this IInversionContainer container, string id)
        {
            return container.get<ISessionFactoryProvider>().getConnection(id).ConnectionString;
        }

        public static ICriteria getCriteria<T>(this IInversionContainer container)
        {
            return getCriteria<T>(container, "","this");
        }

        public static ICriteria getCriteria<T>(this IInversionContainer container, string alias){
            return getCriteria<T>(container, "", alias);
        }

        public static ICriteria getCriteria<T>(this IInversionContainer container, string id, string alias)
        {
            return container.get<ISessionFactoryProvider>().getCriteria<T>(id,"this");
        }

        
    }
}
