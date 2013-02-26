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
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Model.Interfaces;
using Comdiv.Security.Acl;
using NUnit.Framework;

namespace Comdiv.Security.Tests{
    public abstract class XmlTokenizerTest<T,S,I> where T: XmlBasedTokenProvider<S>,new() where S:class,IWithCode where I:S,new(){
        protected T tokenizer;
        protected S def;

        [SetUp]
        public void setup(){
            tokenizer = new T();
            def = new I {Code = "test"};
        }

        [Test]
        public void valid_class()
        {
            Assert.True((tokenizer is IAclTokenResolverImpl));
            ;
        }
        [Test]
        public void should_implement_by_IT_tokenizing(){
            var token = tokenizer.GetToken(def);
            Assert.NotNull(token);
            Assert.IsNotEmpty(token);
        }

        [Test]
        public void must_ignore_NOIT_by_IT_tokenizing()
        {
            Assert.Null(tokenizer.GetToken(new object()));
        }

        [Test]
        public void should_implement_by_TK_tokenizing(){
            var token = tokenizer.GetToken(typeof (S), "test");
            Assert.NotNull(token);
            Assert.IsNotEmpty(token);
        }

        [Test]
        public void must_ignore_NOIT_by_TK_tokenizing()
        {
            Assert.Null(tokenizer.GetToken(typeof(object), "test"));
        }

        [Test]
        public void rd_and_tk_tokens_must_be_identical()
        {
            Assert.AreEqual(tokenizer.GetToken(typeof(S), "test"), tokenizer.GetToken(def));

        }


        [Test]
        public void tokens_must_be_unique()
        {
            Assert.AreNotEqual(tokenizer.GetToken(typeof(S), "test"), tokenizer.GetToken(typeof(S), "test1"));

        }

        [Test]
        public void must_apply_prefixes()
        {
            tokenizer.Loaded = true;
            tokenizer.Prefixes["test"] = "the/testo";
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/the/testo/test/", tokenizer.GetToken("test"));
            Assert.AreEqual("/" + tokenizer.DefaultPrefix + "/test1/", tokenizer.GetToken("test1"));
        }

        [Test]
        public abstract void must_apply_suffixes();

        [Test]
        public void can_read_token_prefixes_from_xml()
        {
            myapp.files.Write("~/usr/"+tokenizer.FileName,
                                 @"<rat>
                    <p n='b/c'>
                           <r c='t1'/>
                           <p n='d'>
                                <r c='t2'/>
                                <p n='z'>
                                     <r c='t7'/>
                                </p>
                            </p>
                    </p>
                    </rat>");
            myapp.files.Write("~/mod/" + tokenizer.FileName,
                                 @"<rat>
                    <p n='f/g'>
                           <r c='t3'/>
                            <r c='t4'/>
                           <p n='h'>
                                <r c='t5'/>
                            </p>
                    </p>
                    </rat>");
            myapp.files.Write("~/sys/" + tokenizer.FileName,
                                 @"<rat>
                    <p n='f/g'>
                           <r c='t3'/>
                            
                           <p n='h'>
                                <r c='t6'/>
                            </p>
                    </p>
                    <p n='k'>
                        <r c='t4'/>
                        <r c='t1'/>
                         <r c='t7'/>
                    </p>
                    </rat>");
            tokenizer.Loaded = false;
            tokenizer.ReloadPrefixes();
            CollectionAssert.Contains(tokenizer.Prefixes.Keys, "t1");
            CollectionAssert.Contains(tokenizer.Prefixes.Keys, "t2");
            CollectionAssert.Contains(tokenizer.Prefixes.Keys, "t3");
            CollectionAssert.Contains(tokenizer.Prefixes.Keys, "t4");
            CollectionAssert.Contains(tokenizer.Prefixes.Keys, "t5");
            CollectionAssert.Contains(tokenizer.Prefixes.Keys, "t6");
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/b/c/t1/", tokenizer.GetToken("t1"));
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/b/c/d/t2/", tokenizer.GetToken("t2"));
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/f/g/t3/", tokenizer.GetToken("t3"));
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/f/g/t4/", tokenizer.GetToken("t4"));
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/f/g/h/t5/", tokenizer.GetToken("t5"));
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/f/g/h/t6/", tokenizer.GetToken("t6"));
            Assert.AreEqual("/"+tokenizer.DefaultPrefix+"/b/c/d/z/t7/", tokenizer.GetToken("t7"));
        }
    }
}