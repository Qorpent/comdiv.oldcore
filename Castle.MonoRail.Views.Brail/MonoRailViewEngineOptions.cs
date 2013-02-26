// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

using Comdiv.Extensibility.Brail;

namespace Castle.MonoRail.Views.Brail
{
    using Boo.Lang.Extensions;
	using Framework;

    public class MonoRailViewEngineOptions : ViewEngineOptions {
        public MonoRailViewEngineOptions() : base() {
            this.ViewEngineType = "Castle.MonoRail.Views.Brail.BooViewEngine";
            this.BaseType = "Castle.MonoRail.Views.Brail.BrailBase";
            AssembliesToReference.Add(typeof(MonoRailViewEngineOptions).Assembly); //Brail's assembl
            AssembliesToReference.Add(typeof(Controller).Assembly); //MonoRail.Framework's assembly
            AssembliesToReference.Add(typeof(AssertMacro).Assembly); //Boo.Lang.Extensions assembly
        }
    }
}