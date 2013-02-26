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
using System.Collections.Generic;
using System.Linq;
using Comdiv;
using Comdiv.Application;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.Inversion{
    ///<summary>
    /// Вспомогательный класс, связывающий некий целевой объект с набором расширений из ioc
    /// используется интерфейс ICanCheckMatching,  и конкретный запрос класса объекта,
    /// для расширений нацеленных на конкретные варианты использования
    ///</summary>
    ///<typeparam name="TMatch"></typeparam>
    public class MatchedExtensionsSet<TMatch>
    {
        public MatchedExtensionsSet(TMatch obj)
        {
            this.Target = obj;
        }

        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        private TMatch Target { get; set; }

        public TTarget first<TTarget>() 
        {
            return all<TTarget>().FirstOrDefault();
        }

        public IEnumerable<TTarget> all<TTarget>() 
        {
            return
                Container.all<TTarget>().Where(m =>
                                         (m is ICanCheckMatching<TMatch> &&  ((ICanCheckMatching<TMatch>)m).IsMatch(Target))
                                         ||
                                         (!(m is ICanCheckMatching<TMatch>))).OrderByDescending(m => 
                                                                                                (m is IWithIdx)? ((IWithIdx)m).Idx : 0);
        }
    }
}