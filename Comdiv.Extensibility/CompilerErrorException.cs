//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Runtime.Serialization;
using Boo.Lang.Compiler;

namespace Comdiv.Extensibility {
    [Serializable]
    public class CompilerErrorException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CompilerErrorException(CompilerContext context) {
            CompilerContext = context;
        }


        public CompilerErrorException(CompilerContext context, string message) : base(message) {
            CompilerContext = context;
        }

        public CompilerErrorException(CompilerContext context, string message, Exception inner) : base(message, inner) {
            CompilerContext = context;
        }

        protected CompilerErrorException(
            CompilerContext ctx,
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
            CompilerContext = ctx;
        }

        public CompilerContext CompilerContext { get; private set; }
    }
}