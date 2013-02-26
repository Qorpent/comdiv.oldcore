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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Comdiv.Extensions{
    ///<summary>
    ///</summary>
    /// 
    [DebuggerStepThrough]
    public static class DictionaryExtensions{



        /// <summary>
        /// Обертка над apply чтобы сразу создавать объекты
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static X toObject<X>(this IDictionary<string, string> dict) where X : class,new()
        {
            return dict.apply(new X());
        }

      
        /// <summary>
        /// Применяет значения из словаря в качестве свойств целевого объекта
        /// </summary>
        /// <typeparam name="T">тип значения словаря, для того чтобы и string->string, и string->object</typeparam>
        /// <typeparam name="X">тип целевого класса для floint стиля вызова</typeparam>
        /// <param name="dict">словарь из которого брать</param>
        /// <param name="target">целевой объект</param>
        public static X apply<T, X>(this IDictionary<string, T> dict, X target) where X : class
        {
            if (dict == null || dict.Count == 0 || target == null)
            {
                return target;
            }

            foreach (var item in dict)
            {
                string name = item.Key;
                T value = item.Value;
                Type type = target.GetType();
                PropertyInfo property = type.GetProperty(name,
                                                         BindingFlags.IgnoreCase | BindingFlags.Instance |
                                                         BindingFlags.Public |
                                                         BindingFlags.SetProperty);
                if (property == null || property.GetSetMethod() == null)
                {
                    continue;
                }

                object normalvalue = value.to(property.PropertyType);
                property.SetValue(target, normalvalue, null);
            }
            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T get<T>(this IDictionary<string, T> dict, string key, T defobj = default(T))
        {
            return (((IDictionary)dict).get(key, null, true, defobj)).to<T>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T get<T>(this IDictionary<string, object> dict, string key)
        {
            return (((IDictionary)dict).get(key, null, true, default(T))).to<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T get<T>(this IDictionary<string, object> dict, string key, T def = default(T))
        {
            return (((IDictionary)dict).get(key, null, true, def)).to<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static string get(this IDictionary<string, string> dict, string key, string defaultvalue)
        {
            return (string)((IDictionary)dict).get(key, null, true, defaultvalue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="init"></param>
        /// <param name="defobj"></param>
        /// <returns></returns>
        public static R get<R, K, V>(this IDictionary<K, V> dict, K key = default(K), Func<R> def = null, bool init = true, R defobj = default(R))
        {
            def = def ?? (() => default(R));
			if(null==key) return def();
            //HACK: all descendants of generic Dictionary are IDictionary too
            return get((IDictionary)dict, key, () => (object)def(), init, defobj, typeof(V)).to<R>();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="dict">словарь из которого берем значения</param>
        /// <param name="key">ключ по которому значение ищем</param>
        /// <param name="def"></param>
        /// <param name="init"></param>
        /// <param name="defobj"></param>
        /// <returns></returns>
        public static object get(this IDictionary dict, object key = null, Func<object> def = null, bool init = true, object defobj = null, Type outtype = null)
        {
            if (null == def) def = () => defobj;
            if (null == dict || null == key) return def();
            if (dict.Contains(key)) return dict[key];
            object res = def();
            if (init && null != res) dict[key] = res.to(outtype ?? typeof(object));
            return res;
        }        
    }
}