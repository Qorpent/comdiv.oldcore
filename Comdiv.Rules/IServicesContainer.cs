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


namespace Comdiv.Rules{
    public interface IServicesContainer{
        object this[string name] { get; }
        object this[Type type] { get; }

        IServicesContainer ParentContainer { get; set; }
        void RegisterService(string name, object servive);

        void RegisterService(object servive);
        void RegisterService(Type type, object service);
        void UnRegisterService(object service);
        T Get<T>();

        void RegisterService<S>(S servive)
            where S : class;

        void RegisterService<S>(string name, S service)
            where S : class;
    }
}