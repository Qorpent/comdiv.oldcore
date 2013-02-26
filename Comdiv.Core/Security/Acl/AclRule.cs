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
using System.Security.Principal;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Logging;
using Qorpent.Security;

namespace Comdiv.Security.Acl{
    public class AclRule : IAclRule,IWithContainer {
        private IRoleResolver _resolver;

        public virtual IRoleResolver Resolver{
            get{
                if (null == _resolver) _resolver = Container.get<IRoleResolver>();
                return _resolver;
            }
            set { _resolver = value; }
        }

        public AclRule(){
           
            TokenMask = String.Empty;
            PrincipalMask = String.Empty;
            Permissions = String.Empty;
            System = String.Empty;
            Active = true;
            StartDate = DateExtensions.Begin;
            EndDate = DateExtensions.End;
        }
        private IInversionContainer _container;

        public virtual IInversionContainer Container
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
            set{
  
                _container = value;
  
                
            }
        }
        void _containerDispose(object sender,EventArgs eventArgs){
            if (sender.Equals(_container)){
                _container = null;
            }
        }

        public virtual string TokenMask { get; set; }
        public virtual string PrincipalMask { get; set; }
        public virtual AclRuleType RuleType { get; set; }
        public virtual string Permissions { get; set; }
        public virtual string System { get; set; }
        public virtual bool Active { get; set; }
        public virtual string Evidence { get; set; }
		static ILog log = logger.get("comdiv.security.acl");
        public virtual DateTime StartDate
        {
            get; set;
        }

        protected virtual string getRegex(){
            var result = "(?ix)^" + TokenMask;
            result = result.replace(@"\*\*+", @"[^/]+");
            result = result.replace(@"~+", @"[\s\S]*?");
            return result;
        }

        public virtual DateTime EndDate
        {
            get; set;
        }
		
        public virtual bool Match(AclRequest request){
			//check by token
			
            if (!string.IsNullOrWhiteSpace(TokenMask) && !request.Token.like(getRegex())) {
				log.debug(()=>String.Empty+this+" no match token");
				return false;
			}
            //check by system
            if (!string.IsNullOrWhiteSpace(System) && request.System!=System) {
				log.debug(()=>String.Empty+this+" no match system");
				return false;
			}
            //check by domain or user
            if (!string.IsNullOrWhiteSpace(PrincipalMask) && !PrincipalMask.StartsWith("r:")){
				
                if (PrincipalMask.Contains("\\"))
                {
                    if (request.Principal.Identity.Name != PrincipalMask) return false;
                }
                else{
                    if (!request.Principal.Identity.Name.StartsWith(PrincipalMask)) return false;
                }
            }
            //check by permission
            var rp = request.Permission;
            if (string.IsNullOrWhiteSpace(rp)) rp = "_NULL_";
            if (!string.IsNullOrWhiteSpace(Permissions) && !Permissions.Contains(rp + ";")){
				log.debug(()=>String.Empty+this+" no match permission");
				return false;
			}
            //check by role - if rule has special REQUIRE mode - roles are not checked at match stage, but collected in 
            //eval stage
            if (!string.IsNullOrWhiteSpace(PrincipalMask) && PrincipalMask.StartsWith("r:") && RuleType!=AclRuleType.Require)
            {
                if (!resolveByRoles(request)) return false;
            }
            
            return true;
        }

        public virtual bool IsRequireMatched(AclRequest request){
            return resolveByRoles(request);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)){
                return false;
            }
            if (ReferenceEquals(this, obj)){
                return true;
            }
            if (!typeof (IAclRule).IsAssignableFrom(this.GetType())){
                return false;
            }
            return Equals((IAclRule) obj);
        }
        private bool resolveByRoles(AclRequest request){
            var principal = request.Principal;
            return CheckRoles(principal);
        }
        [TestPropose(Description = "we make it public")]
        public virtual bool CheckRoles(IPrincipal principal) {
            if(Resolver==null) return false;
            var rulelist = PrincipalMask.Substring(2);
            var conjunctions = rulelist.Split('|');
            bool finded = false;
            foreach (var conjunction in conjunctions){
                var roles = conjunction.Split(',');
                finded = true;
                foreach (var role in roles){
                    if(!Resolver.IsInRole(principal,role)){
                        finded = false;
                        break;
                    }  
                }

                if (finded) break;
                    

            }
			log.debug(()=>principal.Identity.Name + "by roles "+finded+" of "+this);
            return finded;
        }

        public virtual bool Equals(IAclRule other){
            if (ReferenceEquals(null, other)){
                return false;
            }
            if (ReferenceEquals(this, other)){
                return true;
            }
            return Equals(other.TokenMask, TokenMask) && Equals(other.PrincipalMask, PrincipalMask) && Equals(other.RuleType, RuleType) && Equals(other.Permissions, Permissions) && Equals(other.System, System);
        }
		
		public override string ToString ()
		{
			return string.Format("[AclRule: TokenMask={0}, PrincipalMask={1}, RuleType={2}, Permissions={3}, System={4}, Active={5}, Evidence={6}, StartDate={7}, EndDate={8}]", TokenMask, PrincipalMask, RuleType, Permissions, System, Active, Evidence, StartDate, EndDate);
		}


        public override int GetHashCode(){
            unchecked{
                int result = (TokenMask != null ? TokenMask.GetHashCode() : 0);
                result = (result*397) ^ (PrincipalMask != null ? PrincipalMask.GetHashCode() : 0);
                result = (result*397) ^ RuleType.GetHashCode();
                result = (result*397) ^ (Permissions != null ? Permissions.GetHashCode() : 0);
                result = (result*397) ^ (System != null ? System.GetHashCode() : 0);
                return result;
            }
        }
    }
}