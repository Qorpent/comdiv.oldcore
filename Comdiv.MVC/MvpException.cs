using System;
using System.Linq;
using System.Runtime.Serialization;
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

namespace Comdiv.MVC{
    /// <summary>
    /// Корневое исключение, содержит весь контекст вызова
    /// </summary>
    public class MvpException : Exception{
        public MvpException(string message, IMvcContext mvcContext) : base(message){
            MVCContext = mvcContext;
        }

        public MvpException(IMvcContext mvcContext){
            MVCContext = mvcContext;
        }

        public MvpException(SerializationInfo info, StreamingContext context, IMvcContext mvcContext)
            : base(info, context){
            MVCContext = mvcContext;
        }

        public MvpException(string message, Exception innerException, IMvcContext mvcContext)
            : base(message, innerException){
            MVCContext = mvcContext;
        }

        public MvpException(string message, object contextImplementation, object engineContextImplementation,
                            IMvcContext mvcContext) : base(message){
            ContextImplementation = contextImplementation;
            EngineContextImplementation = engineContextImplementation;
            MVCContext = mvcContext;
        }

        public MvpException(object contextImplementation, object engineContextImplementation, IMvcContext mvcContext){
            ContextImplementation = contextImplementation;
            EngineContextImplementation = engineContextImplementation;
            MVCContext = mvcContext;
        }

        public MvpException(SerializationInfo info, StreamingContext context, object contextImplementation,
                            object engineContextImplementation, IMvcContext mvcContext) : base(info, context){
            ContextImplementation = contextImplementation;
            EngineContextImplementation = engineContextImplementation;
            MVCContext = mvcContext;
        }

        public MvpException(string message, Exception innerException, object contextImplementation,
                            object engineContextImplementation, IMvcContext mvcContext) : base(message, innerException){
            ContextImplementation = contextImplementation;
            EngineContextImplementation = engineContextImplementation;
            MVCContext = mvcContext;
        }

        public object ContextImplementation { get; protected set; }
        public object EngineContextImplementation { get; protected set; }

        public IMvcContext MVCContext { get; set; }
    }
}