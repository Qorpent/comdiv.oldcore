// // Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// // Supported by Media Technology LTD 
// //  
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //  
// //      http://www.apache.org/licenses/LICENSE-2.0
// //  
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// // 
// // MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using MvcContrib.Comdiv.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    [TestFixture]
    public class ViewCachingTest : BrailMacroTestBase{
        #region Setup/Teardown

        [SetUp]
        public void setup(){
            MyBrail.Default.Engine.OutputCache.Clear();
            MyBrail.Default.Engine.TemporaryCompiledCache.Clear();
        }

        #endregion

        [Test]
        public void can_define_param_based_cache(){
            var code =
                @"
#pragma boo
override def _key() as string:
    if usecache :
        return 'cached'
    else:
        return null
output x
";
            checkHtml(code, new{x = 3, usecache = false}, "3");
            Assert.AreEqual(0, MyBrail.Default.Engine.OutputCache.Count);
            checkHtml(code, new{x = 4, usecache = true}, "4");
            Assert.AreEqual(1, MyBrail.Default.Engine.OutputCache.Count);
            checkHtml(code, new{x = 2, usecache = true}, "4"); //this will be loaded from cache
            checkHtml(code, new{x = 2, usecache = false}, "2"); //this will be loaded not from cache
            checkHtml(code, new{x = 555, usecache = true}, "4"); //but cache is not dopped
        }

        [Test]
        public void check_cache_configured(){
            checkHtml(@"
#pragma boo
output 1
", "1");
            Assert.AreEqual(0, MyBrail.Default.Engine.OutputCache.Count);
            checkHtml(@"
#pragma boo
override def _key() as string:
    return 'static_cache'
output 1
", "1");
            Assert.AreEqual(1, MyBrail.Default.Engine.OutputCache.Count);
        }

        [Test]
        public void check_cache_reused(){
            checkHtml(@"
#pragma boo
override def _key() as string:
    return 'static_cache'
output x
",
                      new{x = 3}, "3");
            Assert.AreEqual(1, MyBrail.Default.Engine.OutputCache.Count);
            checkHtml(@"
#pragma boo
override def _key() as string:
    return 'static_cache'
output x
",
                      new{x = 4}, "3"); //must be loaded from cache
            Assert.AreEqual(1, MyBrail.Default.Engine.OutputCache.Count);
        }
    }
}