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
using System.ComponentModel;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Inversion;

namespace Comdiv.QueryEngine{
    public class QueryProcessorStep<TQuery, TResult, TSelf> : IQueryProcessorStep<TQuery, TResult> where TQuery : IQuery<TResult>
           
        where TSelf : QueryProcessorStep<TQuery, TResult, TSelf>, new(){
        private volatile TSelf _current;
        private Pool<TSelf> pool;
        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        protected bool Syncronized;

        public QueryProcessorStep(){
            Primary = true;
        }

        public TSelf Parent { get; set; }

        ///<summary>
        ///</summary>
        public bool Primary { get; set; }

        protected TQuery MyQuery { get; set; }

       


        #region IQueryProcessorStep<T,R> Members

        public void Process(TQuery query){
            if (Primary && !Syncronized){
                if (pool == null){
                    lock (this){
                        if (pool == null){
                            pool = new Pool<TSelf>();
                        }
                    }
                }
                lock (pool){
                    _current = pool.Get(() => new TSelf{
                                                       Parent = (TSelf) this,
                                                       Primary = false
                                                   }, q => q.MyQuery = query
                                                   );
                }
                _current.Process();
            }
            else{
                var myq = MyQuery;
                try{
                    MyQuery = query;
                    Process();
                }finally{
                    MyQuery = myq;
                }
            }
        }

        public int Idx { get; set; }

        #endregion

        public void Release(TSelf instance){
            if (pool != null){
                pool.Return(instance);
            }
        }

        public void Process(){
            try{
                
                if (IsApplyableTo(MyQuery)){
                    internalProcess(MyQuery);
                }
            }
            finally{
                if (null != Parent){
                    Parent.Release((TSelf) this);
                }
            }
        }

        public bool IsApplyableTo(TQuery query){
            return internalIsApplyable(query);
        }

        protected virtual bool internalIsApplyable(TQuery query){
            return true;
        }

        protected virtual void internalProcess(TQuery query) {}
                                                                         }
}