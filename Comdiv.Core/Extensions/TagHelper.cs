using System.Collections.Generic;
using System.Text;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Extensions{
    public static class TagHelper{
        public static string ToString(IDictionary<string,string> tags){
            var result = new StringBuilder();
            foreach (var tag in tags){
                result.Append(" /" + tag.Key + ":" + tag.Value + "/");
            }
            return result.ToString().Trim();
        }
        public static string RemoveTag(string tags, string tag)
        {

            var trg = Parse(tags);
            if (trg.ContainsKey(tag))
            {
                trg.Remove(tag);
            }
            return ToString(trg);
        }
        public static string SetValue(string tags, string tagname, string value)
        {
            var dict = Parse(tags);
            if (null == value)
            {
                if (dict.ContainsKey(tagname))
                {
                    dict.Remove(tagname);
                }
            }
            else
            {
                dict[tagname] = value;
            }
            return ToString(dict);
        }
		public static string Merge(string target, string source) {
			return ToString(Merge(Parse(target), Parse(source)));
		}
        public static IDictionary<string, string> Merge(IDictionary<string, string> target, IDictionary<string, string> source)
        {
            foreach (var val in source){
                if(val.Value==null || val.Value=="DELETE"){
                    target.Remove(val.Key);
                }
                else target[val.Key] = val.Value;
            }
            return target;
        }

        public static bool Has(string tags, string tagname) {
            var dict = Parse(tags);
            return dict.ContainsKey(tagname);
        }
        
        public static string Value (string tags, string tagname){
            var dict = Parse(tags);
            return dict.get(tagname, "").Replace("~","/");
        }
        public static IDictionary<string ,string > Parse (string tags){
            var result = new Dictionary<string, string>();
            if (tags.noContent()) return result;
            if (tags.Contains("/")){
                tags.replace(@"/([^:/]+):([^:/]*)/", m => {
                                                   result[m.Groups[1].Value] = m.Groups[2].Value;
                                                   return m.Value;
                                               });
            }else{
                foreach (var s in tags.split(false, true, ' '))
                {
                    if (s.Contains(":"))
                    {
                        result[s.Split(':')[0]] = s.Split(':')[1];
                    }
                    else
                    {
                        result[s] = "";
                    }
                }
            }
            return result;
        }
    }
}