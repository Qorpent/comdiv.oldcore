using System;
using System.Collections;
using System.Data.SqlClient;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;

using Comdiv.IO;
using Comdiv.Security;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Transform;

namespace Comdiv.Persistence{
    public class HibernateStorageImplementator{
        public HibernateStorageImplementator(HibernateStorage parentStorage, StorageQuery query){
            this.storage = parentStorage;
            this.factoryProvider = storage.FactoryProvider;
            lock (factoryProvider){
                this.factory = factoryProvider.Get(query.System);
                this.session = factory.GetCurrentSession();
                //in cases of temporary database changes
                if(((SessionImpl)this.session).Factory!=this.factory){
                    this.session = factory.OpenSession();
                }
                session.FlushMode = FlushMode.Never;
                
            }
            this.query = query;
        }

        public void Process(){
            try {
                switch (query.QueryType) {
                    case StorageQueryType.Delete:
                        doDelete();

                        break;
                    case StorageQueryType.Exists:
                        query.Result = getExists();
                        break;
                    case StorageQueryType.Load:
                        query.Result = getLoad();
                        break;
                    case StorageQueryType.Query:
                        var result = getQuery();
                        query.Result = result;
                        break;
                    case StorageQueryType.Save:
                        doSave();
                        break;

                    case StorageQueryType.Refresh:
                        session.Refresh(query.Target);
                        break;
                }
                query.CommandProcessed = true;
                if (query.IsCacheAble) {
                    lock (storage.Cache) {
                        storage.Cache.Store(query);
                    }
                }
            }catch(Exception ex) {
                if (ex.ToString().Contains("Could not open a connection to SQL Server"))
                {
                    throw new Exception("erorr sql connection: "+myapp.ioc.get<IConnectionsSource>().Get(query.System).ConnectionString);
                }else {
                    throw;
                }
            }
        }

     
        public ILog log = logger.get("comdiv.persistence.hibernate");


        private ISession session;

        private string _entname;
        private string entname{
            get{
                if(null==_entname){
                    _entname = factory.GetClassMetadata(query.RealType).EntityName;
                }
                return _entname;
            }
            
        }

