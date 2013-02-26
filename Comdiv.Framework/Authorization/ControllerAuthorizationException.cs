using System;
using System.Runtime.Serialization;
using System.Security.Principal;
using Castle.MonoRail.Framework;

namespace Comdiv.Authorization {
    [Serializable]
    public class ControllerAuthorizationException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ControllerAuthorizationException() {
        }

        public IController Controller { get; set; }
        public IPrincipal User { get; set; }
        public string Action { get; set; }

        public ControllerAuthorizationException(string message) : base(message) {
        }

        public ControllerAuthorizationException(string message, Exception inner) : base(message, inner) {
        }

        protected ControllerAuthorizationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }
    }
}