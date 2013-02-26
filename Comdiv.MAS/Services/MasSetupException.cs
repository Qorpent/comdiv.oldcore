using System;
using System.Runtime.Serialization;

namespace Comdiv.MAS {
    [Serializable]
    public class MasSetupException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MasSetupException() {
        }

        public MasSetupException(string message) : base(message) {
        }

        public MasSetupException(string message, Exception inner) : base(message, inner) {
        }

        protected MasSetupException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }
    }

    [Serializable]
    public class MasException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MasException() {
        }

        public MasException(string message) : base(message) {
        }

        public MasException(string message, Exception inner) : base(message, inner) {
        }

        protected MasException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }
    }
}