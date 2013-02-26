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
using System.Collections.Generic;
using Comdiv.Extensions;

namespace Comdiv.Security.Acl{
    public class AclRuleComparer:IComparer<IAclRule>{
        public int Compare(IAclRule x, IAclRule y){
            if(x==y) return 0;
            //if x and y has different token length - more concrete is in priority
            if (x.TokenMask._length() != y.TokenMask._length()) return y.TokenMask._length().CompareTo(x.TokenMask._length());

            //if one has PrincipalMask about user and another not - user-oriented is more prioritized
            if (x.PrincipalMask._contains("\\") && !y.PrincipalMask._contains("\\"))
            {
                return -1;
            }
            if (y.PrincipalMask._contains("\\") && !x.PrincipalMask._contains("\\"))
            {
                return 1;
            }

            //if one has PrincipalMask about roles and another not - role-oriented is more prioritized
            if (x.PrincipalMask._starts("r:") && !y.PrincipalMask._starts("r:"))
            {
                return -1;
            }
            if (y.PrincipalMask._starts("r:") && !x.PrincipalMask._starts("r:"))
            {
                return 1;
            }

            //if one's type is not equal to another's type compare it
            if(x.RuleType!=y.RuleType){
                return x.RuleType.CompareTo(y.RuleType);
            }
            
            //if one has system and another not return system's upper
            if(string.IsNullOrWhiteSpace(x.System) && string.IsNullOrWhiteSpace(y.System)){
                return -1;
            }
            if (string.IsNullOrWhiteSpace(y.System) && string.IsNullOrWhiteSpace(x.System))
            {
                return 1;
            }

            return 0;
        }
    }
}