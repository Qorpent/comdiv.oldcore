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


namespace Comdiv.Collections{
    /// <summary>
    /// Карта строк, работает в режиме
    /// игнорирования или использования
    /// регистра
    /// </summary>
    public class StringMap : Map<string, string>{
        private bool ignoreCase = true;

        private bool returnAsUpperCase = true;

        /// <summary>
        /// Признак игнорирования регистра,
        /// по умолчанию - true (игнорируется)
        /// </summary>
        public bool IgnoreCase{
            get { return ignoreCase; }
            set { ignoreCase = value; }
        }

        /// <summary>
        /// Признак возврата значений с общим (верхним) регистром
        /// </summary>
        public bool ReturnAsUpperCase{
            get { return returnAsUpperCase; }
            set { returnAsUpperCase = value; }
        }

        protected override Func<IMapItem<string, string>, bool> GetMainPredicate(string from){
            return m => (!IgnoreCase && (m.From == from)) || (IgnoreCase && (m.From.ToUpper() == from.ToUpper()));
        }

        protected override Func<IMapItem<string, string>, bool> GetReveresePredicate(string to){
            return m => (!IgnoreCase && (m.To == to)) || (IgnoreCase && (m.To.ToUpper() == to.ToUpper()));
        }

        protected override Func<IMapItem<string, string>, string> GetMainConverter(){
            return m => ReturnAsUpperCase ? m.To.ToUpper() : m.To;
        }

        protected override Func<IMapItem<string, string>, string> GetReverseConverter(){
            return m => ReturnAsUpperCase ? m.From.ToUpper() : m.From;
        }

        public string[] ReverseAll(string to){
            var result = new List<string>(this.Where(GetReveresePredicate(to)).Select(GetReverseConverter()).Distinct().ToList());      
            var cnt = 1;
            while (cnt != 0) {
                cnt = 0;
                foreach (var i in result.ToArray()) {
                    var refs = this.Where(GetReveresePredicate(i)).Select(GetReverseConverter()).Distinct().ToArray();
                    foreach (var r in refs) {
                        if(!result.Contains(r)) {
                            result.Add(r);
                            cnt++;
                        }
                    }
                }
            }
            return result.ToArray();

        }

    }
}