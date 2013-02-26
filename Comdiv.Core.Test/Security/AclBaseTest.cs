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
using Comdiv.Security.Acl;
using NUnit.Framework;

namespace Comdiv.Security.Tests{
    [TestFixture]
    public class AclBaseTest{
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

        public class tokenizer : IAclTokenResolverImpl{
            #region IAclTokenResolverImpl Members

            public string GetToken(object aclTarget){
                return aclTarget.ToString();
            }

            public string GetToken(Type aclType, string aclId){
                return aclType.Name + "_" + aclId;
            }

            public int Idx { get; set; }

            #endregion
        }

        public class tokenRewriter : IAclTokenRewriter{
            #region IAclTokenRewriter Members

            public int Idx { get; set; }

            public string Rewrite(string currentResult, object targetObject, Type type, string key){
                return currentResult + "/rw";
            }

            #endregion
        }

        private string mod = "<rules><deny token='/test' /></rules>";
        private string usrRule = "<rules><allow token='/test' principal='test\\test2' /></rules>";

        [Test, Ignore]
        public void a0_annotation(){
            Console.WriteLine("Работы по созданию общей таблицы ACL");
        }

        [Test]
        public void acl_provider_initialize_after_repository_changes(){
            ioc.clear();
            ioc.Container.setupSecurity();

            var i = 0;
            acl.provider.OnInitialize += (s, e) => i++;
            acl.get("/test");
            //check that it was called
            Assert.AreEqual(1, i);
            acl.repository.Add(new AclRule(), AclRuleStorageLevel.Memory);
            acl.get("/test2");
            //check that it was called twice
            Assert.AreEqual(2, i);
        }

        [Test]
        public void acl_provider_initialize_is_not_called_twice(){
            ioc.finish();
            ioc.Container.setupSecurity();
            var i = 0;
            acl.provider.OnInitialize += (s, e) => i++;
            acl.get("/test");
            //check that it was called
            Assert.AreEqual(1, i);
            acl.get("/test2");
            //check that it was not called twice
            Assert.AreEqual(1, i);
        }

        [Test]
        public void acl_rules_can_use_pseudo_regexes(){
            Assert.True(
                new AclRule{TokenMask = "/app/*****/***/index"}.Match(new AclRequest
                                                                      {Token = "/app/area/controller/index"}));
            Assert.True(new AclRule{TokenMask = "/app/~/index"}.Match(new AclRequest{Token = "/app/area/index"}));
            Assert.True(
                new AclRule{TokenMask = "/app/~~~~~~~~/index"}.Match(new AclRequest
                                                                     {Token = "/app/area/controller/after/index"}));
            Assert.True(
                new AclRule{TokenMask = "/app/~/index"}.Match(new AclRequest{Token = "/app/area/controller/after/index"}));
            Assert.False(
                new AclRule{TokenMask = "/app/*****/***/index"}.Match(new AclRequest
                                                                      {Token = "/app/area/controller/after/index"}));
        }

        [Test]
        public void acl_rules_can_use_regexes(){
            Assert.True(new AclRule{TokenMask = "/t.*"}.Match(new AclRequest{Token = "/test"}));
            Assert.False(new AclRule{TokenMask = "/t.*"}.Match(new AclRequest{Token = "/an/test"}));
            Assert.True(new AclRule{TokenMask = @"[\s\S]+?t.*"}.Match(new AclRequest{Token = "/an/test"}));
        }

        [Test]
        public void bug_001_token_rule_was_not_meet(){
            ioc.finish();
            ioc.Container.setupSecurity();
            Assert.True(new AclRule{RuleType = AclRuleType.Allow, TokenMask = "/test"}.Match(
                new AclRequest{
                                  Permission = acl.accessPermission,
                                  Principal = myapp.usr,
                                  System = "",
                                  Token = "/test"
                              }
                            ));
        }

        [Test]
        [Ignore("it's empty in case that it's TDD created test environment")]
        public void bug_002_user_is_empty_why(){
            ioc.finish();
            ioc.Container.setupSecurity();
            Assert.IsNotEmpty(myapp.usr.Identity.Name);
        }

        [Test]
        public void bug_003_provider_do_not_use_rules_from_memory(){
            ioc.finish();
            ioc.Container.setupSecurity();
            var dp = acl.provider as DefaultAclProvider;
            dp.Initialize();
            Assert.AreEqual(0, dp.Rules.Count);
            acl.repository.Add(new AclRule{RuleType = AclRuleType.Allow, TokenMask = "/test"});
            dp.Initialize();
            Assert.AreEqual(1, dp.Rules.Count);
        }

        [Test]
        public void bug_004_provider_should_see_memory_rules_provider(){
            ioc.finish();
            ioc.Container.setupSecurity();
            var dp = acl.provider as DefaultAclProvider;
            dp.Initialize();
            Assert.AreEqual(1, dp.RuleProviders.OfType<IAclInMemoryRuleProvider>().Count());
        }

        [Test]
        public void bug_005_in_memory_returns_rules_not_correctly(){
            var rp = new DefaultAclInMemoryRuleProvider();
            Assert.AreEqual(0, rp.GetRules().Count());
            rp.Add(new AclRule());
            Assert.AreEqual(1, rp.GetRules().Count());
        }

