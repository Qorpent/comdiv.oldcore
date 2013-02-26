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
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Inversion;
using Comdiv.Model;

namespace Comdiv.Security.Acl{
    public class DefaultAclTokenResolver:IAclTokenResolver,IWithContainer {
        public DefaultAclTokenResolver(){
            Resolvers = Container.all<IAclTokenResolverImpl>();
            
        }

        private IInversionContainer _container;

        public IInversionContainer Container
        {
            get
            {
                if (_container.invalid())
                {
                    lock (this)
                    {
                        if (_container.invalid())
                        {
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }


        public IEnumerable<IAclTokenResolverImpl> Resolvers { get; protected set; }
        
        /// <summary>
        /// "for this" sygnature for get token
        /// </summary>
        /// <param name="aclTarget"></param>
        /// <returns></returns>
        /// 
        [Extensible(typeof(IAclTokenRewriter), typeof(IAclTokenResolverImpl))]
        public string GetToken(object aclTarget){
            lock (this){
                var result = 
                    Resolvers.OrderBy(r => r.Idx).Select(r => r.GetToken(aclTarget)).Where(x => null != x).
                        FirstOrDefault();
                if(null==result) {
                    result= aclTarget.GetType().ToString() + "_" + aclTarget.Id() + "_" + aclTarget.Code();
                }

                //but it can be rewrited for some cases
                foreach (var all in Container.all<IAclTokenRewriter>())
                {
                    result = all.Rewrite(result, aclTarget,null,null);
                }
                return result;
            }
        }
        /// <summary>
        /// lookup sygnature for get token, type used as selector, and aclId as identifier
        /// </summary>
        /// <param name="aclType"></param>
        /// <param name="aclId"></param>
        /// <returns></returns>
        [Extensible(typeof(IAclTokenRewriter), typeof(IAclTokenResolverImpl))]
        public string GetToken(Type aclType, string aclId){
            lock (this){
                var result = 
                    Resolvers.OrderBy(r => r.Idx).Select(r => r.GetToken(aclType, aclId)).Where(x => null != x).
                        FirstOrDefault();
                if (null == result)
                {
                    throw new AclTokenResolverException();
                }
                //but it can be rewrited for some cases
                foreach (var all in Container.all<IAclTokenRewriter>())
                {
                    result = all.Rewrite(result, null, aclType, aclId);
                }
                return result;
            }
        }
       
    }
}