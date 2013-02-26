// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Threading;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC{
    public class Async : Controller{
        #region Delegates

        public delegate string Output();

        #endregion

        private readonly Output output;

        public Async(){
            output = (LongOp);
        }

        public IAsyncResult BeginIndex(){
            return output.BeginInvoke(ControllerContext.Async.Callback, ControllerContext.Async.State);
        }

        private string LongOp(){
            Thread.Sleep(250);

            return "foo";
        }

        public void restest(){
            throw new Exception("ldldldl");
        }

        public void EndIndex(){
            var s = output.EndInvoke(ControllerContext.Async.Result);
            RenderText(s);
        }
    }
}