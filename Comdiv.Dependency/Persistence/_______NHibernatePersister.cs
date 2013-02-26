using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Comdiv.SessionEasier;
using log4net;
using NHibernate;
using NHibernate.Criterion;

namespace Comdiv.Model {
    public class NHibernatePersister : PersisterBase, INewPersisentObjectCreator, IPersisterInitiator, IPersisterTypeResolver{
        
        private readonly IDictionary<string, Type> resolveCache = new Dictionary<string, Type>();

        private bool usePreprocessors = false;

        private ILog log = logger.getLogger("NHibernatePersister");

        protected IList<IHqlPreprocessor> preprocessors = new List<IHqlPreprocessor>();

        public NHibernatePersister() {

            Priority = 100;
            RealStorage = true;
            IsForDefaultPersisterChain = true;
            InitializeDefaultInterceptors();
        }

        protected void InitializeDefaultInterceptors(){
            interceptors
                .fillFromLocator()
                //.ensureInstance(()=>new UidSetInterceptor()) 
                .ensureInstance(()=> new CodeGenerator { Format = "~{sname}" })
                .ensureInstance(()=>new UsrAwareGenerator())
                ;
            preprocessors
                .fillFromLocator();
            usePreprocessors = preprocessors.Count != 0;
        }

        protected IDictionary<string, Type> ResolveCache {
            get { return resolveCache; }
        }

       

        #region INewPersisentObjectCreator Members

        public T CreateNew<T>() {
            return (T)CreateNew(typeof(T));
        }

        public object CreateNew(Type type) {
            return CreateNew(null, type);
        }

        #endregion



        #region IPersisterInitiator Members

        public bool Init() {
            return true;
        }

        #endregion

        #region IPersisterTypeResolver Members

        public Type Resolve(Type type) {
            return Resolve(null, type);
        }

        #endregion

        public object CreateNew(string alias, Type type) {
            var realType = Resolve(alias, type);
            var result = realType.create();
            InterceptNew(result);
            return result;
        }
        object sync = new object();
        public Type Resolve(string alias, Type type) {
            lock (sync)
            {
                var key = (alias ?? "") + "_" + type.FullName;
                if (!ResolveCache.ContainsKey(key))
                {
                    var registeredClasses = this._<ISessionFactoryProvider>().GetFactory(alias).GetAllClassMetadata();
                    Type targetType = null;
                    foreach (var pair in registeredClasses)
                    {
                        var testType = pair.Value.GetMappedClass(EntityMode.Poco);
                        if (testType.Equals(type) || type.IsAssignableFrom(testType) || testType.IsAssignableFrom(type))
                        {
                            targetType = testType;
                            break;
                        }
                    }
                    ResolveCache[key] = targetType;
                }
                return ResolveCache[key];
            }
        }

        [ThreadStatic]
        public static ISession FOR_HACKS_ONLY_SESSION;

        private ISession GetSession(string alias){ 
            if (null != FOR_HACKS_ONLY_SESSION) return FOR_HACKS_ONLY_SESSION;
            var result = (this._<ISessionProvider>() ?? new DefaultSessionProvider()).GetSession(alias);
            if(!result.IsConnected){
                result.Reconnect();
            }
            if(result.Connection.State!=ConnectionState.Open){
                result.Connection.Open();
            }
            return result;
        }

        protected override void InternalDelete(IPersisterContext context) {
            Delete(context.Alias, context.Target, context.Hint.Flush);
        }
        public virtual void Delete(string alias, object target, bool flush)
        {
            using (var s = FlushedContext.Create(flush, alias, this))
            {
                s.Session.Delete(target);
            }
        }


        public override void Exists(IPersisterContext context) {
            var session = GetSession(context.Alias);
            var type = Resolve(context.Alias, context.TargetType);
            var exists = session.CreateCriteria(type, "this");
            if (context.PrimaryKey is string && typeof(IWithCode).IsAssignableFrom(type)) {
                exists.Add(Restrictions.Eq("Code", context.PrimaryKey));
            } else if (context.PrimaryKey is Guid && typeof(IWithUid).IsAssignableFrom(type)) {
                exists.Add(Restrictions.Eq("Uid", context.PrimaryKey));
            } else {
                exists.Add(Restrictions.Eq(Projections.Id(), context.PrimaryKey));
            }

            exists.SetProjection(Projections.Count(Projections.Id()));
            var existedId = exists.UniqueResult<object>();
            context.Finish(0 != existedId.toInt());
        }

