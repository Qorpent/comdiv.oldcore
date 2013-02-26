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
using System.Linq;
using System.Reflection;
using Comdiv.Application;
using Comdiv.Common;
using Comdiv.Design;
using Comdiv.Inversion;
using Comdiv.Logging;

namespace Comdiv.QueryEngine{
    ///<summary>
    ///</summary>
    ///<typeparam name="TQuery"></typeparam>
    ///<typeparam name="TResult"></typeparam>
    ///<typeparam name="TContext"></typeparam>
    ///<typeparam name="TInnerQuery"></typeparam>
    ///<typeparam name="TStep"></typeparam>
    public class QueryProcessor<TQuery, TResult, TContext, TInnerQuery, TStep> : IQueryProcessor<TQuery, TResult>, IWithQueryCache<TQuery, TResult, TContext, TInnerQuery>,IWithContainer 
        where TQuery : IContextualQuery<TResult, TContext, TInnerQuery>
        
        where TStep : IQueryProcessorStep<TQuery, TResult>
        where TInnerQuery : class, IContextualQuery<TResult, TContext, TInnerQuery>, new() where TContext : IWithProperties, new(){
        private readonly QueryResultCache<TQuery, TResult, TContext, TInnerQuery> _cache = new QueryResultCache<TQuery, TResult, TContext, TInnerQuery>();
        public ILog log;
        public bool usedebug;
        public QueryProcessor(){
            log = logger.get(GetType().FullName.ToLower());
            AutoFinish = true;
            UseIocOnPipelineConstruction = true;
            usedebug = log.IsDebugEnabled;
#if FAIL_SAFE
            pi = new QueryProcessorInstance<TQuery, TResult, TContext, TInnerQuery, TStep>(this,default(TQuery));
#endif
        }

        public QueryResultCache<TQuery, TResult, TContext, TInnerQuery> Cache{
            get { return _cache; }
        }
        

        public bool AutoFinish
        {
            get;
            set;
        }

        public bool UseContext
        {
            get;
            set;
        }


        private IList<TStep> pipeline;
        public IList<TStep> Pipeline{
            get{
                if(null==pipeline){
                    lock (this){
                        if(null==pipeline){
                            if (UseIocOnPipelineConstruction){
                                pipeline = this.Container.all<TStep>().OrderBy(x=>x.Idx).ToList();
                            }else{
                                pipeline = new List<TStep>();
                                
                            }
                            preparePipeline();
                        }
                    }
                }
                return pipeline;
            }
        }

        protected  virtual void preparePipeline(){
            
        }

        #region IQueryProcessor<T,R> Members

        private volatile bool busy;

        public  void Process(TQuery query){
#if FAIL_SAFE
            lock (this){
                //pi = pi ?? new QueryProcessorInstance<TQuery, TResult, TContext, TInnerQuery, TStep>(this, default(TQuery));
                pi.SetQuery(query);
                pi.Process();
            }

               
#else
            new QueryProcessorInstance<TQuery, TResult, TContext, TInnerQuery, TStep>(this, prepare(query)).Process();
#endif

        }

#if FAIL_SAFE
        private QueryProcessorInstance<TQuery, TResult, TContext, TInnerQuery, TStep> pi;
#endif

        protected virtual TQuery prepare(TQuery query){
            return query;
        }

        #endregion
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
        public bool UseIocOnPipelineConstruction { get; set; }

        ///<summary>
        ///</summary>
        ///<typeparam name="T"></typeparam>
        ///<typeparam name="R"></typeparam>
        ///<typeparam name="C"></typeparam>
        ///<typeparam name="F"></typeparam>
        ///<typeparam name="S"></typeparam>
     //   [DebuggerStepThrough]
        internal class QueryProcessorInstance<T, R, C, F, S>
            where T : IContextualQuery<R, C, F>
            where C : IWithProperties, new()
            where F : class, IContextualQuery<R, C, F>, new() where S : IQueryProcessorStep<T, R>
        {
            private static int _id = 0;
            private int id = _id++;
            private QueryProcessor<T, R, C, F, S> processor;

            private T query;

#if FAIL_SAFE
            public void SetQuery(T query){
                this.query = query;
            }
#endif

            public QueryProcessorInstance(QueryProcessor<T, R, C, F, S> processor, T query)
            {
                this.processor = processor;

                this.query = query;
            }

            public void Process()
            {
                if (processor.usedebug){
                    processor.log.Debug("start process query:\r\n" + query);
                    
                }
                bool usecache = query.IsCacheAble;
                if (!usecache || !tryGetFromCache())
                {

                    processLine(processor.Pipeline);

                }
                if(usecache)saveCache();
                if(processor.usedebug){
                    var res = "end query " + query.Code + " finished: " + query.IsFinished +
                              " , result : " + query.Result;
                    if (query.Error != null)
                    {
                        res += ", error: " + query.Error.Message;
                    }
    
                    processor.log.Debug(res);
                }
                
            }

            

            private void saveCache()
            {
                lock (processor.Cache)
                {
                    processor.Cache.Store(query);
                }
            }

            private void processLine(IList<S> line)
            {
                
                if (line.Count != 0)
                {
                    if (processor.usedebug){
                        processor.log.Debug("pipeline aquired with " + line.Count + " items ");
                    }
                    C context = default(C);
                    if (processor.UseContext){
                        context = new C();
                        context.Properties["___processor"] = processor;
                        context.Properties["___real_processor"] = this;
                        context.Properties["___line"] = line;
                        query.Context = context;
                    }
                    if (processor.usedebug){
                        processor.log.Debug("query " + query.Code + " contextualized");
                    }
                    foreach (var step in line.OrderBy(x=>x.Idx).ToArray())
                    {
                        if (processor.usedebug){
                            processor.log.debug(() => "start step" + step + " for query " + query.Code);
                        }
                        if (processor.UseContext){
                             context.Properties["___step"] = step;
                        }
                        try
                        {
                            step.Process(query);
                        }
                        catch (Exception ex)
                        {
                            processor.log.Warn("error was occured during processing query :" + query + " in " + step +
                                               " step");
                            query.Error = ex;
                        }
                        if (query.IsFinished || query.Error != null)
                        {
                            break;
                        }
                    }

                    if (processor.AutoFinish && null == query.Error)
                    {
                        (query as Query<R,C,F>)._isFinished = true;
                    }
                }
            }

            private bool tryGetFromCache()
            {
                processor.log.Debug("try to get query " + query.Code + " from cache");
                lock (processor.Cache)
                {
                    object cachedResult;
                    cachedResult = processor.Cache.Get(query);
                    if (!Missing.Value.Equals(cachedResult))
                    {
                        processor.log.Debug("query " + query.Code + " was retrieved from cache");
                        query.finish((R)cachedResult);
                        query.LoadedFromCache = true;
                        return true;
                    }
                    return false;
                }
            }


        }
        
        }
}