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

namespace Comdiv.Model.Lookup{
    public class LookupItemComparer : IComparer<ILookupItem>{
        #region IComparer<ILookupItem> Members

        public int Compare(ILookupItem x, ILookupItem y){
            if (x.Category != y.Category){
                if (x.Category.noContent()){
                    return 1;
                }
                if (y.Category.noContent()){
                    return -1;
                }
                return x.Category.CompareTo(y.Category);
            }
            if (x.Idx != y.Idx){
                return x.Idx.CompareTo(y.Idx);
            }
            return x.Name.CompareTo(y.Name);
        }

        #endregion
    }

    /// <summary>
    /// Простая реализация ILookupItem
    /// </summary>
    public class LookupItem : ILookupItem{
        private IDictionary<string, object> properties;

        #region ILookupItem Members

        public string Alias { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public IDictionary<string, object> Properties{
            get { return properties ?? (properties = new Dictionary<string, object>()); }
        }

        public string Category { get; set; }

        public int Idx { get; set; }

        #endregion

        /// <summary>
        /// Утилитный метод для нужд обработки строк и передачи параметров - сворачивает все
        /// основные и дополнительные свойства в один словарь
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> AsDictionary(){
            var result = new Dictionary<string, object>(Properties);
            result["Name"] = Name;
            result["Code"] = Code;
            result["Alias"] = Alias;
            return result;
        }
    }
}