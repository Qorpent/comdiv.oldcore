using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Engine;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;

namespace Comdiv.Persistence
{
	public static class LinqExtensions {
		private static PropertyInfo SessionProperty = typeof (DefaultQueryProvider).GetProperty("Session",
		                                                                                        BindingFlags.NonPublic |
		                                                                                        BindingFlags.Public |
		                                                                                        BindingFlags.Instance);
		public static string ToSqlString<T>(this IQueryable<T>  query) {
			if(query is NhQueryable<T>) {
				var nhp = query.Provider;
				var sessionImp = SessionProperty.GetValue(nhp) as ISessionImplementor;
				var nhLinqExpression = new NhLinqExpression(query.Expression, sessionImp.Factory);
				var translatorFactory = new ASTQueryTranslatorFactory();
				var translators = translatorFactory.CreateQueryTranslators(nhLinqExpression.Key, nhLinqExpression, null, false, sessionImp.EnabledFilters, sessionImp.Factory);
				return translators[0].SQLString;
			} 
			return query.ToString();//may be it's EF
		}
	}
}
