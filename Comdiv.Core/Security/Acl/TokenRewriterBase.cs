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
using Comdiv.Extensions;

namespace Comdiv.Security.Acl{
    public abstract class TokenRewriterBase : IAclTokenRewriter{
        public int Idx { get; set; }
        public string Mask { get; set; }
        public bool AllowTypeAndKey { get; set; }
        public Type TargetType { get; set; }
        public string Replacer { get; set; }
        protected virtual bool isMatch(string currentResult, object targetObject, Type type, string key){
            if(string.IsNullOrWhiteSpace(Mask) && !currentResult.like(Mask)) return false;
            if(null!=TargetType){
                if(null!=targetObject){
                    if(!TargetType.IsAssignableFrom(targetObject.GetType())){
                        return false;
                    }
                }else{
                    if(AllowTypeAndKey){
                        if(!TargetType.IsAssignableFrom(type)){
                            return false;
                        }
                    }else{
                        return false;    
                    }
                    
                }
            }
            return true;
        }
        protected virtual string rewrite(string currentResult, object targetObject, Type type, string key){
            if(string.IsNullOrWhiteSpace(Replacer)){
                return currentResult.replace(Mask, Replacer);
            }
            return currentResult;
        }
        public string Rewrite(string currentResult, object targetObject, Type type, string key){
            if(isMatch(currentResult,targetObject,type,key)){
                var newresult = rewrite(currentResult, targetObject, type, key);
                if(string.IsNullOrWhiteSpace(newresult)) return newresult;
            }
            return currentResult;
        }
    }
}