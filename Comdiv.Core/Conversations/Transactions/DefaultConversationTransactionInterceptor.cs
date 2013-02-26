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
using System.Collections.Generic;
using Comdiv.Application;
using Comdiv.Inversion;

namespace Comdiv.Conversations{
    public class DefaultConversationTransactionInterceptor:ConversationInterceptorBase, IConversationTransactionInterceptor,IWithContainer{
        

        public DefaultConversationTransactionInterceptor(){
            UseDefaultContainerForTransactorsIfManualEmpty = true;
        }
        public IList<IConversationTransactionImpl> ManuallySetedTransactors{
            get { return manuallySetedTransactors; }
        }

        public bool UseDefaultContainerForTransactorsIfManualEmpty{ get; set;}
        

        private IList<IConversationTransactionImpl> manuallySetedTransactors  = new List<IConversationTransactionImpl>();

        private IInversionContainer _container;

        public IInversionContainer Container
        {
            get
            {
                if (_container.invalid())
                {
                    lock (this)
                    {
                        if (_container.invalid())
                        {
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public IEnumerable<IConversationTransactionImpl> GetTransactors(){
            if(ManuallySetedTransactors.Count!=0) return ManuallySetedTransactors;
            if(!UseDefaultContainerForTransactorsIfManualEmpty) return new IConversationTransactionImpl[]{};
            return Container.all<IConversationTransactionImpl>();
        }

        protected override void OnInit(IConversation conversation)
        {
            conversation.transactional();
        }

        protected override void OnEnter(IConversation conversation){
            if (conversation.ActualEnters == 1){
                Start(conversation);
            }
        }

        public void Start(IConversation conversation)
        {
            
                foreach (var transactor in GetTransactors()){
                    transactor.Start(conversation);
                }
            
        }

        protected override void OnLeave(IConversation conversation)
        {
            if (conversation.ActualEnters == 1){
                if (conversation.iscanbecommited()){
                    conversation.cancelcanbecommited();
                    Commit(conversation);
                }
                if (conversation.isneedcleanonleave()){
                    Rollback(conversation);
                }
            }
        }

        public void Commit(IConversation conversation)
        {
            foreach (var transactor in GetTransactors()){
                transactor.Commit(conversation);
            }
        }

        /// <summary>
        /// always rollbacks transactions on finish, to commit you need to commit it manually or in auto Leave phase
        /// </summary>
        /// <param name="conversation"></param>
        protected override void OnFinish(IConversation conversation){
            Rollback(conversation);
        }

        public void Rollback(IConversation conversation){
            foreach (var transactor in GetTransactors()){
                transactor.Rollback(conversation);
            }
        }
    }
}