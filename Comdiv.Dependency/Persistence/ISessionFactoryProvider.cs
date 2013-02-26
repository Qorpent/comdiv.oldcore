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
using System.Data;
using Comdiv.Application;
using Comdiv.Extensions;
using NHibernate;

namespace Comdiv.Persistence{

    public static class SessionFactoryProviderExtensions{
        public static IDbConnection getConnection(this ISessionFactoryProvider provider){
            return getConnection(provider, "");
        }

        public static IDbConnection getConnection(this ISessionFactoryProvider provider,string name){
            var f = provider.Get(name);
            var s = f.GetCurrentSession();
            var t = s.Connection.GetType();
            var cs = s.Connection.ConnectionString;
            var newcon = t.create<IDbConnection>(cs);
            return newcon;
        }


        public static ICriteria getCriteria<T>(this ISessionFactoryProvider provider){
            return getCriteria<T>(provider, "this");
        }

        public static ICriteria getCriteria<T>(this ISessionFactoryProvider provider,string prefix){
            return getCriteria<T>(provider, "", prefix);
        }

        public static ICriteria getCriteria<T>(this ISessionFactoryProvider provider,string name,string prefix) {
            var session = provider.Get(name).GetCurrentSession();
            var realtype = myapp.storage.Get<T>().Resolve();
            return session.CreateCriteria(realtype,prefix);
        }
        public static ICriteria getCriteria(this ISessionFactoryProvider provider, Type type, string prefix)
        {
            return getCriteria(provider,type, "", prefix);
        }

        public static ICriteria getCriteria(this ISessionFactoryProvider provider, Type type, string name, string prefix)
        {
            var session = provider.Get(name).GetCurrentSession();
            var realtype = myapp.storage.Get(type).Resolve(type);
            return session.CreateCriteria(realtype, prefix);
        }
    }

    public interface ISessionFactoryProvider{
        ISessionFactory Get(string id);
        IEnumerable<string> GetIds();
    }
}