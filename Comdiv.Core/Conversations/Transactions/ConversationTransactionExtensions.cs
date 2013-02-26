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
using System.Linq;
using Comdiv.Extensions;

namespace Comdiv.Conversations{
    ///<summary>
    ///</summary>
    public static class ConversationTransactionExtensions{



        public static TConversationManager setupTransactionSupport<TConversationManager>(this TConversationManager manager, params IConversationTransactionImpl[] implementations)
            where TConversationManager:IConversationManager
        {
            if (implementations == null || implementations.Length == 0){
                if (null == manager.Interceptors.OfType<IConversationTransactionInterceptor>().FirstOrDefault()){
                    manager.Interceptors.Add(new DefaultConversationTransactionInterceptor());
                }
            }else{
                var i = new DefaultConversationTransactionInterceptor();
                i.UseDefaultContainerForTransactorsIfManualEmpty = false;
                foreach (var impl in implementations){
                    i.ManuallySetedTransactors.Add(impl);
                }
                manager.Interceptors.Add(i);
            }
            return manager;
        }


        public static TConversationManager setupStatisticsSupport<TConversationManager>(this TConversationManager manager)
           where TConversationManager : IConversationManager
        {
           
                var i = new DefaultConverstionStatisticsInterceptor();
               
                manager.Interceptors.Add(i);
            
            return manager;
        }

        public static IConversation transactional(this IConversation conversation){
            conversation.Data["__t_yes"] = true;
            return conversation;
        }
        public static bool istransactional(this IConversation conversation)
        {
            return conversation.Data.get<bool>("__t_yes");
        }

        public static IConversation needcleanonleave(this IConversation conversation)
        {
            conversation.Data["__t_ncol"] = true;
            return conversation;
        }
       
        public static IConversation cancelneedcleanonleave(this IConversation conversation)
        {
            conversation.Data["__t_ncol"] = false;
            return conversation;
        }
        public static bool isneedcleanonleave(this IConversation conversation)
        {
            return conversation.Data.get<bool>("__t_ncol");
        }

        public static IConversation canbecommited(this IConversation conversation)
        {
            conversation.Data["__t_cbc"] = true;
            return conversation;
        }
        public static IConversation cancelcanbecommited(this IConversation conversation)
        {
            conversation.Data["__t_cbc"] = false;
            return conversation;
        }
        public static bool iscanbecommited(this IConversation conversation)
        {
            return conversation.Data.get<bool>("__t_cbc");
        } 
    }
}