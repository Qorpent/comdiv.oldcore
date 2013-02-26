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
    public class TypeAliasHelperException : Exception{
        public TypeAliasHelperException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        public TypeAliasHelperException(string message, Exception innerException) : base(message, innerException) {}

        public TypeAliasHelperException(string message) : base(message) {}

        public TypeAliasHelperException() {}
    }
}