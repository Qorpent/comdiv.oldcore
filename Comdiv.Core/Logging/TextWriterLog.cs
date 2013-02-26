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
using Comdiv.Extensions;

namespace Comdiv.Logging{
    public class TextWriterLog : ILog {
        public string Name { get; protected set; }
        public TextWriterLog(string name,TextWriter output){
            Name = name;
            Output = output;
        }
        public void Debug(string message, params object[] formats){
            message = "debug: "+message._format(formats);
            write(message);
        }

        private void write(string message){
            Output.WriteLine(Name+": "+message);
            Output.Flush();
        }

        public void Info(string message, params object[] formats){
            message = "info: " + message._format(formats);
            write(message);
        }

        public void Warn(string message, params object[] formats){
            if (Output == Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            message = "warn: " + message._format(formats);
            write(message);
            if (Output == Console.Out)
            {
                Console.ResetColor();
            }
        }

        public void Warn(string message, Exception ex){
            if (Output == Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            message = "warn: " + message +" exception:\r\n"+ex;
            write(message);
            if (Output == Console.Out)
            {
                Console.ResetColor();
            }
        }

        public void Error(string message, params object[] formats){
            if(Output==Console.Out){
                Console.ForegroundColor = ConsoleColor.Red;
            }
            message = "error: " + message._format(formats);
            write(message);
            if (Output == Console.Out)
            {
                Console.ResetColor();
            }
        }

        public void Error(string message, Exception ex){
            if (Output == Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            message = "error: " + message + " exception:\r\n" + ex;
            write(message);
            if (Output == Console.Out)
            {
                Console.ResetColor();
            }
        }

        public bool IsDebugEnabled
        {
            get; set;
        }

        public bool IsWarnEnabled{
            get;
            set;
        }

        public bool IsErrorEnabled{
            get;
            set;
        }

        public bool IsInfoEnabled{
            get;
            set;
        }

        public TextWriter Output { get; private set; }
    }
}