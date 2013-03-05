﻿// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
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

namespace Comdiv.Logging
{
    public class Log4netILogWraper:ILog
    {
        public readonly log4net.ILog RealLog;

        public Log4netILogWraper(log4net.ILog log ){
            this.RealLog = log;
        }
        public void Debug(string message, params object[] formats){
            RealLog.DebugFormat(message,(object[])formats);
        }

        public void Info(string message, params object[] formats){
            RealLog.InfoFormat(message, (object[])formats);
        }

        public void Warn(string message, params object[] formats){
            RealLog.WarnFormat(message, (object[])formats);
        }

        public void Warn(string message, Exception ex){
            RealLog.Warn(message,ex);
        }

        public void Error(string message, params object[] formats){
            RealLog.ErrorFormat(message, (object[])formats);
        }

        public void Error(string message, Exception ex){
            RealLog.Error(message, ex);
        }

       

        public bool IsDebugEnabled{
            get { return RealLog.IsDebugEnabled; }
        }

        public bool IsWarnEnabled{
            get { return RealLog.IsWarnEnabled; }
        }

        public bool IsErrorEnabled{
            get { return RealLog.IsErrorEnabled; }
        }

        public bool IsInfoEnabled{
            get { return RealLog.IsInfoEnabled; }
        }

        
    }
}