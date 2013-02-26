using System;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using NHibernate.Criterion;

namespace Comdiv.Model{
    public class DefaultMaxVersionProvider:IMaxVersionProvider{
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
        public DateTime GetMaxVersion<T>()
        {
            return new DateTime(1980,1,1);
            lock (this){
                var result_ = new DateTime(1900, 1, 1);
                if (typeof (IWithVersion).IsAssignableFrom(typeof (T))){
                    var crit = Container.getCriteria<T>();
                    if (null == crit) return result_;
                    var res = crit
                        .SetProjection(Projections.Max("Version"))
                        .UniqueResult();
                    DateTime result = new DateTime(1900, 1, 1);
                    if (res != null){
                        result = (DateTime) res;
                    }
                    if (result > result_){
                        result_ = result;
                    }
                }
                return result_;
            }
        }
    }
}