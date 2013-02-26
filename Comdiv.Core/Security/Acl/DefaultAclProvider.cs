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
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Logging;

namespace Comdiv.Security.Acl{
    public class DefaultAclProvider:IAclProviderService,IWithContainer {
        public bool Initialized { get; set; }

        public void Reload(){
            Initialized = false;
        }
      
        protected  IDictionary<string,AclResult> cache = new Dictionary<string, AclResult>();
        private IList<IAclRule> _rules;
        public IList<IAclRule> Rules{
            get{
                lock (this){
                    checkInitialization();
                    return _rules;
                }
            }
            protected set { _rules = value; }
        }

        
        public IAclRepository AclRepository { get; set; }

        public IEnumerable<IAclProviderImpl> AclProviders { get; set; }
        public IEnumerable<IAclRuleProvider> RuleProviders { get; set; }

        

        public AclResult Evaluate(AclRequest request){
            lock (this){
                checkInitialization();
                return cache.get(request.ToString(), () => internalEvaluate(request));
            }
        }

        public event EventHandler OnInitialize;

        private void InvokeOnInitialize(){
            InvokeOnInitialize(null);
        }

        private void InvokeOnInitialize(EventArgs e){
            EventHandler initialize = OnInitialize;
            if (initialize != null){
                initialize(this, e);
            }
        }

        protected AclResult internalEvaluate(AclRequest request){
            AclResult result = null;
            result = tryEvaluateBySubProviders(request);
            if(null!=result) return result;
            result = tryEvaluateByRules(request);
            if (null != result) return result;
            return new AclResult {AccessAllowed = true, Processed = false};
        }
static ILog log = logger.get("comdiv.security.acl");
        private AclResult tryEvaluateByRules(AclRequest request){

            var rules = new List<IAclRule>();
            foreach (var rule in Rules){
                if(!rule.Active) continue;
                if(!(rule.StartDate <= DateTime.Now && rule.EndDate >= DateTime.Now)) continue;
                if(!rule.Match(request)) continue;
                rules.Add(rule);
                
            }
            rules = rules.OrderBy(x => x, new AclRuleComparer()).ToList();
                
            if(log.IsDebugEnabled){
                log.Debug("matched rules for " + request + " (" + rules.concat("; ") + ")");    
            }
			
            if(rules.Count()==0) return null;
            var result = new AclResult {Processed = true, AccessAllowed = true};
            foreach (var rule in rules){
                if(AclRuleType.Deny==rule.RuleType){
                    result.AccessAllowed = false;
					log.debug(()=>"deny by "+rule);
                    break;
                }
                if(AclRuleType.Allow==rule.RuleType){
                    result.AccessAllowed = true;
					log.debug(()=>"allow by "+rule);
                    break;
                }
                if(AclRuleType.Require==rule.RuleType){
                    if(!rule.IsRequireMatched(request)){
                        result.AccessAllowed = false;
						log.debug(()=>"no meet require by "+rule);
                        break;
                    }
                }
            }
			log.debug(()=>"result "+result.AccessAllowed);
            return result;
        }

        private AclResult tryEvaluateBySubProviders(AclRequest request){
            if(null==AclProviders) return null;
            foreach (var impl in AclProviders.OrderBy(r=>r.Idx)){
                var result = impl.Evaluate(request);
                if(null!=result){
                    return result;
                }
            }
            return null;
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
            set
            {
                
                _container = value;
                

            }
        }
        
        protected void checkInitialization(){
            if(!Initialized){
                Initialize();
            }
        }
        public void Initialize(){
            AclProviders = Container.all<IAclProviderImpl>();
            RuleProviders = Container.all<IAclRuleProvider>();
            Application.myapp.OnReload += ReloadService_OnReload;
            reloadRules();
            cache.Clear();
            if(AclRepository!=null){
                AclRepository.OnChange -= AclOperator_OnChange;
                AclRepository.OnChange += AclOperator_OnChange;
            }
            Initialized = true;
            InvokeOnInitialize();
        }

        void ReloadService_OnReload(object sender, EventArgs e)
        {
            Reload();
        }
        void AclOperator_OnChange(object sender, EventArgs e)
        {
            Reload();
        }

        private void reloadRules(){
            var rules = new List<IAclRule>();
            foreach (var provider in RuleProviders){
                foreach (var rule in provider.GetRules()){
                    rules.Add(rule);
                }
            }
            this._rules = rules.OrderBy(x => x, new AclRuleComparer()).ToList();
        }
    }
}