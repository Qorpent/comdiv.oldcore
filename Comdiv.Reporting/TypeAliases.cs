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
using Comdiv.Extensions;

namespace Comdiv.MVC
{
    public static class TypeAliases
    {
        public static IDictionary<string, Type> Registry = new Dictionary<string, Type>
                                                               {
                                                                   {"",typeof(string)},
//                                                                   {null,typeof(string)},
                                                                   {"int",typeof(int)},
                                                                   {"num",typeof(decimal)},
                                                                   {"bool",typeof(bool)},
                                                                   {"date",typeof(DateTime)},
                                                                   {"str",typeof(string)},
                                                                   {"string",typeof(string)},
																   {"file",typeof(string)}
                                                               };
        public static Type Get(string alias)
        {
            
            return Registry.get((alias??"").ToLower(),()=>alias.toType());
        }

        public static string Get(Type type)
        {
            foreach (var registry in Registry)
            {
                if(registry.Value==type) return registry.Key;
            }
            return type.Name;

        }
    }
}