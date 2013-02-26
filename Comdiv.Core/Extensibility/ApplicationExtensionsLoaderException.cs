using System;
using System.Runtime.Serialization;
using System.Text;

namespace Comdiv.Extensibility.ExtensionsCompiler {
    [Serializable]
    public class ApplicationExtensionsLoaderException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ApplicationExtensionsLoaderException() {
        }

        public ApplicationExtensionsLoaderException(string message) : base(message) {
        }

        public ApplicationExtensionsLoaderException(string message, Exception inner) : base(message, inner) {
        }

        protected ApplicationExtensionsLoaderException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }

        public int ErrorCode { get; set; }
        public string OutputLog { get; set; }
        public string ErrorLog { get; set; }
        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine("Error while calling to extensionscompiler.exe");
            if(InnerException!=null) {
                sb.AppendLine(InnerException.ToString());
            }
            if(ErrorCode!=0) {
                sb.AppendLine("errorcode : " + ErrorCode);
            }
            if(!string.IsNullOrWhiteSpace(OutputLog)) {
                sb.AppendLine("output:");
                sb.AppendLine(OutputLog);
            }
            if(!string.IsNullOrWhiteSpace(ErrorLog)) {
                sb.AppendLine("error:");
                sb.AppendLine(ErrorLog);
            }
            return sb.ToString();
        }
    }
}