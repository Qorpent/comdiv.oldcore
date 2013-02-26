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
using System.Linq;
using System.Text;
using Comdiv.Design;

namespace Comdiv.Common
{
    [NoCover]
    ///<summary>
    ///</summary>
    public class GroupException : Exception
    {
        public GroupException(IEnumerable<Exception> exceptions) : this(String.Empty, exceptions) { }

        public GroupException(string message, IEnumerable<Exception> exceptions)
            : base(message)
        {
            this.InnerExceptions = exceptions.ToArray();
        }

        public Exception[] InnerExceptions { get; protected set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Message);
            foreach (var exception in InnerExceptions)
            {
                if (null == exception) continue;
                builder.AppendLine("-----------------------------");
                builder.AppendLine(exception.GetType().Name + ": " + exception.Message + " " +
                                   exception.TargetSite);
                builder.AppendLine(exception.ToString());
                builder.AppendLine("-----------------------------");
            }
            return builder.ToString();
        }
    }
}