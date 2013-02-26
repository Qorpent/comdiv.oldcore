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

namespace Comdiv.Model.Lookup{
    /// <summary>
    /// Корневое исключение Comdiv.Model.Lookup
    /// </summary>
    public class LookupException : ModelException{
        public LookupException() {}
        public LookupException(string message) : base(message) {}
        public LookupException(string message, Exception innerException) : base(message, innerException) {}
        public LookupException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}