        [Test]
        public void bug_006_user_current_works_and_role_resolver_too(){
            ioc.finish();
            ioc.Container.setupSecurity();
            myapp.reload(10);
            myapp.SetCurrentUser("test111".toPrincipal());
            Console.WriteLine("1");
            Assert.AreNotEqual("test", myapp.usrName);
            Assert.False(myapp.roles.IsInRole("role1"), "is in role");
            Console.WriteLine("2");
            myapp.SetCurrentUser("test".toPrincipal("role1", "role2"));
            Assert.AreEqual("local\\test", myapp.usrName);
            Assert.True(myapp.roles.IsInRole("role1"), "is in role 2");
            Console.WriteLine("3");
            ioc.finish();
            ioc.Container.setupSecurity();
            myapp.reload(10);
            myapp.SetCurrentUser("test".toPrincipal());
            Assert.False(myapp.roles.IsInRole("role1"));
        }

        [Test]
        public void bug_007_require_rules_must_match_correctly(){
            ioc.finish();
            ioc.Container.setupSecurity();
            myapp.reload(5);
            //myapp.SetCurrentUser();
            //myapp.roles.IsInRole("role1").Should("is in role").Be.False();
            myapp.SetCurrentUser("test".toPrincipal("role1", "role2"));
            Assert.AreEqual("local\\test", myapp.usrName);
            Assert.True(new AclRule
                        {RuleType = AclRuleType.Require, TokenMask = "/test/token/data", PrincipalMask = "r:role1"}
                            .Match(new AclRequest{Principal = myapp.usr, Token = "/test/token/data"}));
        }

        [Test]
        public void bug_008_require_rules_must_match_require_correctly(){
            ioc.finish();
            ioc.Container.setupSecurity();

            myapp.SetCurrentUser("test_bug_008_require_rules_must_match_require_correctly".toPrincipal("role1", "role2"));
            Assert.True(new AclRule
                        {RuleType = AclRuleType.Require, TokenMask = "/test/token/data", PrincipalMask = "r:role1"}
                            .IsRequireMatched(new AclRequest{Principal = myapp.usr, Token = "/test/token/data"}));
        }

        [Test]
        public void bug_009_acl_rule_role_resolver_not_works(){
            ioc.finish();
            ioc.Container.setupSecurity();
            myapp.SetCurrentUser("test".toPrincipal("role1", "role2"));
            Assert.True(new AclRule
                        {RuleType = AclRuleType.Require, TokenMask = "/test/token/data", PrincipalMask = "r:role1"}
                            .CheckRoles(myapp.usr));
        }

        [Test]
        public void bug_010_permission(){
            ioc.finish();
            ioc.Container.setupSecurity();

            myapp.SetCurrentUser("ugmk\test2".toPrincipal("OPERATOR"));
            AclRule r1 = null;
            Assert.False((r1 =
                          new AclRule{
                                         RuleType = AclRuleType.Require,
                                         TokenMask = "/input",
                                         PrincipalMask = "r:UNDERWRITER",
                                         Permissions = "underwrite;"
                                     }
                         ).IsRequireMatched(new AclRequest
                                            {Principal = myapp.usr, Token = "/input", Permission = "underwrite"}));
            AclRule r2 = null;
            Assert.True((r2 =
                         new AclRule
                         {RuleType = AclRuleType.Require, TokenMask = "/input", PrincipalMask = "r:OPERATOR",})
                            .IsRequireMatched(new AclRequest
                                              {Principal = myapp.usr, Token = "/input", Permission = "underwrite"}));


            acl.repository.Add(r1);
            acl.repository.Add(r2);
            Assert.True(acl.get("/input", "access"));
            Assert.False(acl.get("/input", "underwrite"));
        }

        [Test]
        public void custom_providers_must_override_rules(){
            ioc.finish();
            ioc.Container.setupSecurity();
            myapp.SetCurrentUser("test\\test".toPrincipal());
            acl.repository.Add(new AclRule{RuleType = AclRuleType.Deny, TokenMask = "/test"});
            Assert.False(acl.get("/test", true));
            ioc.set<TestAclProvider1>(new TestAclProvider1());
            acl.provider.Reload();
            Assert.True(acl.get("/test", true));

            myapp.SetCurrentUser("test\\test2".toPrincipal());
            Assert.False(acl.get("/test", true));
        }


        [Test]
        public void deny_can_be_overrided_for_more_concrete_token_with_allow_but_not_at_this_level(){
            ioc.finish();
            ioc.Container.setupSecurity();
            Assert.True(acl.get("/test/token", true));
            acl.repository.Add(new AclRule{RuleType = AclRuleType.Deny, TokenMask = "/test"});
            Assert.False(acl.get("/test/token", true));
            acl.repository.Add(new AclRule{RuleType = AclRuleType.Allow, TokenMask = "/test"});
            Assert.False(acl.get("/test/token", true));
            acl.repository.Add(new AclRule{RuleType = AclRuleType.Allow, TokenMask = "/test/token"});
            Assert.True(acl.get("/test/token", true));
        }

