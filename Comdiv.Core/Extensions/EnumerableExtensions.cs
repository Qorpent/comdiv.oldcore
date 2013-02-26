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
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Comdiv.Extensions;


namespace Comdiv.Extensions
{
    ///<summary>
    ///</summary>
    public static  class EnumerableExtensions{
        public static bool isIn(this object obj, params object[] objs){
            return objs.Contains(obj);
        }

        public static T add<T,R>(this T list, params R[] objects ) where T:IList<R>{
            foreach (var o in objects){
                list.Add(o);
            }
            return list;
        }

        public static IEnumerable<T> range<T>(this T from, T to){
            if(typeof(T)==typeof(int)){

                if (from.toInt() < to.toInt()){
                    for (int i = (int) (object) from; i <= (int) (object) to; i++){
                        yield return (T) (object) i;
                    }
                }else{
                    for (int i = (int)(object)from; i >= (int)(object)to; i--)
                    {
                        yield return (T)(object)i;
                    }
                }
            }else{
                throw new ArgumentException("T", "пока метод range поддерживается только для int");
            }
        }


        /// <summary>
        /// Applyes <paramref name="expression"/> for each element in set
        /// </summary>
        /// <typeparam name="T">type of collection </typeparam>
        /// <param name="set">collection</param>
        /// <param name="expression">delegate to execute</param>
        public static IEnumerable<T> map<T>(this IEnumerable<T> set, Action<T> expression)
        {
            return map(set, expression, null);
        }

        /// <summary>
        /// Applyes <paramref name="expression"/> for each element in set and controll exceptions
        /// </summary>
        /// <typeparam name="T">type of collection </typeparam>
        /// <param name="set">collection</param>
        /// <param name="expression">delegate to execute</param>
        /// <param name="exceptionHandler">действие при исключении</param>
        public static IEnumerable<T> map<T>(this IEnumerable<T> set, Action<T> expression, Action<T,int,Exception> exceptionHandler)
        {
            if(null==set||null==expression) return set;
            var idx = 0;
            foreach (var t in set)
            {
                try
                {
                    expression(t);
                    idx++;
                }
                catch (Exception ex)
                {
                    if (exceptionHandler.yes()) exceptionHandler(t,idx,ex);
                    else throw;
                }
            }
            return set;
        }

        public static IEnumerable<T> copy<T>(this IEnumerable<T> source)
        {
            return new List<T>(source);
        }

        public static T addRange<T, I>(this T collection, IEnumerable<I> range) where T : IList<I>
        {
            foreach (var t in range)
            {
                collection.Add(t);
            }
            return collection;
        }

        public static bool containsAll<T>(this IEnumerable<T> source, IEnumerable<T> values){
            if (source == null || values == null) return false;

            foreach (var t in values)
            {
                if (!source.Contains(t)) return false;
            }
            return true;
        }

        public static IList<I> ensureInstance<I, C>(this IList<I> list, Func<C> retriever) where C : I
        {
            if (null == list.FirstOrDefault(x => x is C)){
                list.Add(retriever());
            }
            return list;
        }
    }
}