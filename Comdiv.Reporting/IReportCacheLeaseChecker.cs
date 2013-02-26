using System;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Persistence;

namespace Comdiv.Reporting{
    public interface IReportCacheLeaseChecker{
        bool IsValid(IReportDefinition definition, DateTime forTime);
    }

    public class DataBasedReportCacheLeaseProvider : IReportCacheLeaseChecker {
        public bool IsValid(IReportDefinition definition, DateTime forTime){
            lock (this){
                if (!definition.Parameters.ContainsKey("datalease")) return false;
                var roottype = definition.Parameters.get<string>("dataleaseroot");
                if (roottype.noContent()){
                    throw new Exception("cannot use datalease, while root type not seted");
                }
                var type = roottype.toType();
                var checker = definition.Parameters["datalease"].toStr();
                if (checker.noContent() || checker == "nocheck") return true;
                if (checker == "nocache") return false;
                var hqls = checker.split(false, true, '|');
                foreach (string hql in hqls){
                    var dt = myapp.storage.Get(type).First(type, hql).toDate();
                    if(dt>forTime) return false;
                }
                return true;
            }

        }
    }
}