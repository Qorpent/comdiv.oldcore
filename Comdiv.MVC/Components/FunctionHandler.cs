using System;
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

namespace Comdiv.Web{
    public class FunctionHandler<P, R>{
        private readonly Func<P, R> h;

        public FunctionHandler(Func<P, R> func){
            h = func;
        }

        public R call(P parameter){
            return h(parameter);
        }
    }
}