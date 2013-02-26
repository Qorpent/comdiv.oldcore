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
using System.Collections;
using Comdiv.Extensions;
using Comdiv.QueryEngine;

namespace Comdiv.Persistence{
    public class StorageQueryStep<THIS> : QueryProcessorStep<StorageQuery, object, THIS>,IStorageQueryStep where THIS : StorageQueryStep<THIS>, new() {
        protected virtual bool supportsQueryType(StorageQueryType type){
            return true;
        }

        protected override void internalProcess(StorageQuery query){
            
                this.MyQuery = query;
                switch (query.QueryType){
                    case StorageQueryType.Delete:
                        doDelete();

                        break;
                    case StorageQueryType.Exists:
                        query.Result = getExists();
                        break;
                    case StorageQueryType.Load:
                        query.Result = getLoad();
                        break;
                    case StorageQueryType.Query:
                        var result = getQuery();
                        query.Result = result;
                        break;
                    case StorageQueryType.Save:
                        doSave();
                        break;
                    case StorageQueryType.Supports:
                        query.Result = getSupport();
                        break;
                    case StorageQueryType.Resolve:
                        query.Result = query.RealType;
                        break;
                    case StorageQueryType.New:
                        query.Result = query.RealType.create();
                        break;
                    case StorageQueryType.Refresh:
                        doRefresh(query.Target);
                        break;
                    case StorageQueryType.Custom1:
                        onCustom(1);
                        break;
                    case StorageQueryType.Custom2:
                        onCustom(2);
                        break;
                    case StorageQueryType.Custom3:
                        onCustom(3);
                        break;
                    case StorageQueryType.Custom4:
                        onCustom(4);
                        break;
                    case StorageQueryType.Custom5:
                        onCustom(5);
                        break;
                }
                query.CommandProcessed = true;
            

        }

        protected virtual bool getSupport(){
            return MyQuery.RealType != null;
        }

        protected virtual void doRefresh(object target){
            
        }

        protected  virtual void onCustom(int command_id){
            
        }

        protected virtual void doSave() {}

        protected virtual IEnumerable getQuery(){
            return null;
        }

        protected virtual object getLoad(){
            return null;
        }

        protected virtual object getExists(){
            return false;
        }

        protected virtual void doDelete() {}

        protected override bool internalIsApplyable(StorageQuery query){
            lock (this){
                if (!supportsQueryType(query.QueryType)){
                    return false;
                }
                return true;
            }
        }
    }
}