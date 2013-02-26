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

namespace Comdiv.Collections{
    public class Pool<T> : IPool<T>{
        internal Stack<T> stack = new Stack<T>();

        public T Get(Func<T> ctor){
            return Get(ctor, null);
        }

        public T Get(Func<T> ctor,Action<T> prepare){
            lock (stack){
                T result = default(T);
                if (stack.Count != 0)
                {
                    result = stack.Pop();
                }
                else{
                    result = ctor();
                }
                if(prepare!=null){
                    prepare(result);
                }
                return result;
            }


        }
        public void Return(T obj){
            lock(stack) stack.Push(obj);
            
        }

        public void Clear(){
            lock (stack)stack.Clear();
        }
    }
}