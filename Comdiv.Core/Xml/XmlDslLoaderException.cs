using System;
using System.Runtime.Serialization;

namespace Comdiv.Xml {
	/// <summary>
	/// any error in xml dsl loading
	/// </summary>
	[Serializable]
	public class XmlDslLoaderException : Exception {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public XmlDslLoaderException() {
		}

		public XmlDslLoaderException(string message) : base(message) {
		}

		public XmlDslLoaderException(string message, Exception inner) : base(message, inner) {
		}

		protected XmlDslLoaderException(
			SerializationInfo info,
			StreamingContext context) : base(info, context) {
		}
	}
}