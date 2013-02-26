using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace Comdiv.Extensions{

    public static class MainExtensions {
        public static bool noContent(this string s) {
            if (string.IsNullOrEmpty(s)) return true;
            if(string.IsNullOrEmpty(s.Trim())) return true;
            return false;
        }
        public static bool hasContent(this string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (string.IsNullOrEmpty(s.Trim())) return false;
            return true;
        }

        public static V get<K,V>(this IDictionary<K,V> dict, K key, V def) {
            if (null == dict) return def;
            if (!dict.ContainsKey(key)) return def;
            return dict[key];
        }

        public static string replace(this string  s, string p, string r) {
            if (null == s) return "";
            return Regex.Replace(s, p, r, RegexOptions.Compiled);
        }

        
        static string[] emptyStrings = new string[]{};
        public static IList<string> split(this string str, bool empty, bool trim, params char[] splitters)
        {
            if (str.noContent())
            {
                return emptyStrings;
            }
            var res = (IEnumerable<string>)str.Split(splitters);
            if (!empty)
            {
                res = res.Where(s => s.hasContent());
            }
            if (trim)
            {
                res = res.Select(s => s.Trim());
            }
            return res.ToArray();
        }
        /// <summary>
        /// Простой шоткат для Regex.Replace в виде расширения
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="substitute"></param>
        /// <returns></returns>
        public static string replace(this string str, string pattern, MatchEvaluator evaluator)
        {
            return replace(str, pattern, evaluator,
                           RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Простой шоткат для Regex.Replace в виде расширения
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="substitute"></param>
        /// <returns></returns>
        public static string replace(this string str, string pattern, MatchEvaluator evaluator, RegexOptions options)
        {
            if (null == str || null == pattern || null == evaluator)
            {
                return String.Empty;
            }
            return Regex.Replace(str, pattern, evaluator, options);
        }
    }

    public static class TagHelper{
        public static string ToString(IDictionary<string,string> tags){
            var result = new StringBuilder();
            foreach (var tag in tags){
                result.Append(" /" + tag.Key + ":" + tag.Value + "/");
            }
            return result.ToString().Trim();
        }

        public static string SetValue(string tags, string  tagname, string  value) {
            var dict = Parse(tags);
            if(null==value) {
                if(dict.ContainsKey(tagname)) {
                    dict.Remove(tagname);
                }
            }else {
                dict[tagname] = value;
            }
            return ToString(dict);
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
            return dict.get(tagname, "");
        }
        public static IDictionary<string ,string > Parse (string tags){
            var result = new Dictionary<string, string>();
            if (tags.noContent()) return result;
            if (tags.Contains("/")){
                tags.replace(@"/([^:/]+):([^:/]+)/", m => {
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