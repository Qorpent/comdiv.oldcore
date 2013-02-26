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
using System.IO;

namespace Comdiv.Logging{
    ///<summary>
    ///</summary>
    public class TextWriterLogManager : ILogManager {
        public TextWriterLogManager() : this(true) {}
        public TextWriterLogManager(bool debug) : this(debug, true) {}
        public TextWriterLogManager(bool debug, bool info) : this(debug, info, true) {}
        public TextWriterLogManager(bool debug, bool info, bool warn) : this(debug, info, warn, true) {}
        public TextWriterLogManager(bool debug, bool info, bool warn, bool error) : this(null, debug, info, warn, error) {}

        public TextWriterLogManager(TextWriter output) : this(output, true) {}
        public TextWriterLogManager(TextWriter output, bool debug) : this(output, debug, true) {}
        public TextWriterLogManager(TextWriter output, bool debug, bool info) : this(output, debug, info, true) {}
        public TextWriterLogManager(TextWriter output, bool debug, bool info, bool warn) : this(output, debug, info, warn, true) {}

        public TextWriterLogManager(TextWriter output, bool debug, bool info, bool warn, bool error){
            this.Output = output ?? Console.Out;
            this.IsDebugEnabled = debug;
            this.IsInfoEnabled = info;
            this.IsWarnEnabled = warn;
            this.IsErrorEnabled = error;
        }

        protected bool IsErrorEnabled { get; set; }

        protected bool IsWarnEnabled { get; set; }

        protected bool IsInfoEnabled { get; set; }

        protected bool IsDebugEnabled { get; set; }

        protected TextWriter Output { get; private set; }

        public ILog GetLogger(Type type){
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name){
            return new TextWriterLog(name, Output){
                                                      IsDebugEnabled = this.IsDebugEnabled,
                                                      IsInfoEnabled = this.IsInfoEnabled,
                                                      IsWarnEnabled = this.IsWarnEnabled,
                                                      IsErrorEnabled = this.IsErrorEnabled
                                                  };

        }
    }
}