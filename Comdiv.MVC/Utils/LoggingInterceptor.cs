using System;
using System.Linq;
using System.Text;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
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

namespace Comdiv.MVC.Utils{
    public class LoggingInterceptor : IInterceptor{
        #region IInterceptor Members

        public void Intercept(IInvocation invocation){
            //var parameters = (invocation.Arguments.ToArray() as string[]);
            //if (parameters == null) {
            //    parameters = new[] {"no parameters"};
            //}
            //DevLogger.Debug("{0}.{1}({2})", invocation.TargetType.FullName, invocation.Method, parameters);
            //+ invocation.TargetType.FullName
            try{
                var sb = new StringBuilder(DevLogger.DefaultApplicatonCode)
                    .Append(": ")
                    //.Append(invocation.TargetType)
                    //.Append(": ")
                    .Append(DevLogger.EnteringMethodMsg)
                    .Append(" : [")
                    .Append(invocation.Method)
                    .Append("] with")
                    .Append("\r\n[");
                for (var i = 0; i < invocation.Arguments.Length; i++){
                    if (i > 0){
                        sb.Append(",");
                    }
                    sb.Append(invocation.Arguments[i]);
                }
                sb.Append("]");
                DevLogger.Debug(sb.ToString());
                invocation.Proceed();

                sb = new StringBuilder(DevLogger.DefaultApplicatonCode)
                    .Append(": ")
                    .Append(DevLogger.LeavingMethodMsg)
                    .Append(": [" + invocation.Method + "], the result is")
                    .Append(" \r\n[" + invocation.ReturnValue + "]");


                DevLogger.Debug(sb.ToString());
            }
            catch (Exception e){
                DevLogger.LogExceptions(e);
                throw;
            }
        }

        #endregion
    }
}