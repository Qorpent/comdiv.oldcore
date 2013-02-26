// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Comdiv.MVC.Report{
    [Serializable]
    public class ReportLoadException : ReportException{
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ReportLoadException(string message, IDictionary<string, object> advancedParameters, string uid)
            : base(message){
            AdvancedParameters = advancedParameters;
            Uid = uid;
        }

        public ReportLoadException(SerializationInfo info, StreamingContext context, IReportRequest request)
            : base(info, context, request) {}

        public ReportLoadException(string message, Exception innerException, IReportRequest request)
            : base(message, innerException, request) {}

        public ReportLoadException(string message, IReportRequest request) : base(message, request) {}

        public ReportLoadException(IReportRequest request) : base(request) {}

        public ReportLoadException(IDictionary<string, object> advancedParameters, string uid){
            AdvancedParameters = advancedParameters;
            Uid = uid;
        }

        public ReportLoadException(SerializationInfo info, StreamingContext context,
                                   IDictionary<string, object> advancedParameters, string uid) : base(info, context){
            AdvancedParameters = advancedParameters;
            Uid = uid;
        }

        public ReportLoadException(string message, Exception inner, IDictionary<string, object> advancedParameters,
                                   string uid)
            : base(message, inner){
            AdvancedParameters = advancedParameters;
            Uid = uid;
        }

        public ReportLoadException() {}

        public ReportLoadException(string message) : base(message) {}

        public ReportLoadException(string message, Exception inner) : base(message, inner) {}

        protected ReportLoadException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}

        public string Uid { get; protected set; }
        public IDictionary<string, object> AdvancedParameters { get; protected set; }
    }
}