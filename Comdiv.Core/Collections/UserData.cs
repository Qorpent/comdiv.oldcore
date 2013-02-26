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
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;

namespace Comdiv.Collections{

    public class CurrentError {
        IDictionary<string, IDictionary<int, Handler<Exception>>> cache = new Dictionary<string , IDictionary<int, Handler<Exception>>>();

        private Dictionary<int, Handler<Exception>> _errors()
        {
            return cache.get(myapp.usrName, () => new Dictionary<int, Handler<Exception>>());
        }

        public int RegisterError(Exception ex)
        {
            lock (this)
            {
                var h = new Handler<Exception>(ex);
                _errors()[h.Id] = h;
                return h.Id;
            }
        }

        public CurrentError RemoveError()
        {
            lock (this)
            {
                return RemoveError(0);

            }
        }

        public CurrentError RemoveError(int idx)
        {
            lock (this)
            {
                if (0 == idx)
                {
                    _errors().Clear();
                }
                else
                {
                    _errors().Remove(idx);
                }
                return this;
            }
        }



        public IEnumerable<Handler<Exception>> Errors
        {
            get
            {
                lock (this)
                {
                    return _errors().Values.ToArray();
                }
            }
        }

    }

    [Obsolete("Надо пользоваться только профилями")]
    public class UserData{
        IDictionary<string ,IDictionary<string ,object >> data = new Dictionary<string, IDictionary<string, object>>();

        [Overload]
        public IDictionary<string ,object > GetData(){
            return GetData(myapp.usrName);
        }

        

        public IEnumerable<Handler<Exception>> Errors{
            get{
                lock(this){
                    return _errors().Values.ToArray();
                }
            }
        }

        public void DoAutoSave(){
            if(AutoSave){
                Save(AutoSaveFile);
            }
        }

        public bool AutoSave { get; set; }

        public string AutoSaveFile { get; set; }

        private Dictionary<int, Handler<Exception>> _errors(){
            return this.Get("exceptions", () => new Dictionary<int, Handler<Exception>>());
        }

        public int RegisterError(Exception ex){
            lock(this){
                var h = new Handler<Exception>(ex);
                _errors()[h.Id] = h;
                return h.Id;
            }
        }

        public UserData RemoveError(){
            lock (this){
                return RemoveError(0);

            }
        }

        public UserData RemoveError(int idx){
            lock (this){
                if (0 == idx){
                    _errors().Clear();
                }else{
                    _errors().Remove(idx);
                }
                return this;
            }
        }

        [Worker]
        public IDictionary<string ,object > GetData(string usr){
            lock (this){
                if (!data.ContainsKey(usr)){
                    data[usr] = new Dictionary<string, object>();
                }
                return data[usr];
            }
        }
        public void Clear(){
            data.Clear();
        }
        [Overload]
        public T Get<T>(string name){
            return Get<T>(myapp.usrName, name);
        }
        [Overload]
        public T Get<T>(string user,string name){
            return Get(user, name, default(T));
        }
        [Overload]
        public T Get<T>(string name,T def){
            return Get(myapp.usrName, name, def);
        }
        [Overload]
        public T Get<T>(string user,string name,T def){
            return Get(user, name, Equals(def,default(T))?(Func<T>)null:() => def);
        }
        [Overload]
        public T Get<T>(string name, Func<T> def){
            return Get(myapp.usrName, name, def);
        }
        [Worker]
        public T Get<T>(string user, string name, Func<T> def)
        {
            Func<T> def_ = def ?? (() => default(T));
            return GetData(user).get(name,()=>def_(), null!=def);
        }
        [Overload]
        public UserData Set(string name, object value){
            lock (this){
                return Set(myapp.usrName, name, value);    
            }
            
        }

        public UserData Set(string user, string name, object value)
        {
            lock (this){
                GetData(user)[name] = value;
                return this;
            }
        }

            public void Load(string filename){
            var file = myapp.files.Resolve(filename, true);
            if(null==file) return;
            var root = XElement.Load(file);
            foreach (var element in root.Elements()){
                var d = this.GetData(element.Attribute("usr").Value);
                var t = Type.GetType(element.Attribute("type").Value);
                d[element.Attribute("name").Value] = element.Value.to(t);
            }
        }

        
        public void Save(string filename){
            var file = myapp.files.Resolve(filename, false);
            var x = new XElement("usrdata");
            foreach (var d in data){
                var usr = d.Key;
                foreach (var val in d.Value){
                    if(null==val.Value) continue;
                    if(new[]{typeof(string),typeof(int),typeof(bool),typeof(DateTime),typeof(decimal)}.Contains(val.Value.GetType())){
                          x.Add(new XElement("item",
                                new XAttribute("usr", usr),
                                new XAttribute("type",val.Value.GetType().FullName),
                                new XAttribute("name",val.Key),
                                
                                new XText(val.Value.ToString())
                                ));                      
                    }
                }
            }
            x.Save(file);
        }
    }
}