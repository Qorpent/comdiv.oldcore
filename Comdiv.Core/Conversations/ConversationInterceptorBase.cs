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

namespace Comdiv.Conversations{

    
    ///<summary>
    ///</summary>
    public class ConversationInterceptorBase : IConversationInterceptor {
        public class OnInterceptEventArgs:EventArgs{
            public bool Handled { get; set;}
            public OnInterceptEventArgs(ConversationLifeCycleStage stage, IConversation conversation){
                this.Stage = stage;
                this.Conversation = conversation;
            }

            public IConversation Conversation { get; protected set; }

            public ConversationLifeCycleStage Stage { get; protected set; }
        }

        public delegate void OnInterceptEventHandler(IConversationInterceptor sender, OnInterceptEventArgs args);
        public int Idx { get; set; }

        public event OnInterceptEventHandler OnIntercept;
        public void Execute(ConversationLifeCycleStage stage, IConversation conversation){
            if(OnIntercept!=null){
                var args = new OnInterceptEventArgs(stage, conversation);
                OnIntercept(this, args);
                if(args.Handled){
                    return;
                }
            }
            switch (stage){
                case ConversationLifeCycleStage.Init:
                    OnInit(conversation);
                    break;
                case ConversationLifeCycleStage.Enter:
                    OnEnter(conversation);
                    break;
                case ConversationLifeCycleStage.Leave:
                    OnLeave(conversation);
                    break;
                case ConversationLifeCycleStage.Finish:
                    OnFinish(conversation);
                    break;
            }
        }



        protected virtual void OnInit(IConversation conversation){
            
        }
        protected virtual void OnEnter(IConversation conversation)
        {

        }
        protected virtual void OnLeave(IConversation conversation)
        {

        }
        protected virtual void OnFinish(IConversation conversation)
        {

        }
    }

    
}