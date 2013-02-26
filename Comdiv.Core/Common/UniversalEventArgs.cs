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


namespace Comdiv.Common{
    public class UniversalEventArgs<T, C, R> : EventArgs
        where C : struct{
        private readonly IDictionary<string, object> parameters = new
            Dictionary<string, object>();

        private bool canBeHandled;
        private bool cancel;
        private bool handled;

        private R returnValue;

        public UniversalEventArgs() {}

        public UniversalEventArgs(T data){
            Data = data;
        }

        public UniversalEventArgs(C eventType, T data, bool cancelAble, bool accepReturnValue){
            Data = data;
            EventClass = eventType;
            CancelAble = cancelAble;
            AcceptReturnValue = accepReturnValue;
        }

        public UniversalEventArgs(C eventType, T data, bool cancelAble, bool accepReturnValue, bool canBeHandled){
            Data = data;
            EventClass = eventType;
            CancelAble = cancelAble;
            AcceptReturnValue = accepReturnValue;
            CanBeHandled = canBeHandled;
        }

        public C EventClass { get; set; }

        public IDictionary<string, object> Parameters{
            get { return parameters; }
        }

        public T Data { get; set; }

        public R ReturnValue{
            get { return returnValue; }
            set{
                if (!AcceptReturnValue) return;
                returnValue = value;
            }
        }

        public bool Cancel{
            get { return cancel; }
            set{
                if (!CancelAble) return;
                cancel = value;
            }
        }

        public bool CancelAble { get; protected set; }

        public bool AcceptReturnValue { get; protected set; }

        public bool CanBeHandled{
            get { return canBeHandled; }
            protected set { canBeHandled = value; }
        }

        public bool Handled{
            get { return handled; }
            set{
                if (!canBeHandled) return;
                handled = value;
            }
        }
        }
}