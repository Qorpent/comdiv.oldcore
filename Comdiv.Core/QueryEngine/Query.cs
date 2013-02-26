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
using System.IO;
using Comdiv.Common;
using Comdiv.Extensions;

namespace Comdiv.QueryEngine{
    [DebuggerStepThrough]
    public abstract class Query<R, C, F> : IContextualQuery<R, C, F>
        where C : IWithProperties
        where F : class,IContextualQuery<R, C, F>, new(){
        private static int _id;
        private IList<F> _children = new List<F>();
        private string _code;
        private int childcount;
        protected Query(){
            Properties = new Dictionary<string, object>();
            Code = (++_id).ToString();
        }

        #region IContextualQuery<R,C,F> Members

        ///<summary>
        ///</summary>
        public string Code{
            get{
                if (!_code.hasContent()){
                    _code = (++_id).ToString();
                }
                return _code;
            }
            set { _code = value; }
        }
        public bool LoadedFromCache { get; set; }
        public IDictionary<string, object> Properties { get; private set; }
        public R Result { get; set; }
        internal bool _isFinished;
        public bool IsFinished{
            get { return _isFinished; }
            set { _isFinished = value; }
        }

        public Exception Error { get; set; }
        public C Context { get; set; }
        public int Level { get; set; }

        public virtual F Clone(){
            var result = new F();
            result.Level = Level + 1;
            Children.Add(result);
            result.Parent = this as F;
            foreach (var property in Properties){
                result.Properties[property.Key] = property.Value;
            }
            result.Context = Context;
            result.Code = Code + "-" + (++childcount);
            return result;
        }

        public F Parent { get; set; }

        public IList<F> Children{
            get { return _children; }
            set { throw new NotSupportedException("provided just for interface compatibility"); }
        }

        public bool IsCacheAble { get; set; }

        public string GetCacheKey(){
            return Properties.get("cacheKey", () => ToString().toMD5(),false);
        }

        public Func<bool> GetLeaseChecker(){
            return Properties.get("__cacheLease", () => new Func<bool>(() => true),false);
        }

        #endregion

        public override string ToString(){
            var wr = new StringWriter();
            preToString(wr);
            usualToString(wr);
            postToString(wr);
            return wr.ToString();
        }

        public override bool Equals(object obj){
            if (base.Equals(obj)){
                return true;
            }
            if (!(obj is Query<R, C, F>)){
                return false;
            }
            return ((Query<R, C, F>) obj).GetCacheKey().Equals(GetCacheKey());
        }

        protected virtual void postToString(TextWriter writer) {}

        protected virtual void usualToString(TextWriter writer){
            var indent = String.Empty;
            Action ind = () => indent += "\t";
            Action<string> wl = s =>{
                                    writer.Write(indent);
                                    writer.WriteLine(s);
                                };
            wl("query (" + GetType().Name + "):");
            ind();
            wl("code: " + (Code ?? "NOCODE"));
            wl("properties: ");
            ind();
            foreach (var o in Properties){
                //in tostring we return just "public" properties, 
                // _*** means as private, temporaty, system
                if (o.Key.StartsWith("_")){
                    continue;
                }
                var val = o.Value ?? "NULL";
                if(!(val is string) && val is IEnumerable){
                    val = ((IEnumerable) val).concat(";");
                }
                wl(o.Key + ": " + val);
            }
        }

        protected virtual void preToString(TextWriter writer) {}
        }
}