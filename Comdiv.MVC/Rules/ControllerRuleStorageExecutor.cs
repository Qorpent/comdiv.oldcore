using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Rules{
    public class ControllerRuleStorageExecutor : StorageQueryStep<ControllerRuleStorageExecutor>{
        public object sync = new object();

        public ControllerRuleStorageExecutor(){
            Loader = new ControllerRuleLoader();
            Cache = new Dictionary<string, IControllerExpert>();
            myapp.OnReload += (s, a) => Cache.Clear();
        }

        public IControllerRuleLoader Loader { get; set; }


        public IDictionary<string, IControllerExpert> Cache { get; set; }

        protected override bool internalIsApplyable(StorageQuery query){
            return query.TargetType.Equals(typeof (IControllerExpert));
        }
        protected override bool getSupport()
        {
            return true;
        }
        protected override object getLoad(){
            lock (sync){
                var expertKey = MyQuery.Key as string;
                var cacheKey = myapp.usrName + "/" + expertKey;
                return Cache.get(cacheKey,
                                 () =>{
                                     var result = Loader.Load(expertKey);
                                     return Container.all<IControllerExpertModifier>().Where(
                                                m => m.IsMatch(expertKey, result))
                                                .Select(m => m.Modify(result)).LastOrDefault() ?? result;
                                 }, true
                    );
            }
        }
    }

    
}