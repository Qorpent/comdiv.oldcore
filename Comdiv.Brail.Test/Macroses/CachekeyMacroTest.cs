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
using Comdiv.Brail;
using Comdiv.Extensibility.Brail;
using MvcContrib.Comdiv.Extensibility.TestSupport;
using MvcContrib.Comdiv.ViewEngines.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    

    [TestFixture]
    [Category("Brail")]
    public class CachekeyMacroTest : BaseMacroTestFixture<CachekeyMacro>{
        [Test]
        public void from_argument(){
            checkMacro(
                @"cachekey ""${param}-${date}-${usr}""",
#if !LIB2
                @"public override def _key() as string:
	return '${param}-${date}-${usr}'");
#else
                           @"public override def _key() as string:
	return '$param-$date-$usr'");
#endif
        }

        [Test]
        public void from_body()
        {
            checkMacro(
                @"cachekey:
    x = 1
    return x.ToString()",

                @"public override def _key() as string:
	x = 1
	return x.ToString()");
        }

       
        
    }
}