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
using System.Linq;
using Comdiv.Extensions;
using log4net;
using log4net.Config;
using log4net.Core;

namespace Comdiv.Logging{
    public class Log4netILogManagerWrapper:ILogManager{
        private bool init = false;
        void checkInit(){
            if(!init){
                init = true;
                var file = new[] { "~/usr/log4net.config", "~/mod/log4net.config", "~/sys/log4net.config" }
                    .Select(s =>
                    {
                        //				File.AppendAllText(_file,"probe "+s);
                        return s.mapPath();
                    })
                    .FirstOrDefault(File.Exists);


                if (null == file)
                {
                    BasicConfigurator.Configure();
                    var r = LogManager.GetRepository();
                    r.Threshold = Level.Warn;

                }
                else
                {
                    //		File.AppendAllText(_file,"{0}:{1}".fmt(DateTime.Now,file));
                    XmlConfigurator.ConfigureAndWatch(new FileInfo(file));
                }
            }
        }
        public ILog GetLogger(Type type){
            lock(this){
                checkInit();
            }
            return new Log4netILogWraper( LogManager.GetLogger(type));
        }

        public ILog GetLogger(string name){
            lock (this){
                checkInit();
            }
            return new Log4netILogWraper(LogManager.GetLogger(name));
        }
    }
}