        protected void doDelete(){
            lock (session){


                if (null != query.Target){
                    if (query.Target is IEnumerable){
                        foreach (var obj in (IEnumerable) query.Target){
                            session.Delete(obj);
                        }
                    }
                    else{
                        session.Delete(query.Target);
                    }
                }
                else{
                    var q = "";
                    object p = 0;
                    if (0 != query.PrimaryKey){
                        q = "from " + entname + " where Id = ?";
                        p = query.PrimaryKey;
                    }
                    else{
                        if (string.IsNullOrWhiteSpace(query.BizCode)){
                            q = query.BizCode;
                            if (!q.Contains(" ")){
                                q = "from " + entname + " where Code = ?";
                                p = query.BizCode;
                            }
                            else{
                                q = "from " + entname + " as this where " + query.BizCode;
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(q)){
                        session.Delete(q, p, NHibernateUtil.GuessType(p));
                    }
                    else{
                        throw new StorageException("no valid data to perform delete", query);
                    }
                }

                session.Flush();
            }
        }

        protected  void doSave(){
            lock (session){
                if (query.Target is IWithCode && string.IsNullOrWhiteSpace(query.Target.Code())){
                    CheckCode(query.Target);
                }
                session.SaveOrUpdate(query.Target);
                session.Flush();
            }
        }

        private void CheckCode(object target)
        {

            var index = 1;
            var basecode = target.Name().toShort();
            if (string.IsNullOrWhiteSpace(basecode))
            {
                basecode = "NONAME";
            }
            var code = basecode;

            Func<string, string> tohql = c => string.Format("select Id from {0} where Code = '{1}'" ,entname,c );

            while (session.CreateQuery(tohql(code)).List().Count!=0){
                code = basecode + "_" + index++;
            }

            target.setCode(code);

        }

        protected  object getExists(){
            lock (session){
                if (query.PrimaryKey != 0){
                    return session
                               .CreateCriteria(query.RealType)
                               .Add(Restrictions.Eq("Id", query.PrimaryKey))
                               .SetProjection(Projections.Property("Id"))
                               .UniqueResult() != null;
                }
                return session
                           .CreateCriteria(query.RealType)
                           .Add(Restrictions.Eq("Code", query.BizCode))
                           .SetProjection(Projections.Property("Id"))
                           .UniqueResult() != null;
            }
        }


        protected  object getLoad(){
            lock (session){
                if (query.PrimaryKey != 0 || query.Key==null){
                    try{
                        var res = session.Get(query.RealType, query.PrimaryKey);
                        if(null!=res) return res;
                       
                    }
                    catch (OverflowException ex){}
                }
                //if (query.Key.Equals(0)){
                //    return null;
                //}
                
                return
                    session.CreateCriteria(query.RealType, "this").Add(Restrictions.Eq("Code", query.Key.ToString())).UniqueResult();
            }
        }

        protected  IEnumerable getQuery(){
            lock (session){
                if (!string.IsNullOrWhiteSpace(query.QueryText) && query.Dialect == QueryDialect.Hql){
                    return getByHql();
                }
                if (query.CommonQueryObjects.yes() &&
                    (query.CommonQueryObjects.FirstOrDefault() is ICriteria)){
                    return ((ICriteria) query.CommonQueryObjects.FirstOrDefault()).List();
                }
                if (query.CommonQueryObjects.yes() &&
                    (query.CommonQueryObjects.FirstOrDefault() is DetachedCriteria)){
                    return
                        ((DetachedCriteria) query.CommonQueryObjects.FirstOrDefault()).GetExecutableCriteria(session).SetFlushMode(FlushMode.Never).
                            List();
                }

                if (query.CommonQueryObjects != null){
                    return getByCriteria();
                }
                var crit = session.CreateCriteria(query.RealType);
                commonPrepare(crit);
                return crit.List().Cast<object>().ToArray();
            }
        }

        

        private IEnumerable getByCriteria(){
            var crit = session.CreateCriteria(query.RealType);
            commonPrepare(crit);
            var projections = query.CommonQueryObjects.OfType<IProjection>().ToList();
            var criterions = query.CommonQueryObjects.OfType<ICriterion>().ToList();
            var orders = query.CommonQueryObjects.OfType<Order>().ToList();
            var transformer = query.CommonQueryObjects.OfType<IResultTransformer>().FirstOrDefault();

            if (projections.Count != 0){
                var projlist = Projections.ProjectionList();
                foreach (var projection in projections){
                    projlist.Add(projection);
                }
                crit.SetProjection(projlist);
            }

            foreach (var criterion in criterions){
                crit.Add(criterion);
            }

            foreach (var order in orders){
                crit.AddOrder(order);
            }

            if (transformer != null){
                crit.SetResultTransformer(transformer);
            }
            
            return crit.List();
        }

        private ICriteria commonPrepare(ICriteria crit){
            if (query.MaxCount != 0){
                crit.SetMaxResults(query.MaxCount);
            }
            if (query.StartIndex != 0){
                crit.SetFirstResult(query.StartIndex);
            }
            return crit;
        }

        

        private StorageQuery query;
        private HibernateStorage storage;
        private ISessionFactoryProvider factoryProvider;
        private ISessionFactory factory;

        private IEnumerable getByHql() {
        	int top = 0;
               var hql = query.QueryText;
                if (hql.Contains("ENTITY")){
                    hql = hql.Replace("ENTITY", entname);
                }
                if (!hql.Contains("from ")){
                    if (hql.like(@"^\w+$")){
                        hql = hql + " = ?";
                    }
                    hql = "from " + entname + " where " + hql;
                }
				if(hql.like(@"(\b|^)top\s+\d+\s+")) {
					top = hql.find(@"(\b|^)top\s+(\d+)\s+", 2).toInt();
					hql = hql.replace(@"(\b|^)top\s+\d+", "");
				}
                var q = session.CreateQuery(hql);

                commonPrepare(q);
			if(top!=0) {
				q.SetMaxResults(top);
			}
                if (query.QueryTextPositionalParameters != null){
                    int x = 0;
                    foreach (var obj in query.QueryTextPositionalParameters){
                        q.SetParameter(x, obj);
                        x++;
                    }
                }
            
            return q.Enumerable();
        }

        private void commonPrepare(IQuery q){
            
            if (query.MaxCount != 0){
                q.SetMaxResults(query.MaxCount);
            }
            if (query.StartIndex != 0){
                q.SetFirstResult(query.StartIndex);
            }
        }
    }
}