using System;
using System.Runtime.Serialization;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	[Serializable]
	public class RolesActionException : Exception {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public RolesActionException() {}
		public RolesActionException(string message) : base(message) {}
		public RolesActionException(string message, Exception inner) : base(message, inner) {}

		protected RolesActionException(
			SerializationInfo info,
			StreamingContext context) : base(info, context) {}
	}
}