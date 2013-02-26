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
using Comdiv.Inversion;

namespace Comdiv.Security.Acl{
    public static class AclRepositoryExtensions{
        [ThreadStatic] private static AclRuleStorageLevel level = AclRuleStorageLevel.Memory;
        public static IAclRepository temp(this IAclRepository repository){
            level = AclRuleStorageLevel.Application;
            return repository;
        }
        public static IAclRepository local(this IAclRepository repository)
        {
            level = AclRuleStorageLevel.Memory;
            return repository;
        }
        public static IAclRepository db(this IAclRepository repository)
        {
            level = AclRuleStorageLevel.Database;
            return repository;
        }


        public static IAclRepository need(this IAclRepository repository, string token,string principal,params Action<IAclRule>[] ctors){
            return need(repository, token, principal, String.Empty, ctors);
        }

        public static IAclRepository need(this IAclRepository repository, string token,string principal, string permission,params Action<IAclRule>[] ctors){
            return add(repository, token, principal, permission, AclRuleType.Require, ctors);
        }

        public static IAclRepository deny(this IAclRepository repository, string token,string principal,params Action<IAclRule>[] ctors){
            return deny(repository, token, principal, String.Empty, ctors);
        }

        public static IAclRepository deny(this IAclRepository repository, string token,string principal, string permission,params Action<IAclRule>[] ctors){
            return add(repository, token, principal, permission, AclRuleType.Deny, ctors);
        }

        public static IAclRepository allow(this IAclRepository repository, string token,string principal,params Action<IAclRule>[] ctors){
            return allow(repository, token, principal, String.Empty, ctors);
        }

        public static IAclRepository allow(this IAclRepository repository, string token,string principal, string permission,params Action<IAclRule>[] ctors){
            return add(repository, token, principal, permission, AclRuleType.Allow, ctors);
        }

        public static IAclRepository add(this IAclRepository repository, string token,string principal, string permission, AclRuleType type,params Action<IAclRule>[] ctors){
            IAclRule rule = new AclRule
                            {
                                Permissions = permission,
                                TokenMask = token,
                                PrincipalMask = principal,
                                RuleType = type,
                                Container = (repository as IWithContainer).getContainer(),

                            };
            if(null!=ctors){
                foreach (var func in ctors){
                    func(rule);
                }
            }
            repository.Add(rule, level);
            return repository;
        }
        
    }
}