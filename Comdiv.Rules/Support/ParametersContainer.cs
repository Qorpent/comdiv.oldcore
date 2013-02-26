using System;
using System.Collections.Generic;
using Comdiv.Rules;


namespace Comdiv.Rules.Support{
    public class ParametersContainer<T> : Dictionary<string, object>, IParametersProvider<T>{
        #region IParametersProvider<T> Members

        

        public object this[string name, object def]{
            get{
                //@//"name".contract_HasContent(name);
                if (!ContainsKey(name)) return def;
                return this[name];
            }
        }

        public S Get<S>(string name, S def){
            return Get(name, def, false);
        }

        public S Get<S>(string name, S def, bool setup){
            if (setup && !ContainsKey(name)) this[name] = def;
            return (S) this[name, def];
        }

        public S GetParented<S>(string name){
            return GetParented(name, default(S));
        }

        public S GetParented<S>(string name, S def){
            if (!ContainsKey(name)) return (S) resolveParentOrDef(name, def);
            return (S) this[name];
        }

        public S Get<S>(string name){
            return (S) this[name, default(S)];
        }

        public void Add(KeyValuePair<string, object> item){
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item){
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex){
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item){
            throw new NotImplementedException();
        }

        public bool IsReadOnly { get; set; }

        #endregion

        #region ITargetedModule<T> Members

        public T Target { get; set; }

        #endregion

        protected virtual object resolveParentOrDef(string name, object def){
            return def;
        }
    }
}