        public override bool IsApplyableTo(Type type) {
            return IsApplyableTo(null, type);
        }

        public virtual bool IsApplyableTo(string alias, Type type) {
            if (type.Equals(typeof(NHiberanatedType))) return true;
            return null != Resolve(alias, type);
        }

        protected override void InternalLoad(IPersisterContext context) {
            if(context.Finished) return;
            var type = ResolveType(context);
            var session = GetSession(context.Alias);
            object result = null;
            if (context.PrimaryKey is string && typeof(IWithCode).IsAssignableFrom(type)){
                result =
                    session.CreateCriteria(type, "this").SetMaxResults(1).Add(Restrictions.Eq("Code",
                                                                                              context.PrimaryKey)).
                        UniqueResult();
               
            }
            else if (context.PrimaryKey is string && typeof(IWithName).IsAssignableFrom(type))
            {

                result =
                   session.CreateCriteria(type, "this").SetMaxResults(1).Add(Restrictions.Eq("Name",
                                                                                             context.PrimaryKey)).
                       UniqueResult();
            }
            else if (context.PrimaryKey is Guid && typeof(IWithUid).IsAssignableFrom(type))
            {
                result =
                    session.CreateCriteria(type, "this").SetMaxResults(1).Add(Restrictions.Eq("Uid",
                                                                                              context.PrimaryKey)).
                        UniqueResult();
            }
            else
            {
                result = session.Get(type, context.PrimaryKey);
            }

            context.Finish(result);
        }

        protected override void InternalQuery(IPersisterContext context) {
            if (context.Finished) return;
            var type = ResolveType(context);
            var query = context.BaseQuery;
            var advancedParameters = context.AdvancedQuery;
            var hint = context.Hint;
            var alias = context.Alias;

            if (context.BaseQuery is ICriteria){
                context.FinishQuery(
                    getByCriteria(alias, (ICriteria)query, type,
                                  advancedParameters.Cast<Order>().ToArray(),
                                  hint)
                    );
                return;
            }

            if (null == query){
                context.FinishQuery(
                    getOnNullCondition(alias, type, context.Hint)
                    );
                return;
            }
            if (query is string && advancedParameters.yes() && advancedParameters.Count == 1){
                context.FinishQuery(
                    getByProperty(alias, query as string, type, advancedParameters[0], hint)
                    );
                return;
            }
            if (query is string){
                context.FinishQuery(
                    getByHql(alias, type, (string)query, context.Hint, advancedParameters)
                    );
                return;
            }
            if (query is Order[]){
                context.FinishQuery(
                    getByOrdersAndCriterions(alias, (Order[])query, type,
                                             advancedParameters.Cast<ICriterion>().ToArray(),
                                             hint)
                    );
                return;
            }
            if (query is Order){
                context.FinishQuery(
                    getByOrdersAndCriterions(alias, new[] { (Order)query }, type,
                                             advancedParameters.Cast<ICriterion>().ToArray(), hint)
                    );
                return;
            }
            if (query is ICriterion) {
                var criterions = advancedParameters.OfType<ICriterion>().ToList();
                var orders = advancedParameters.OfType<Order>().ToArray();

                criterions.Insert(0, query as ICriterion);

                context.FinishQuery(
                    getByOrdersAndCriterions(alias, orders, type, criterions.ToArray(), hint)
                    );
                return;
            }
            if (query is DetachedCriteria){
                context.FinishQuery(
                    getByDetachedCriteria(alias, (DetachedCriteria)query, type,
                                          advancedParameters.Cast<Order>().ToArray(),
                                          hint)
                    );
                return;
            }

            context.Throw(new NHibernateException(query, advancedParameters));
        }

        public override void Refresh(IPersisterContext context) {
            if (context.Finished) {
                return;
            }
            GetSession(context.Alias).Refresh(context.Target);
            context.Finished = true;
        }

