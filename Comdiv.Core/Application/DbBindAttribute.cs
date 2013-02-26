using System;
using Comdiv.Extensions;
using Comdiv.Persistence;
using Qorpent.Mvc;
using Qorpent.Mvc.Binding;
using Qorpent.Utils.Extensions;

namespace Comdiv.Application
{
    public class DbBindAttribute: BindAttribute,ICustomBindConverter
    {
        private StorageWrapper<object> storage;
        public string System { get; set; }
        public bool ContextSystem { get; set; }
		
        public  void SetConverted(object action, string val, IMvcContext context, Action<object,object> directsetter) {
			if(null==val) {
				directsetter(action, null);
				return;
			}
            var sys = System;
            if(ContextSystem) {
                sys = context.Get("system");
            }
            if(sys.noContent()) {
                sys = System;
                if(sys.noContent()) {
                    sys = "Default";
                }
            }
            
            this.storage = this.storage ?? myapp.storage.Get(TargetType, sys, true);
            object key = val;
            if(val.ToStr().like(@"^\d+$")) {
                key = key.toInt();
            }
            var realval = storage.Load(TargetType, key, sys);
            directsetter(action, realval);
        }
    }
}
