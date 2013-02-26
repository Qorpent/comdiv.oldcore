using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Brail {
	[Serializable]
	public class BrailCompilerException : Exception,Qorpent.IExceptionRegistryDataException {
		public readonly ViewCompilerInfo info;
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//


		public BrailCompilerException(ViewCompilerInfo info, Exception inner) : base("Ошибка компиляции "+info.Sources.Select(x=>x.FileName).concat(","), inner) {
			this.info = info;
		}


		public IDictionary<string, string> ExceptionRegisryData { get { return null; } set {throw new NotSupportedException();} }
	}
}