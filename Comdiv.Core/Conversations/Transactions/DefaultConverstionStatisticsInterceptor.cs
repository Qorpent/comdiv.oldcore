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
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;


namespace Comdiv.Conversations{
    public class DefaultConverstionStatisticsInterceptor:ConversationInterceptorBase{

        protected override void OnInit(IConversation conversation)
        {
            base.OnInit(conversation);
            var c = ConversationStatistics.Get(conversation);
            c.InitTime = DateTime.Now;
        }
        protected override void OnEnter(IConversation conversation)
        {
            base.OnEnter(conversation);
            var c = ConversationStatistics.Get(conversation);
            c.Enters++;
            c.LastEnterTime = DateTime.Now;
            if(c.LastLeaveTime.Year>1900){
                c.StayTime += (c.LastEnterTime - c.LastLeaveTime);
            }
        }
        protected override void OnLeave(IConversation conversation)
        {
            base.OnLeave(conversation);
            var c = ConversationStatistics.Get(conversation);
            c.LastLeaveTime = DateTime.Now;
            c.WorkTime += c.LastLeaveTime - c.LastEnterTime;
        }
    }
}