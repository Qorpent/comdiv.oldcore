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
using Comdiv.Common;

namespace Comdiv.Conversations{
    ///<summary>
    ///</summary>
    public class DefaultConversation : IConversation{
        public DefaultConversation(){
            Data  = new Dictionary<string, object>();
        }
        public string Code { get; set; }
        public string Owner { get; set; }
        public bool Finished { get; set; }
        public IDictionary<string, object> Data { get; private set; }
        public void Init(){
            BeforeInit.invoke(this);
            doInit();
            AfterInit.invoke(this);
        }

        protected virtual void doInit()
        {
            
        }

        public event EventHandler BeforeInit;
        public event EventHandler AfterInit;

        public void 
            
            Enter(){
                lock (this){
                    ActualEnters++;

                    BeforeEnter.invoke(this);
                    doEnter();
                    AfterEnter.invoke(this);
                }
        }

        protected virtual void doEnter(){
            
        }

        public event EventHandler BeforeEnter;
        public event EventHandler AfterEnter;

        public void Leave(){
            lock (this){
                ActualEnters--;
                BeforeLeave.invoke(this);
                doLeave();
                AfterLeave.invoke(this);
            }
        }

        protected virtual void doLeave()
        {
            
        }

        public event EventHandler BeforeLeave;
        public event EventHandler AfterLeave;

        public void Finish(){
            BeforeFinish.invoke(this);
            doFinish();
            AfterFinish.invoke(this);
            Finished = true;
        }

        public int ActualEnters
        {
            get; set;
        }

        public string Class
        {
            get; set;
        }

        protected virtual void doFinish()
        {
            
        }

        public event EventHandler BeforeFinish;
        public event EventHandler AfterFinish;
    }
}