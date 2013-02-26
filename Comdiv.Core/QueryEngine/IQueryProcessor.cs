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
using System.Diagnostics;
using Comdiv.Design;

namespace Comdiv.QueryEngine{

    [DebuggerStepThrough]
    [NoCover]
    public static class QueryProcessorShortcutExtensions{
        
        public static R Eval<T,R>(this IQueryProcessor<T,R> engine, T query) where T : IQuery<R>{
            
            engine.Process(query);
            if (query.Error != null) throw query.Error;
            if (!query.IsFinished) throw new Exception("not finished");
            return query.Result;
        }
    }
    public interface IQueryProcessor<T, R>
        where T : IQuery<R>{
        void Process(T query);
        }
}