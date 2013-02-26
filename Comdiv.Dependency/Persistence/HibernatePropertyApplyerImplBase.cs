using System;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model.Interfaces;
using NHibernate.Mapping;
using Enumerable = System.Linq.Enumerable;

namespace Comdiv.Persistence {
    public abstract class HibernatePropertyApplyerImplBase : IHibernatePropertyApplyerImpl {
        private StorageWrapper<object> storage;
        private object item;
        private Type type;
        public int Idx { get; set; }

        
    

        protected abstract void internalApply(object item, string property, string value,string system);
        protected abstract bool internalMatch(object item, string property, string value);

        public void Apply(object item, string property, string value, string system) {
            
                internalApply(item, property, value,system);

                if(item is IWithUsr) {
                    ((IWithUsr) item).Usr = myapp.usrName;
                }
            
        }

        public bool IsMatch(object item, string property, string value) {
            return internalMatch(item,property,value);
        }


    }
}