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
using System.Reflection;
using System.Text;
using Comdiv.Extensions;

namespace Comdiv.Logging{
    ///<summary>
    ///</summary>
    public static class logger{
        private static readonly object sync = new object();
        private static ILogManager _manager;
        public static bool PreventFromLoadingDefaultLog4NetManager { get; set; }
        public static readonly ClapanConsoleTextWriter DefaultOutput = new ClapanConsoleTextWriter();
        public static ILogManager Manager{
            get{
                
                if (null == _manager){
                    lock (sync){
                        bool isdebugmode = AppDomain.CurrentDomain.FriendlyName.Contains("nunit");
                        
                        DefaultOutput.UseConsole = !isdebugmode;
                        if (null == _manager){
                             
                            if (!PreventFromLoadingDefaultLog4NetManager && !isdebugmode){
                                try{
                                    var type =
                                        "Comdiv.Logging.Log4netILogManagerWrapper, Comdiv.Dependency".toType();
                                    _manager = type.create<ILogManager>();
                                }catch(NullReferenceException){
                                    
                                }
                            }

                            if (null == _manager){
                                //HACK: uses console log writer with all levels if is called under nuinit
                                //otherwise - console just for wars-fatal
                                
                                _manager = new TextWriterLogManager(DefaultOutput, false, false);
                            }
                        }
                    }
                }
                return _manager;
            }
            set{
                lock (sync){
                    _manager = value;
                }
            }
        }

        public static ILog get<T>(){
            return get(typeof (T));
        }

        public static ILog get(Type type){
            return Manager.GetLogger(type);
        }

        public static ILog get(string name){
            return Manager.GetLogger(name);
        }
    }

    ///<summary>
    ///</summary>
    public class ClapanConsoleTextWriter : TextWriter{
        public ClapanConsoleTextWriter(){
            swriter = new StringWriter(Buffer);
        }
        public bool UseConsole { get; set; }
        public override void Write(string value)
        {
            if(UseConsole)Console.Write(value);
            swriter.Write(value);
        }
        public override void WriteLine(string value)
        {
            Write(value);
            Write(Environment.NewLine);
        }
        private StringBuilder buffer = new StringBuilder();
        private StringWriter swriter;

        public StringBuilder Buffer{
            get { return buffer; }
        }

        public override Encoding Encoding{
            get { return Encoding.UTF8; }
        }
    }
}