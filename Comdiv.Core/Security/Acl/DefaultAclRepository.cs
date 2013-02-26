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

namespace Comdiv.Security.Acl{
    public class DefaultAclRepository:IAclRepository,IWithContainer{
        public event EventHandler OnChange;

        public IAclRepository Add(IAclRule rule){
            return Add(rule, AclRuleStorageLevel.Memory);
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

        public IAclRepository Add(IAclRule rule, AclRuleStorageLevel level){
            if(AclRuleStorageLevel.Memory==level){
                Container.get<IAclInMemoryRuleProvider>().Add(rule); 
            }
            else if(AclRuleStorageLevel.Application==level){
                Container.get<IAclApplicationRuleProvider>().Add(rule);
            }
            else{
                throw  new NotSupportedException("Пока только в память умею писать");
            }

            invokeOnChange();

            return this;
        }

        public IAclRepository Remove(IAclRule rule){
            return Remove(rule, AclRuleStorageLevel.Memory);
        }

        public IAclRepository Remove(IAclRule rule, AclRuleStorageLevel level)
        {
            if (AclRuleStorageLevel.Memory == level)
            {
                Container.get<IAclInMemoryRuleProvider>().Remove(rule); 
            }
            else if (AclRuleStorageLevel.Application == level)
            {
                Container.get<IAclApplicationRuleProvider>().Remove(rule);
            }
            else
            {
                throw new NotSupportedException("Пока только в память умею писать");
            }
            invokeOnChange();
            return this;
        }

        public IAclRepository Clear(){
            return Clear(AclRuleStorageLevel.Memory);
        }

        public IAclRepository Clear(AclRuleStorageLevel level)
        {
            if (AclRuleStorageLevel.Memory == level)
            {
                Container.get<IAclInMemoryRuleProvider>().Clear();
            }
            else if (AclRuleStorageLevel.Application == level)
            {
                Container.get<IAclApplicationRuleProvider>().Clear();
            }
            else
            {
                throw new NotSupportedException("Пока только в память умею писать");
            }
            invokeOnChange();

            return this;
        }

        protected void invokeOnChange(){
            invokeOnChange(EventArgs.Empty);
        }

        protected void invokeOnChange(EventArgs e){
            EventHandler change = OnChange;
            if (change != null){
                change(this, e);
            }
        }
    }
}