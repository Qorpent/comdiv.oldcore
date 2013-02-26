#region

using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using NHibernate;
using NHibernate.Criterion;

#endregion

namespace Comdiv.Data
{
    public interface IIdCodeNameCriteria {
        string Code { get; set; }
        string CodePattern { get; set; }
        int Id { get; set; }
        string NamePattern { get; set; }
        IList<string> Codes { get; set; }
        IList<int> Ids { get; set; }
        bool IsOnlyIdsQuery();
        bool IsEmpty();
    }

    public class IdCodeNameCriteria<T> :  IIdCodeNameCriteria{
		private object chainObject;
		public string Code { get; set;}
		public JoinFunction CodeNameJoinFunction = JoinFunction.Or;
		public string CodePattern { get; set;}
	    private IList<string> codes = new List<string>();
		public int Id { get; set;}
	    private IList<int> ids = new List<int>();
		public bool IsRoot;
		public string NamePattern { get; set;}

		public IdCodeNameCriteria(){
			ChainObject = this;
		}

		public object ChainObject{
			get { return chainObject; }
			set{
				chainObject = value;
				afterSetChainObject(value);
			}
		}

	    public IList<string> Codes{
	        get { return codes; }
	        set { codes = value; }
	    }

	    public IList<int> Ids{
	        get { return ids; }
	        set { ids = value; }

        }

        protected bool? isOnlyIdsQuery;
        protected bool? isEmpty;
        public  bool IsOnlyIdsQuery(){
            if(isOnlyIdsQuery.HasValue) return isOnlyIdsQuery.Value;

            isOnlyIdsQuery = internalIsOnlyIds();
            return isOnlyIdsQuery.Value;
        }

        protected virtual bool internalIsOnlyIds() {
            if(Code.hasContent()) return false;
            if(NamePattern.hasContent()) return false;           
            if (0 != Codes.Count) return false;
            return true;
        }

        public bool IsEmpty(){
            if(isEmpty.HasValue) return isEmpty.Value;
            isEmpty = internalIsEmpty();
            return isEmpty.Value;
        }

        protected virtual bool internalIsEmpty() {
            if(!IsOnlyIdsQuery()) return false;
            if (0!=Id) return false;
            if (0!=Ids.Count) return false;
            return true;
        }

        #region IAliasSetup Members

		public virtual void SetupAliases(DetachedCriteria criteria, string prefix, string selfName){
			if (IsRoot) return;
			if (GetCriterion(null) != null){
				var self = prefix.NormalAlias(selfName);
				if (prefix != string.Empty && !prefix.EndsWith(".")) prefix += ".";
				criteria.CreateAlias(prefix + selfName, self);
			}
		}

		#endregion

		#region ICriterionSource Members

		public virtual ICriterion GetCriterion(string prefix){
			prefix = Utils.NormalAlias(prefix);
			CleanElements();
			if (0 != Id) return Restrictions.Eq(prefix + "Id", Id);
			if (null != Ids && 1 == Ids.Count) return Restrictions.Eq(prefix + "Id", Ids[0]);
			if (null != Ids && 1 < Ids.Count) return Restrictions.InG(prefix + "Id", Ids);
			if (Code.hasContent()) return Restrictions.Eq(prefix + "Code", Code);
			if (null != Codes && 1 == Codes.Count) return Restrictions.Eq(prefix + "Code", Codes[0]);
			if (null != Codes && 1 < Codes.Count) return Restrictions.InG(prefix + "Code", Codes);
			ICriterion codes = null;
			ICriterion names = null;
			if (CodePattern.hasContent())
				codes = Restrictions.Like(prefix + "Code", CodePattern);
			if (NamePattern.hasContent())
				names = Restrictions.Like(prefix + "Name", NamePattern);
			if (null == codes && null == names) return null;
			if (null == names) return codes;
			if (null == codes) return names;
			if (JoinFunction.Or == CodeNameJoinFunction)
				return Restrictions.Or(codes, names);
			else
				return Restrictions.And(codes, names);
		}

		#endregion

		public T SetId(params int[] ids){
			Id = 0;
			Ids = ids;
			return (T) ChainObject;
		}

		public T AddId(int id){
			if (0 != id){
				if (0 != Id)
					Ids.Add(Id);
				Ids.Add(id);
				Id = 0;
			}
			return (T) ChainObject;
		}

		public T AddCode(string code){
			if (code.hasContent()){
                
				Codes.Add(code);
				Code = null;
			}
			return (T) ChainObject;
		}

		public T SetCodePattern(string codePattern){
			CodePattern = codePattern;
			if (codePattern.hasContent())
				ClearDirectConditions();
			return (T) ChainObject;
		}

		private void ClearDirectConditions(){
			Id = 0;
			Ids = new List<int>();
			Code = null;
			Codes = new List<string>();
		}

		public T SetNamePattern(string namePattern){
			NamePattern = namePattern;
			if (namePattern.hasContent())
				ClearDirectConditions();
			return (T) ChainObject;
		}

		public T SetCode(params string[] codes){
			Code = null;
			Codes = codes;
			return (T) ChainObject;
		}

		private void CleanElements(){
			var newids = new List<int>();
			foreach (var i in Ids)
				if (0 != i && !newids.Contains(i)) newids.Add(i);
			Ids = newids;
			var newcodes = new List<string>();
			foreach (var s in Codes)
				if (s.hasContent() && !newcodes.Contains(s)) newcodes.Add(s);
			Codes = newcodes;
		}

		protected virtual void afterSetChainObject(object chainObject) {}
	}

	public class IdCodeNameCriteria<T, R> : IdCodeNameCriteria<T>
	{
		#region ICriteriaProvider Members


        public IList<R> List(){
            return myapp.storage.Get<R>().Query(GetCriteria()).ToList();
        }

	    public DetachedCriteria GetCriteria(){
	        return GetCriteria("this");
	    }

	    private IInversionContainer _container;

	    public IInversionContainer Container{
	        get{
	            if (_container.invalid()){
	                lock (this){
	                    if (_container.invalid()){
	                        Container = myapp.ioc;
	                    }
	                }
	            }
	            return _container;
	        }
	        set { _container = value; }
	    }

	    public DetachedCriteria GetCriteria(string prefix){
		    var root = IsRoot;
            try{
                IsRoot = true;
                var crit = GetCriterion(prefix);

                var result = DetachedCriteria.For<R>();
             
                SetupAliases(result, string.Empty, prefix);
                if (null != crit)
                    result.Add(crit);
                return result;
            }finally{
                IsRoot = root;
            }
		}

		#endregion

		#region IQueryExecutor<R> Members

		public R[] Execute(params Order[] orders){
		    var crit = GetCriteria("this");
		    foreach (var order in orders){
		        crit.AddOrder(order);
		    }
			return myapp.storage.Get<R>().Query(crit).ToArray();
		}

		#endregion

		public T Add(R item){
			if (null != item)
				Ids.Add(((IWithId) item).Id);
			return (T) ChainObject;
		}

		public T Eq(R item){
			if (null != item)
				return SetId(((IWithId) item).Id);
			return (T) ChainObject;
		}
	}
}