        protected override void InternalSave(IPersisterContext context) {
            Save(context.Alias, context.Target, context.Hint.Flush, context.Hint.Copy);
        }

        protected override Type ResolveType(IPersisterContext context) {
            return Resolve(context.Alias, context.TargetType);
        }

        protected virtual void Save(string alias, object target, bool flush, bool copy) {
            // WARN: в object может прилететь все что угодно 
            
            using (var s = FlushedContext.Create(flush, alias, this)) {
                if (copy) {
                    s.Session.SaveOrUpdateCopy(target);
                } else {
                    s.Session.SaveOrUpdate(target);
                }
            }
        }



        private IEnumerable getByHql(string alias, Type type, string query, Hint hint, IList<object> advancedParameters) {

            if(log.IsDebugEnabled){
                log.Debug("query: "+query+"; params :"+advancedParameters.concat(";"));
            }

            if(usePreprocessors){
                preprocessors.map(x => query = x.Rewrite(query));
                if(log.IsDebugEnabled){
                    log.Debug("reqrited query: "+query);
                }
            }

            var session = GetSession(alias);

            var q = "hql" == hint.QueryLanguage.ToLower() ? session.CreateQuery(query) : session.CreateSQLQuery(query);
            if(hint.MaxResult!=0){
                q.SetMaxResults(hint.MaxResult);
            }

            int pos = 0;
            foreach (var o in advancedParameters) {
                q.SetParameter(pos, o);
                pos++;
            }

            var result = q.Enumerable();

            return result;
        }

        private IEnumerable getByDetachedCriteria(string alias, DetachedCriteria criteria, Type realType, Order[] orders,
                                                  Hint hint) {
            if (hint.MaxResult == 1) {
                criteria.SetMaxResults(1);
            }
            foreach (var order in orders) {
                criteria.AddOrder(order);
            }

            var session = GetSession(alias);

            var result = criteria.GetExecutableCriteria(session).List();

            return result;
        }

        private IEnumerable getByCriteria(string alias, ICriteria criteria, Type realType, Order[] orders,
                                                  Hint hint) {
            if (hint.MaxResult == 1) {
                criteria.SetMaxResults(1);
            }
            foreach (var order in orders) {
                criteria.AddOrder(order);
            }

            var result = criteria.List();

            return result;
        }

        private IEnumerable getByOrdersAndCriterions(string alias, Order[] orders, Type realType,
                                                     ICriterion[] criterions, Hint hint) {
            var session = GetSession(alias);
            var crit = session.CreateCriteria(realType, "this");
            if (hint.MaxResult != 0) {
                crit.SetMaxResults(hint.MaxResult);
            }
            foreach (var criterion in (criterions ?? new ICriterion[] { })) {
                crit.Add(criterion);
            }
            foreach (var order in (orders ?? new Order[] { })) {
                crit.AddOrder(order);
            }
            IEnumerable result = null;
            try{
                result = crit.List();
            }catch(NHibernate.ADOException ex){
                if (ex.ToString().Contains("aborted"))
                {
                    session.Transaction.Rollback();
                    session.BeginTransaction();
                    result = crit.List();
                }
                else
                {
                    throw;
                }
            }

            return result;
        }

        private IEnumerable getByProperty(string alias, string property, Type realType, object value, Hint hint) {
            return getByOrdersAndCriterions(alias, null, realType, new[] { Restrictions.Eq(property, value) }, hint);
        }

        private IEnumerable getOnNullCondition(string alias, Type realType, Hint hint) {
            return getByOrdersAndCriterions(alias, null, realType, null, hint);
        }

        #region Nested type: FlushedContext

        internal class FlushedContext : IDisposable {
            private readonly bool Flush;

            public FlushedContext(bool flush, ISession session) {
                Flush = flush;
                Session = session;
            }

            public ISession Session { get; set; }

            #region IDisposable Members

            public void Dispose() {
                if (Flush) {
                    Session.Flush();
                }
            }

            #endregion

            public static FlushedContext Create(bool flush, string alias, NHibernatePersister persister) {

                return new FlushedContext(flush, persister.GetSession(alias));

            }
        }

        #endregion

       
        
    }
}