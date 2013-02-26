//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using NUnit.Framework;

namespace Comdiv.Booxml.Test {
    [TestFixture]
    public class BooxmlGeneratorTest : BooxmlBaseTest {
        [Test]
        public void attributes_generated() {
            Assert.AreEqual("namespace x\r\n\r\na 1", gen("<x><a id='1'/></x>"));
        }

        [Test]
        public void cdata_generated() {
            Assert.AreEqual("namespace x\r\n\r\na :\r\n\t'text'", gen("<x><a><![CDATA[text]]></a></x>"));
        }

        [Test]
        public void elements_generated() {
            Assert.AreEqual("namespace x\r\n\r\na :\r\n\tb", gen("<x><a><b/></a></x>"));
        }

        [Test]
        public void namespece_generated() {
            Assert.AreEqual(@"namespace theroot", gen("<theroot/>"));
        }

        [Test]
        public void text_generated() {
            Assert.AreEqual("namespace x\r\n\r\na :\r\n\t'text'", gen("<x><a>text</a></x>"));
        }
    }
}