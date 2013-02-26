using System;
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
    public class ByNamePropertyValueApplyer:ValueApplyer<object>{
        public override void DoApply(object target, string prop, string value){
            var property = target.GetType().resolveProperty(prop);
            if(null==property){
                throw new Exception("cannot find property {0} on {1}"._format(prop,target.GetType().Name));
            }
            target.setPropertySafe(prop, value);
            
        }
    }
}