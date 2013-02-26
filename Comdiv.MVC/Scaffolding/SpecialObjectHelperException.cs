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

namespace Comdiv.Web{
    public class SpecialObjectHelperException : Exception{
        public SpecialObjectHelperException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        public SpecialObjectHelperException(string message, Exception innerException) : base(message, innerException) {}

        public SpecialObjectHelperException(string message) : base(message) {}

        public SpecialObjectHelperException() {}
    }
}