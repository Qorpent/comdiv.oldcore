using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Model.Admin{
    public abstract class ValueApplyer<T> : IValueApplyer{
        public void Apply(object target, string prop, string value){
            DoApply((T)target,prop,value);
        }
        public abstract void DoApply(T target, string prop, string value);
    }
}