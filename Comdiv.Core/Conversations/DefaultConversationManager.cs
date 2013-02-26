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
using System.Threading;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Logging;

namespace Comdiv.Conversations
{
    public class DefaultConversationManager:IConversationManager,IWithContainer
    {
        public ILog log = logger.get("comdiv.conversations.manager");
        public static object sync = new object();
        private static int _id = 0;
        public DefaultConversationManager(){
            this.id = _id++;
            UseDefaultIocToInitInterceptiors = true;
            current = null;
        }
        [ThreadStatic] private static IConversation current;
        [Notice("thread static field used, U MUST to override Current in any descendant to provide really distinct currents for distinct types")]
        public virtual IConversation Current{
            get{
                return current;
            }
            
        }

        public bool UseDefaultIocToInitInterceptiors
        {
            get;
            set;
        }

        private IInversionContainer _container;

        public IInversionContainer Container{
            get { 
                if(_container.invalid()){
                    lock(this){
                        if(_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container; 
            }
            set { _container = value; }
        }

        private IList<IConversationInterceptor> interceptors;
        public IList<IConversationInterceptor> Interceptors{
            get{
                if(null==interceptors){
                    lock(this){
                        log.Info("start init interceptors");
                        if(null==interceptors){
                            if (UseDefaultIocToInitInterceptiors){
                                interceptors =
                                    new List<IConversationInterceptor>(
                                        Container.all<IConversationInterceptor>().OrderByDescending(x => x.Idx));
                            }else{
                                interceptors = new List<IConversationInterceptor>();
                            }
                        }
                        log.Info("end init interceptors - count == "+interceptors.Count);
                    }
                }
                return interceptors;
            }
            //test propose
            internal set{
                lock(this){
                    interceptors = new List<IConversationInterceptor>();                    
                }
            }
        }

        internal Dictionary<string, IConversation> ConversationList{
            get { return conversationList; }
        }

        private Dictionary<string, IConversation> conversationList = new Dictionary<string, IConversation>();
        private int id;

        public T Enter<T>(string owner, string code) where T:IConversation{
            return (T)Enter(typeof(T), owner, code);
        }


        public void Rename(string oldCode, string newCode){
            conversationList[newCode] = conversationList[oldCode];
            conversationList[newCode].Code = newCode;
            conversationList.Remove(oldCode);
        }

        protected IConversation Enter(Type type,string owner, string code){
            lock(this){
                if(owner.noContent()){
                    throw new ArgumentNullException("owner");
                }
                if(code.noContent()){
                    code = owner + "_default";
                }
                if(null!=Current && Current.Code==code){
                    if(!(type.IsAssignableFrom(Current.GetType()))){
                        throw new InvalidCastException("current conversation type mismatch");
                    }
                    return Current;
                }
                log.Info("start enter into conversation with code "+code+" for owner "+owner);

                
                IConversation result = null;
                if(!ConversationList.ContainsKey(code)){
                    log.Info("start create new conversation with code " + code + " for owner " + owner);
                    var impltype = type == typeof(IConversation) ? typeof(DefaultConversation) : type;
                    result = impltype.create<IConversation>();
                    result.Code = code;
                    result.Owner = owner;
                    ConversationList[result.Code] = result;
                    result.Init();
                    applyInterceptors(ConversationLifeCycleStage.Init,result);
                    log.Info("end create new conversation with code " + code + " for owner " + owner);
                }else{
                    var existed = ConversationList[code];
                    if (!(type.IsAssignableFrom(existed.GetType())))
                    {
                        throw new InvalidCastException("existed conversation type mismatch");
                    }
                    result =  existed;
                    
                }
                current = result;
                result.Enter();
                //if (result.ActualEnters == 1){
                    applyInterceptors(ConversationLifeCycleStage.Enter, result);
                    log.Info("finish enter into conversation with code " + code + " for owner " + owner);
                //}
                return result;
            }
        }
         
        public void Reset(){
            lock(this){
                log.Info("start reset");
                interceptors = null;
                current = null;
                foreach (var key in new List<string>(ConversationList.Keys)){
                    var conversation = ConversationList[key];
                    Finish(conversation);
                }
                log.Info("end reset");
            }
        }

       
        public void Leave(IConversation conversation){
            lock(this)
            {
                if (!ConversationList.ContainsKey(conversation.Code))
                {
                    throw new InvalidOperationException("try to finish conversation that is not managed by manager");
                }
                log.Info("start leave conversation with code " + conversation.Code + " for owner " + conversation.Owner);
                
                applyInterceptors(ConversationLifeCycleStage.Leave,conversation);
                conversation.Leave();

                if(current==conversation){
                    current = null;
                }

                if(conversation.Finished && conversation.ActualEnters==0){
                    Finish(conversation);
                }
                log.Info("finish leave conversation with code " + conversation.Code + " for owner " + conversation.Owner);
            }
        }

        private void applyInterceptors(ConversationLifeCycleStage stage, IConversation conversation){
            Interceptors.map(x =>{
                                 if(log.IsDebugEnabled){
                                     log.Debug("start apply {0} for {1}({2}) on {3}",x.GetType().Name,conversation.Code, conversation.Owner,stage);
                                 }
                                 x.Execute(stage, conversation);
                                 if (log.IsDebugEnabled)
                                 {
                                     log.Debug("end apply {0} for {1}({2}) on {3}", x.GetType().Name, conversation.Code, conversation.Owner, stage);
                                 }
                             },
                             (x,idx,ex)=>{
                                 log.Error(string.Format("error occured on {0} for {1}({2}) on {3}", x.GetType().Name, conversation.Code, conversation.Owner, stage),ex);
                                 if(ex.Message.Contains("[[FATAL]]")){
                                     throw ex;
                                 }
                             });
        }

        
        public void Finish(IConversation conversation){
            lock(this){
                if(!ConversationList.ContainsKey(conversation.Code)){
                    throw new InvalidOperationException("try to finish conversation that is not managed by manager");
                }
                log.Info("start finish conversation with code " + conversation.Code + " for owner " + conversation.Owner);
                applyInterceptors(ConversationLifeCycleStage.Finish, conversation);
                conversation.Finish();
                if (current == conversation){
                    current = null;
                }
                ConversationList.Remove(conversation.Code);
                log.Info("end finish conversation with code " + conversation.Code + " for owner " + conversation.Owner);
            }
        }

        public event ConversationEventHandler OnConversationEvent;
        public void InvokeEvent(string name,IConversation conversation, object data){
            if(OnConversationEvent!=null){
                var args = new ConversationEventArgs(name, conversation, data);
                OnConversationEvent(this, args);

            }
        }

        public IConversation[] GetActiveSnapshot(){
            lock (this){
                return this.ConversationList.Values.ToArray();
            }
        }
    }

    
}
