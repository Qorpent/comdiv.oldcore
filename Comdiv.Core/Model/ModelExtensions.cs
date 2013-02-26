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
using Comdiv.Model;
using Comdiv.Model.Interfaces;


namespace Comdiv.Model{
    public static class ModelExtensions
    {
        public static int Id(this object obj){
            if (null == obj) return 0;
            if(obj is int) return (int)obj;
            if (obj is string) return obj.toInt();
            if (typeof(IWithId).IsAssignableFrom(obj.GetType())) return  ((IWithId) obj).Id;
            return obj.getPropertySafe<int>("Id");
        }
       
        public static string Code(this object obj){
            if (null == obj) return "";
            if (obj is string) return (string) obj;
            if (obj is IWithCode) return ((IWithCode) obj).Code;
            return obj.getPropertySafe<string>("Code");
        }
        public static int Idx(this object obj)
        {
            if (null == obj) return 0;
            if (obj is IWithIdx) return ((IWithIdx)obj).Idx;
            return obj.getPropertySafe<int>("Idx");
        }

        public static T setCode<T>(this T obj,string code)where T:class{
            if (null == obj) return null;
            if (obj is IWithCode){
                ((IWithCode) obj).Code = code;
            }
            return obj;
        }

        public static string Name(this object obj, string def = null){
            if (null == obj) return def;
            if (obj is IWithName) return ((IWithName) obj).Name;
            var prop = obj.getPropertySafe<string>("Name");
            return prop ?? def;
        }

        public static T setName<T>(this T obj, string name) where T : class
        {
            if (null == obj) return null;
            if (obj is IWithName)
            {
                ((IWithName)obj).Name = name;
            }
            return obj;
        }

    }
}