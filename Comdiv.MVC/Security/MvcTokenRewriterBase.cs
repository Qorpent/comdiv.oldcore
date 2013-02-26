using System;
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

namespace Comdiv.MVC.Security{
    public abstract class MvcTokenRewriterBase : TokenRewriterBase{
        public MvcTokenRewriterBase(){
            TargetType = typeof (IMvcContext);
        }

        protected override string rewrite(string currentResult, object targetObject, Type type, string key){
            var subresult = base.rewrite(currentResult, targetObject, type, key);
            return rewrite(subresult, (IMvcContext) targetObject);
        }

        protected abstract string rewrite(string currentresult, IMvcContext context);
    }


    public class MvcTokenByParameterRewriter : MvcTokenRewriterBase{
        public MvcTokenByParameterRewriter() {}

        public MvcTokenByParameterRewriter(string mask, params string[] parameters){
            Mask = "^/app/" + mask;
            Parameters = parameters;
        }

        public IList<string> Parameters { get; set; }

        protected override string rewrite(string currentresult, IMvcContext context){
            if (null == Parameters || 0 == Parameters.Count){
                return currentresult;
            }
            foreach (var param in Parameters){
                var p = "/" + param;
                var v = "null";
                if (context.ParamSource.ContainsKey(param)){
                    if (null != context.ParamSource[param]){
                        v = context.ParamSource[param].ToString();
                    }
                }
                currentresult += p + "_" + v;
            }
            return currentresult.ToLower();
        }
    }
}