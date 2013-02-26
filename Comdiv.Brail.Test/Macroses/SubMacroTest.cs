// Copyright 2007-2009 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
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
using Comdiv.Brail;
using Comdiv.Extensibility.Brail;
using MvcContrib.Comdiv.Extensibility.TestSupport;
using MvcContrib.Comdiv.ViewEngines.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    

    [TestFixture]
    [Category("Brail")]
    public class SubMacroTest : BaseMacroTestFixture<SubMacro>{
        [Test]
        public void MainTest(){
            checkMacro(
                @"sub v,{@name:@n}",

                @"OutputSubView('v', { @name: @n })");
        }

        [Test]
        public void usingVarName_test()
        {
            checkMacro(
                @"sub @v,{@name:@n}",

                @"OutputSubView(v, { @name: @n })");
        }

       [Test]
        public void minimal_notation_checked(){
           var result = compile("sub");
           Assert.NotNull(result.Errors.FirstOrDefault(x => x.Message.Contains("sub macro requires")));
       }
        
    }
}