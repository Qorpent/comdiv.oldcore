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
using System.Diagnostics;
using Comdiv.Common;
using Comdiv.Extensions;

namespace Comdiv.Persistence{
    ///<summary>
    ///</summary>
    public class StorageQueryStepWithResolver<THIS> : StorageQueryStep<THIS>,
                                        IStorageQueryStep where THIS : StorageQueryStepWithResolver<THIS>, new()
    {
        private IDictionary<string, Type> resolveTypeMap;

        protected IDictionary<string, Type> ResolveTypeMap{
            [DebuggerStepThrough] //trivial
            get{
                lock (this){
                    if (null != Parent){
                        return Parent.ResolveTypeMap;
                    }
                    if (null == resolveTypeMap){
                        lock (this){
                            if (null == resolveTypeMap){
                                resolveTypeMap = new Dictionary<string, Type>();
                            }
                        }
                    }
                    return resolveTypeMap;
                }
            }
        }

        private Type ResolveType(Type type, string system){
            lock (this){
                if (null == type){
                    return null;
                }
                    system = system.hasContent() ? system : String.Empty;
                    var key = system + "/" + type.FullName;
                    return ResolveTypeMap.get(key, () => internalResolveType(type, system));
                
            }
        }

        protected virtual Type internalResolveType(Type type, string system){
            return null;
        }

        protected override void internalProcess(StorageQuery query)
        {
            MyQuery = query;
            if(MyQuery.QueryType==StorageQueryType.Supports){
                query.finish((object)true);
                query.CommandProcessed = true;
                return;
            }
            base.internalProcess(MyQuery);
        }

        protected override bool internalIsApplyable(StorageQuery query)
        {
            lock(this){
                var res = base.internalIsApplyable(MyQuery);
                if(!res) return false;
                var realtype = ResolveType(MyQuery.TargetType, MyQuery.System);
                if (null == realtype)
                {
                    return false;
                }
                MyQuery.RealType = realtype;
                return true;
            }
        }
    }
}