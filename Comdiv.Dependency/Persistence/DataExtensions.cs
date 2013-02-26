using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model.Interfaces;
using NHibernate;
using NHibernate.Criterion;

namespace Comdiv.Persistence{
    public static class DataExtensions
    {

        public static DetachedCriteria setByCondition(this DetachedCriteria criteria, Condition condition,string propname, object propvalue) {
            switch (condition) {
                case Condition.Eq:
                    return criteria.Add(Restrictions.Eq(propname, propvalue));
                case Condition.Neq :
                    return criteria.Add(Restrictions.Not(Restrictions.Eq(propname, propvalue)));
                case Condition.Like:
                    return criteria.Add(Restrictions.Like(propname, propvalue));
                case Condition.Lt:
                    return criteria.Add(Restrictions.Lt(propname, propvalue));
                case Condition.Le:
                    return criteria.Add(Restrictions.Le(propname, propvalue));
                case Condition.Gt:
                    return criteria.Add(Restrictions.Gt(propname, propvalue));
                case Condition.Ge:
                    return criteria.Add(Restrictions.Ge(propname, propvalue));
                default:
                    return criteria;
            }
        }

        private static IInversionContainer _container;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(DataExtensions)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public static ICriteria Eq(this ICriteria criteria,string property, object value){
            return criteria.Add(Restrictions.Eq(property, value));
        }
        public static ICriteria CreateCriteria<T>(this T target)where T:IWithId{
            return Container.getCriteria<T>();
            
        }


        public static bool Exists(this ICriteria criteria){
            criteria.SetProjection(Projections.Id()).SetMaxResults(1).SetFlushMode(FlushMode.Never);
            return null != criteria.UniqueResult();
        }
        public static IDataParameter AddParameter(this IDbCommand command, string name, object value){
            var result = command.CreateParameter();
            result.ParameterName = name;
            result.Value = value;
            command.Parameters.Add(result);
            return result;
        }

       
        public static bool Existed(this IWithId obj){
            if (null == obj) return false;
            return 0 != obj.Id;
        }


        public static IDbConnection GetDefaultConnection(){
            //HACK : работает только с SqlConnection
            return new SqlConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
        }


        // TODO: BAD PRACTICE перекрывает уже имеющийся метод.

        public static ICriteria Prepare(this ICriteria criteria, Order[] orders, ICriterion[] criterias,
                                        IDictionary<string, string> aliases, IProjection projection){
            if (null == criteria) return null;
            if (orders.yes()){
                foreach (var order in orders)
                    criteria.AddOrder(order);
            }
            if (criterias.yes()){
                foreach (var c in criterias)
                    criteria.Add(c);
            }
            if (aliases.yes()){
                foreach (var pair in aliases)
                    criteria.CreateAlias(pair.Value, pair.Key);
            }
            if (projection != null)
                criteria.SetProjection(projection);
            return criteria;
        }

        public static ICriteria SetRange(this ICriteria criteria, int? firstResult, int? maxResults)
        {
            if (null == criteria) return null;
            if (firstResult.HasValue) criteria.SetFirstResult(firstResult.Value);
            if (maxResults.HasValue) criteria.SetMaxResults(maxResults.Value);
            return criteria;
        }
    }
}