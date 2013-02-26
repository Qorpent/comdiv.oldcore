using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Scaffolding{
    public class DataHelper{
        private StorageWrapper<object> storage;

        public DataHelper(){
            this.storage = myapp.storage.GetDefault();
        }
        //public object Resolve(object obj, string expr, Object ioc.get){
        //    if(expr.StartsWith("hql:")){
        //        var hql = expr.Substring(4);
        //        if(ioc.get is IEnumerable){

        //            hql = string.Format(hql, (object[]) ((IEnumerable)ioc.get).OfType<object>().ToArray());
        //        }
        //        return ioc.get<IHqlEvaluator>().Execute(hql);
        //    }
        //    var interpreter = (Interpreter) ioc.get;
        //    var res = TryResolveWithoutInterpretation(obj, expr);
        //    if (Missing.Value == res) return interpreter.Eval(obj, expr);
        //    return res;
        //}

        //public IList<ICommonDictionary> Dictionaries(){
        //    return myapp.storage.All<ICommonDictionary>().ToList();
        //}
        public IEnumerable<object> Query(Type type, string query, params object[] advancedParameters){
            return storage.Query(type, query, advancedParameters).Cast<object>();
        }

        public IEnumerable<object> Query(string typeRef, string query, params object[] advancedParameters){
            return storage.Query(Type.GetType(typeRef), query, advancedParameters).Cast<object>();
        }

        //public IList<EmptyEntity> Classes(){
        //    return ScaffoldLookupSource.GetLookup(typeof (ICls), null,new[] {Order.Asc("Code")}, true);
        //}

        //public IList<EmptyEntity> PropertyTypes(){
        //    return ScaffoldLookupSource.GetLookup(typeof (IClsPropertyType), null,new[] {Order.Asc("Code")}, true);
        //}

        //protected virtual object TryResolveWithoutInterpretation(object obj, string expr){
        //    switch (expr){
        //        case "Id":
        //            return ((IWithId) obj).Id;
        //        case "Code":
        //            return ((IWithCode) obj).Code;
        //        case "Name":
        //            return ((IWithName) obj).Name;
        //        case "Comment":
        //            return ((IWithComment) obj).Comment;
        //        case "Idx":
        //            return ((IWithIdx) obj).Idx;
        //    }
        //    return Missing.Value;
        //}
    }
}