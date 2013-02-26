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
using System.Reflection;
using Comdiv.Common;

namespace Comdiv.QueryEngine{
    public class QueryResultCache<T, R, C, F> : IQueryResultCache<T, R, C, F> where T : IContextualQuery<R, C, F>
                                                                              where F : IContextualQuery<R, C, F>
                                                                              where C : IWithProperties{
        private IDictionary<string, cachePair> cache = new Dictionary<string, cachePair>();

        #region IQueryResultCache<T,R,C,F> Members

        public object Get(T query){
            lock (cache){
                var result = Missing.Value;
                var key = query.GetCacheKey();
                if (!cache.ContainsKey(key)){
                    return result;
                }
                var pair = cache[key];
                if (!pair.lease()){
                    cache.Remove(key);
                    return result;
                }
                return pair.result;
            }
        }

        public void Store(T query){
            lock (cache){
                var key = query.GetCacheKey();
                var lease = query.GetLeaseChecker();
                var value = query.Result;
                cache[key] = new cachePair(){lease = lease, result = value};
            }
        }

       

        public void Clear(){
            lock (cache){
                cache.Clear();
            }
        }
        public void Clear(Func<string,bool> keyfilter)
        {
            lock (cache)
            {
                foreach (var key in cache.Keys.ToArray()){
                    if(keyfilter(key)){
                        cache.Remove(key);
                    }
                }
            }
        }

        #endregion

        #region Nested type: cachePair

        public class cachePair{
            public Func<bool> lease;
            public R result;
        }

        #endregion
                                                                              }
}