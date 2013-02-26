// Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//  Supported by Media Technology LTD 
//   
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Security.Acl;
using NUnit.Framework;

namespace Comdiv.Security.Tests{
    [TestFixture]
    public class AclTestWithNonMigratedThings{
        public class ResolverImpl1 : IAclTokenResolverImpl{
            public ResolverImpl1(){
                Idx = 2;
            }

            #region IAclTokenResolverImpl Members

            public string GetToken(object aclTarget){
                return aclTarget.GetType().Name;
            }

            public string GetToken(Type aclType, string aclId){
                return aclType.Name + "/" + aclId;
            }

            public int Idx { get; set; }

            #endregion
        }

        public class ClassToResolve2{
            public int Id { get; set; }
        }

        public class ClassToResolve1{
            public int Id { get; set; }
        }

        public class ResolverImpl2 : IAclTokenResolverImpl{
            public ResolverImpl2(){
                Idx = 1;
            }

            #region IAclTokenResolverImpl Members

            public string GetToken(object aclTarget){
                if (aclTarget is ClassToResolve2){
                    return "/rc/" + ((ClassToResolve2) aclTarget).Id;
                }
                return null;
            }

            public string GetToken(Type aclType, string aclId){
                if (typeof (ClassToResolve2).IsAssignableFrom(aclType)){
                    return "/rc/" + aclId;
                }
                return null;
            }

            public int Idx { get; set; }

            #endregion
        }

        public class TestAclProvider1 : IAclProviderImpl{
            public bool Used { get; set; }

            #region IAclProviderImpl Members

            public AclResult Evaluate(AclRequest request){
                Used = true;
                if (request.Principal.Identity.Name == "test\\test"){
                    return new AclResult{Processed = true, AccessAllowed = true};
                }
                return null;
            }

            public IList<IAclRule> Rules{
                get { throw new NotImplementedException(); }
            }


            public int Idx { get; set; }

            #endregion
        }


        //TODO: return test back

        //[Test]
        //public void t05_rules_well_order()
        //{
        //    var rules = new[]
        //                    {
        //                        new AclRuleEntity {Id = 1, TokenMask = "/test"},
        //                        new AclRuleEntity {Id = 2, TokenMask = "/test/z/x"},
        //                        new AclRuleEntity {Id = 3, TokenMask = "/test/a"},
        //                        new AclRuleEntity {Id = 4, TokenMask = "/test/z", RuleType = AclRuleType.Allow},
        //                        new AclRuleEntity {Id = 5, TokenMask = "/test/z", RuleType = AclRuleType.Deny},
        //                        new AclRuleEntity
        //                            {
        //                                Id = 6,
        //                                TokenMask = "/test/x",
        //                                RuleType = AclRuleType.Allow,
        //                                PrincipalMask = "test\\test2"
        //                            },
        //                        new AclRuleEntity {Id = 7, TokenMask = "/test/x", RuleType = AclRuleType.Deny},
        //                    };
        //    var ordered = rules.OrderBy(x => x, new AclRuleComparer()).ToList();
        //    Action<int, int> before = (x, y) =>
        //    {
        //        var x_ = ordered.First(r => r.Id == x);
        //        var y_ = ordered.First(r => r.Id == y);
        //        (ordered.IndexOf(x_) < ordered.IndexOf(y_)).Should().Be.True();
        //    };
        //    before(3, 1);
        //    before(2, 3);
        //    before(5, 4);
        //    before(6, 7);
        //}

        private string mod = "<rules><deny token='/test' /></rules>";
        private string usrRule = "<rules><allow token='/test' principal='test\\test2' /></rules>";

        [Test]
        public void can_define_local_application_rules(){
            ioc.finish();
            ioc.Container.setupSecurity();
            ioc.Container.setupFilesystem();
            myapp.files.Write("~/mod/acl.config", mod);
            myapp.files.Write("~/usr/acl.config", usrRule);
            var appRules = ioc.get<IAclApplicationRuleProvider>();
            Assert.AreEqual(2, appRules.GetRules().Count());
            appRules.Clear();
            var content = "";
            Assert.AreNotEqual(usrRule, (content = myapp.files.Read("~/usr/acl.config")));
            Assert.AreEqual(mod, myapp.files.Read("~/mod/acl.config"));

            //check that mod file is not affected
            Assert.AreEqual(1, appRules.GetRules().Count());
            var rule = new AclRule{PrincipalMask = "test\\test3", TokenMask = "/test/test3"};
            appRules.Add(rule);
            CollectionAssert.Contains(appRules.GetRules(),rule);
            Assert.AreNotEqual(content, myapp.files.Read("~/usr/acl.config"));
            Assert.AreEqual(mod, myapp.files.Read("~/mod/acl.config"));
            appRules.GetRules().First(x => x.Equals(rule));
        }


        [Test]
        public void local_app_rules_provider_works(){
            ioc.finish();
            ioc.Container.setupSecurity();
            ioc.Container.setupFilesystem();
            myapp.reload(5);
            myapp.files.Write("~/mod/acl.config", mod);
            myapp.files.Write("~/usr/acl.config", usrRule);
            acl.provider.Reload();

            var appRules = ioc.get<IAclApplicationRuleProvider>().GetRules();
            Assert.AreEqual(2, appRules.Count());
            (acl.provider as DefaultAclProvider).Initialize();
            Assert.AreEqual(2, (acl.provider as DefaultAclProvider).Rules.Count);
            myapp.SetCurrentUser("test\\test".toPrincipal());
            Assert.False(acl.get("/test", true));
            myapp.SetCurrentUser("test\\test2".toPrincipal());
            Assert.True(acl.get("/test", true));
        }
    }
}