using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Dictionaries;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using NHibernate.Criterion;

namespace Comdiv.Model.Lookup{
    internal class CommonDictionaryLookupSource : BaseLookupSource{

        public CommonDictionaryLookupSource(){
            this.storage = myapp.storage.Get<ICommonDictionary>();
        }

        private ICommonDictionary parentDict;
        private StorageWrapper<ICommonDictionary> storage;

        public ICommonDictionary ParentDict{
            get{
                if (parentDict == null){
                    var realCode = Alias.Split('_')[1];
                    parentDict = storage.Load(realCode);
                }
                return parentDict;
            }

            set { parentDict = value; }
        }

        public override IEnumerable<ILookupItem> Select(ILookupQuery query){
            var crit = DetachedCriteria.For(typeof (ICommonDictionaryValue));
            if (query.First){
                crit.SetMaxResults(1);
            }
            crit.Add(Restrictions.Eq("Dictionary", ParentDict));
            if (query.Code.hasContent()){
                if (query.CodeMask){
                    crit.Add(Restrictions.Like("Code", query.Code));
                }
                else{
                    crit.Add(Restrictions.Eq("Code", query.Code));
                }
            }

            if (query.Name.hasContent()){
                if (query.NameMask){
                    crit.Add(Restrictions.Like("Name", query.Name));
                }
                else{
                    crit.Add(Restrictions.Eq("Name", query.Name));
                }
            }
            var result = new List<ILookupItem>();
            var res = storage.Query<ICommonDictionaryValue>(crit);
            foreach (var value in res){
                var item = new LookupItem{Alias = query.Alias, Code = value.Code, Name = value.Name};
                item.Properties["Comment"] = value.Comment;
                item.Properties["Id"] = value.Id;
                item.Properties["Idx"] = value.Idx;
                item.Properties["Value"] = value.Value;
                result.Add(item);
            }
            return result;
        }
    }
}