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
using Comdiv.Design;

namespace Comdiv.Conversations{

    public static class ConversationManagerShortcutExtensions{
        public static IConversation Enter(this IConversationManager manager, string owner){
            return manager.Enter(owner, null);
        }
        public static IConversation Enter(this IConversationManager manager, string owner, string code)
        {
            return manager.Enter<IConversation>(owner, code);
        }
        public static T Enter<T>(this IConversationManager manager, string owner) where T:IConversation
        {
            return manager.Enter<T>(owner, null);
        }
        public static void Leave(this IConversationManager manager){
            manager.Leave(manager.Current);
        }
        public static void Finish(this IConversationManager manager)
        {
            manager.Finish(manager.Current);
        }
    }

    public interface  IConversationManager{
        IList<IConversationInterceptor> Interceptors { get;  }
        [Notice("thread static field used, U MUST to override Current in any descendant to provide really distinct currents for distinct types")]
        IConversation Current { get; }
        T Enter<T>(string owner, string code) where T : IConversation;
        void Reset();
        void Leave(IConversation conversation);        
        void Finish(IConversation conversation);
        event ConversationEventHandler OnConversationEvent;
        void InvokeEvent(string name,IConversation conversation, object data);
        IConversation[] GetActiveSnapshot();
    }
}