        [Test]
        public void deny_rules_blocks_access(){
            ioc.finish();
            ioc.Container.setupSecurity();
            Assert.True(acl.get("/test", true));
            acl.repository.Add(new AclRule{RuleType = AclRuleType.Deny, TokenMask = "/test"});
            Assert.False(acl.get("/test", true));
        }


        [Test]
        public void no_any_data_result_not_processed(){
            ioc.finish();
            ioc.Container.setupSecurity();
            Assert.False(acl.provider.Evaluate(new AclRequest{Token = "/test"}).Processed);
            Assert.False(acl.get("/test", false));
            Assert.True(acl.get("/test", true));
        }

        [Test]
        public void require_works_by_cascade(){
            ioc.finish();
            ioc.Container.setupSecurity();
            myapp.SetCurrentUser("test".toPrincipal("role1", "role2"));
            Assert.False(acl.get("/test/token/data", false));
            acl.repository.Add(new AclRule{
                                              RuleType = AclRuleType.Require,
                                              TokenMask = "/test/token/data",
                                              PrincipalMask = "r:role1"
                                          });
            acl.repository.Add(new AclRule{
                                              RuleType = AclRuleType.Require,
                                              TokenMask = "/test/token",
                                              PrincipalMask = "r:role2"
                                          });
            Assert.True(acl.get("/test/token/data", false));
            acl.repository.Add(new AclRule
                               {RuleType = AclRuleType.Require, TokenMask = "/test", PrincipalMask = "r:role3"});
            Assert.False(acl.get("/test/token/data", false));
        }

        [Test]
        public void rules_works_in_memory_provider(){
            ioc.finish();
            ioc.Container.setupSecurity();
            Assert.False(acl.get("/test"));
            acl.repository.Add(new AclRule{RuleType = AclRuleType.Allow, TokenMask = "/test"});
            Assert.True(acl.get("/test"));
        }

        [Test]
        public void t01_ioc_locator_security_must_include_acl_provider_and_acl_operator(){
            ioc.finish();
            ioc.Container.setupSecurity();
            ioc.get<IAclProviderService>();
            ioc.get<IAclRepository>();
            ioc.get<IAclTokenResolver>();
            ioc.get<IAclInMemoryRuleProvider>();
            ioc.get<IAclApplicationRuleProvider>();
        }

        [Test]
        public void t02_main_interfaces_has_their_default_implementations(){
            typeof (IAclRepository).getDefaultImplementation();
            typeof (IAclProviderService).getDefaultImplementation();
            typeof (IAclTokenResolver).getDefaultImplementation();
            typeof (IAclInMemoryRuleProvider).getDefaultImplementation();
            typeof (IAclApplicationRuleProvider).getDefaultImplementation();
            typeof (IAclInMemoryRuleProvider).getDefaultImplementation();
        }

        [Test]
        public void t03_token_resolver_loads_implementations(){
            ioc.finish();
            ioc.Container.setupSecurity();
            ioc.set<ResolverImpl1>().set<ResolverImpl2>();

            var resolver = ioc.get<IAclTokenResolver>() as DefaultAclTokenResolver;
            Assert.AreEqual(2, resolver.Resolvers.Count());
            Assert.NotNull(resolver.Resolvers.FirstOrDefault(x => x is ResolverImpl1));
            Assert.NotNull(resolver.Resolvers.FirstOrDefault(x => x is ResolverImpl2));
        }

        [Test]
        public void t04_default_token_resolver_can_use_resolvers(){
            ioc.finish();
            ioc.Container.setupSecurity();
            ioc.set<ResolverImpl1>().set<ResolverImpl2>();

            Assert.AreEqual("/rc/2", acl.token(new ClassToResolve2{Id = 2}));
            Assert.AreEqual("/rc/3", acl.token<ClassToResolve2>(3));
            Assert.AreEqual("ClassToResolve1", acl.token(new ClassToResolve1()));
            Assert.AreEqual("ClassToResolve1/key", acl.token<ClassToResolve1>("key"));
        }


        [Test]
        public void t05_unknown_tokens_are_not_processed(){
            ioc.finish();
            ioc.Container.setupSecurity();
            ioc.set<ResolverImpl1>().set<ResolverImpl2>();
            Assert.AreEqual("/rc/2", acl.token(new ClassToResolve2{Id = 2}));
            Assert.AreEqual("/rc/3", acl.token<ClassToResolve2>(3));
            Assert.AreEqual("ClassToResolve1", acl.token(new ClassToResolve1()));
            Assert.AreEqual("ClassToResolve1/key", acl.token<ClassToResolve1>("key"));
        }

        [Test]
        public void tokenizing_can_be_modified_by_custom_token_rewriters(){
            ioc.finish();
            ioc.Container.setupSecurity();
            ioc.set<IAclTokenResolverImpl>(new tokenizer());
            Assert.AreEqual("/test", acl.token("/test"));
            ioc.set<IAclTokenRewriter>(new tokenRewriter());
            Assert.AreEqual("/test/rw", acl.token("/test"));
        }
    }
}