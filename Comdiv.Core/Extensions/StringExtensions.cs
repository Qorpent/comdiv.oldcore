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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace Comdiv.Extensions{
    /// <summary>
    /// provides
    /// a) null-safe version for often-used string methods
    /// b) formating with object/dict source in ${NAME} notation
    /// c) more advanced string concatenation and spliting to list
    /// </summary>
    
    public static class StringExtensions{

        const string fiopattern2 = @"^(?ix) 
                ( (?<f>\w+)\s+(?<i>\w+)\s+(?<o>\w+) )
                |
                ((?<i>\w+)\.\s*(?<o>\w+)\.\s+(?<f>\w+))
                |
                ((?<f>\w+)\s+(?<i>\w+)\.\s*(?<o>\w+)\.)$";
        public static string tofio(string name, bool first) {
            name = name.Trim();
            var m = Regex.Match(name, fiopattern2);
            if(m.Success) {
                if(first) {
                    return string.Format("{1}.{2}. {0}", m.Groups["f"].Value, m.Groups["i"].Value[0],
                                         m.Groups["o"].Value[0]);
                }else {
                    return string.Format("{0} {1}.{2}.", m.Groups["f"].Value, m.Groups["i"].Value[0],
                                         m.Groups["o"].Value[0]);
                }
            }else {
                return name;
            }
        }


        private static readonly string[] emptyStrings = new string[] { };
        private const string fmtDictRegex = @"\$\{(?<name>[^\:\}]+)(?<fmt>:[^}]+)?\}";
        private const string fmtDictRegexFmt = @"fmt";
        private const string fmtDictRegexName = @"name";

        public static string toCmdLineString(this string str) {
            if(str.like(@"[\s""]")) {
                return "\"" + str.Replace("\"", "\"\"") + "\"";
            }
            return str;
        }


        public static string splittolines(this string txt, int maxlength)
        {
            var strings = txt.split(false,true,' ');
            var result = "";
            var currentstring = "";
            foreach (var s in strings)
            {
                if (currentstring.noContent())
                {
                    currentstring = s;
                    continue;
                }
                if((currentstring+" "+s).Length>maxlength)
                {
                    result += "\r\n" + currentstring;
                    currentstring = s;
                    continue;
                }
                currentstring += " " + s;
            }
            result += "\r\n" + currentstring;
            return result.Trim();
        }

        /// <summary>
        /// checks string for not-null, not-empty and not-ws-only
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true  - string is not null, empty or white-space only</returns>
        public static bool hasContent(this string str){
            if (String.IsNullOrEmpty(str)){
                return false;
            }
            return 0 < str.Trim().Length;
        }

        public static bool isLiteral(this string str){
            if (str.noContent()) return false;
            return str.Trim().like(@"^(([^\W\d][_\w\d]*)|([^\w\D][\w\d_]+))$");
        }

        private const string fiopattern = @"^(\w+)\s+(\w+)\s+(\w+)$";

        public static string tofio(this string name)
        {
            if (name.noContent()) return "";
            if (name.like(fiopattern))
            {
                return name.replace(fiopattern, m => "{0} {1}.{2}."._format(m.Groups[1].Value,
                                                                            m.Groups[2].Value.Substring(0, 1).ToUpper(),
                                                                            m.Groups[3].Value.Substring(0, 1).ToUpper()));
            }
            return name;
        }

        /// <summary>
        /// checks that string is logical empty - null, empty or ws-only
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if str is empty</returns>
        public static bool noContent(this string str){
            return !str.hasContent();
        }

        /// <summary>
        /// null-safe version of string.Length
        /// </summary>
        public static int _length(this string str){
            if (null == str){
                return 0;
            }
            return str.Length;
        }

        /// <summary>
        /// null-safe version string.StartsWith
        /// </summary>
        /// <param name="str">string to find in</param>
        /// <param name="strToFind">string to find</param>
        /// <returns>
        ///     (str == null || strToFind == null) ? False : str.StartsWith(strToFind)
        /// </returns>
        public static bool _starts(this string str, string strToFind){
            if (String.IsNullOrEmpty(str)){
                return false;
            }
            if (String.IsNullOrEmpty(strToFind)){
                return false;
            }
            return str.StartsWith(strToFind, true, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// null-safe version of string.Contains
        /// </summary>
        /// <param name="str">target string</param>
        /// <param name="strToFind">string to find</param>
        /// <returns>
        ///     (str == null || strToFind == null) ? False : str.Contains(strToFind),
        /// </returns>
        public static bool _contains(this string str, string strToFind){
            if (String.IsNullOrEmpty(str)){
                return false;
            }
            if (String.IsNullOrEmpty(strToFind)){
                return false;
            }
            return str.ToUpperInvariant().Contains(strToFind.ToUpperInvariant());
        }

        /// <summary>
        /// => concat(enumerable,null,null)
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static string concat(this IEnumerable enumerable){
            return concat(enumerable, null);
        }

        /// <summary>
        /// => concat(enumerable,joiner,null)
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="joiner"></param>
        /// <returns></returns>
        public static string concat(this IEnumerable enumerable, string joiner){
            return concat(enumerable, joiner, null);
        }

        /// <summary>
        /// concatenate all items in enumerables with joiner
        /// and creates single string, if nullreplacer is setted up
        /// all nulls in collection replaced with this one
        /// </summary>
        /// <param name="enumerable">an enumerable</param>
        /// <param name="joiner">element joiner</param>
        /// <param name="nullreplacer">string that represents nulls</param>
        /// <returns>
        /// String.Empty - if enumerable is null, otherwise string concatenation
        /// </returns>
        public static string concat(this IEnumerable enumerable, string joiner, string nullreplacer){
            if (null == enumerable){
                return String.Empty;
            }
            var result = new StringBuilder();
            joiner = joiner ?? String.Empty;
            var addjoiner = joiner != String.Empty;
            var first = true;
            var iter = enumerable.GetEnumerator();
            while (iter.MoveNext()){
                var current = iter.Current;
                string str = null;
                if (null != current){
                    if (current is string){
                        str = (string) current;
                    }
                    else{
                        str = current.ToString();
                    }
                }
                if (null == str){
                    str = nullreplacer;
                }
                if (null != str){
                    if (addjoiner && !first){
                        result.Append(joiner);
                    }
                    result.Append(str);
                }
                first = false;
            }

            return result.ToString();
        }

        /// <summary>
        /// Shortcut ConvertTo System.Text.Regex.IsMatch(str,pattern,RegexOptions.Compiled|RegexOptions.IgnoreCase|RegexOptions.IgnorePatternWhitespace)
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="pattern">regex</param>
        /// <returns>returns True if <paramref name="pattern"/> succesfully finded in <paramref name="str"/></returns>
        public static bool like(this string str, string pattern){
            return like(str, pattern,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Shortcut ConvertTo System.Text.Regex.IsMatch(str,pattern,RegexOptions.Compiled)
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="pattern">regex</param>
        /// <returns>returns True if <paramref name="pattern"/> succesfully finded in <paramref name="str"/></returns>
        public static bool like(this string str, string pattern, RegexOptions options){
            if (String.IsNullOrEmpty(str) || String.IsNullOrEmpty(pattern)){
                return false;
            }
            return Regex.IsMatch(str, pattern, options);
        }


        /// <summary>
        /// Замена по словарю, синтаксис {KEY},{KEY:FORMAT}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="subst"></param>
        /// <returns></returns>
        public static string _format(this string str, params object[] subst){
            if (null == str || null == subst){
                return String.Empty;
            }
            return String.Format(str, subst);
        }


        /// <summary>
        /// Замена по словарю, синтаксис {KEY},{KEY:FORMAT}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="subst"></param>
        /// <returns></returns>
        public static string _formatex(this string str, object subst){
            return str._formatex(subst, String.Empty);
        }


        public static string _formatex(this string str, object subst, string nullSubstitute){
            if (null == str || null == subst){
                return String.Empty;
            }
            if (subst is object[]){
                return String.Format(str, (object[]) subst);
            }
            if (null == nullSubstitute){
                nullSubstitute = String.Empty;
            }
            Func<string, bool> _exists = n =>{
                                             if (subst is IDictionary){
                                                 return ((IDictionary) subst).Keys.Cast<object>().Contains(n);
                                             }
                                             else{
                                                 return null != subst.GetType().GetProperty(n);
                                             }
                                         };
            Func<string, object> _get = n =>{
                                            if (!_exists(n)){
                                                return null;
                                            }
                                            if (subst is IDictionary){
                                                return ((IDictionary) subst)[n];
                                            }
                                            else{
                                                return subst.GetType().GetProperty(n).GetValue(subst, null);
                                            }
                                        };
            return str.replace(fmtDictRegex,
                               m =>{
                                   var name = m.Groups[fmtDictRegexName].Value;
                                   var fmt = m.Groups[fmtDictRegexFmt].Value;
                                   var value = String.Empty;
                                   var result = String.Empty;
                                   var obj = _get(name);
                                   if (null == obj){
                                       value = nullSubstitute;
                                   }

                                   else{
                                       if (obj is DateTime){
                                           value = ((DateTime) obj).ToString(fmt.Replace(":", String.Empty),
                                                                             CultureInfo.InvariantCulture);
                                           fmt = String.Empty;
                                       }
                                       else if (obj is decimal){
                                           value = ((decimal) obj).ToString(fmt.Replace(":", String.Empty),
                                                                            CultureInfo.InvariantCulture);
                                           fmt = String.Empty;
                                       }
                                       else if (obj is double){
                                           value = ((double) obj).ToString(fmt.Replace(":", String.Empty),
                                                                           CultureInfo.InvariantCulture);
                                           fmt = String.Empty;
                                       }
                                       else if (obj is int){
                                           value = ((int) obj).ToString(fmt.Replace(":", String.Empty),
                                                                        CultureInfo.InvariantCulture);
                                           fmt = String.Empty;
                                       }
                                       else{
                                           value = obj.ToString();
                                       }
                                   }
                                   var pattern = "{0" + fmt + "}";
                                   result = String.Format(pattern, value);
                                   return result;
                               },
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Простой шоткат для Regex.Replace в виде расширения
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="substitute"></param>
        /// <returns></returns>
        public static string replace(this string str, string pattern, string substitute){
            return replace(str, pattern, substitute,
                           RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Простой шоткат для Regex.Replace в виде расширения
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="substitute"></param>
        /// <returns></returns>
        public static string replace(this string str, string pattern, string substitute, RegexOptions options){
            if (null == str || null == pattern || null == substitute){
                return String.Empty;
            }
            return Regex.Replace(str, pattern, substitute, options);
        }

        /// <summary>
        /// Простой шоткат для Regex.Replace в виде расширения
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="substitute"></param>
        /// <returns></returns>
        public static string replace(this string str, string pattern, MatchEvaluator evaluator){
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
        public static string replace(this string str, string pattern, MatchEvaluator evaluator, RegexOptions options){
            if (null == str || null == pattern || null == evaluator){
                return String.Empty;
            }
            return Regex.Replace(str, pattern, evaluator, options);
        }

        /// <summary>
        /// Оболочка над Regex.Match с возвратом найденного значения, игнорируется регистр и пробельные символы
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string find(this string str, string pattern){
            return find(str, pattern,
                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Оболочка над Regex.Match с возвратом найденного значения, игнорируется регистр и пробельные символы
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string find(this string str, string pattern, RegexOptions options){
            if (null == str || null == pattern){
                return String.Empty;
            }
            var m = Regex.Match(str, pattern, options);
            if (m.Success){
                return m.Value;
            }
            return String.Empty;
        }


        public static string find(this string str, string pattern, string groupname){
            return find(str, pattern, groupname, RegexOptions.Compiled);
        }

        public static string find(this string str, string pattern, string groupname, RegexOptions options)
        {
            if (null == str || null == pattern)
            {
                return String.Empty;
            }
            var m = Regex.Match(str, pattern, options);
            if (m.Success)
            {
                return m.Groups[groupname].Value;
            }
            return String.Empty;
        }

        public static string find(this string str, string pattern, int groupidx)
        {
            return find(str, pattern, groupidx, RegexOptions.Compiled);
        }

        public static string find(this string str, string pattern, int groupidx, RegexOptions options)
        {
            if (null == str || null == pattern)
            {
                return String.Empty;
            }
            var m = Regex.Match(str, pattern, options);
            if (m.Success)
            {
                return m.Groups[groupidx].Value;
            }
            return String.Empty;
        }

        public static IEnumerable<Match> findAll(this string str, string pattern){
            return findAll(str, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        public static IEnumerable<Match> findAll(this string str, string pattern, RegexOptions options){
            if (str.no()){
                yield break;
            }
            if (pattern.no()){
                yield break;
            }
            foreach (Match m in Regex.Matches(str, pattern, options)){
                yield return m;
            }
        }

        /// <summary>
        /// Выполняет подстановки в строке (без учета регистра)
        /// </summary>
        /// <param name="str">Исходная строка</param>
        /// <param name="from">Массив заменяемых строк</param>
        /// <param name="to">Массив строк - замен, null означает пропуск</param>
        /// <returns>Строка с выполненным</returns>
        public static string translate(this string str, IEnumerable<string> from, IEnumerable<string> to){
            return translate(str, from, to, true);
        }

        /// <summary>
        /// Выполняет подстановки в строке
        /// </summary>
        /// <param name="str">Исходная строка</param>
        /// <param name="from">Массив заменяемых строк</param>
        /// <param name="to">Массив строк - замен, null означает пропуск</param>
        /// <returns>Строка с выполненным</returns>
        public static string translate(this string str, IEnumerable<string> from, IEnumerable<string> to,
                                       bool ignoreCase){
            if (String.IsNullOrEmpty(str) ){
                return String.Empty;
            }
            if( null == from || null == to){
                return str;
            }
            var f = from.ToList();
            var t = to.ToList();


            var result = str;

            for (var i = 0; i < Math.Min(f.Count(), t.Count()); i++){
                var _f = f[i];
                if (String.IsNullOrEmpty(_f)){
                    continue;
                }
                var _t = t[i];
                if (null == _t){
                    continue;
                }
                if (ignoreCase){
                    result = result.replace(_f, _t);
                }
                else{
                    result = result.replace(_f, _t, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
                }
            }

            return result;
        }

        ///<summary>
        /// => split(str,false,true,',',';')
        ///</summary>
        ///<param name="str">исходная строка</param>
        ///<returns></returns>
        public static IList<string> split(this string str){
            return str.split(false, true, ',', ';');
        }

        ///<summary>
        /// Преобразует строку в список
        ///</summary>
        ///<param name="str">исходная строка</param>
        ///<param name="empty">true - включать пустые, false - не включать пустые</param>
        ///<param name="trim">true - триммировать при включении в список</param>
        ///<param name="splitters">разделители</param>
        ///<returns></returns>
        public static IList<string> split(this string str, bool empty, bool trim, params char[] splitters){
            if (str.noContent()){
                return new List<string>();
            }
            var res = (IEnumerable<string>) str.Split(splitters);
            if (!empty){
                res = res.Where(s => s.hasContent());
            }
            if (trim){
                res = res.Select(s => s.Trim());
            }
            return res.ToList();
        }

        public static string toMD5(this string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = new MD5CryptoServiceProvider();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static Regex toRegex(this string pattern){
            return pattern.toRegex(RegexOptions.None);
        }

        public static Regex toRegex(this string pattern, RegexOptions options){
            return new Regex(pattern, options | DefaultOptions);
        }

        public static RegexOptions DefaultOptions = RegexOptions.Compiled;

        public static IDictionary<string, string> toDictionary(this Regex regex, string text){
            Match m = regex.Match(text);
            if (!m.Success){
                return new Dictionary<string, string>();
            }
            string[] ss = regex.GetGroupNames();
            var result = new Dictionary<string, string>();
            foreach (string s in ss){
                result[s] = m.Groups[s].Value;
            }
            return result;
        }

        /// <summary>
        /// Создает удобные для использования в качестве ключей или системных имен строки из латиницы и подчеркиваний, добиваемся таким путем возможности использования в любом контексте
        /// </summary>
        /// <param name="str">исходная строка</param>
        /// <returns>строка с системным именем</returns>
        public static string toSystemName(this string str)
        {
            if (null == str) return "";
            return str.ToUpper().translate( _toSystemName_from, _toSystemName_to).ToUpper();
        }

        private static readonly string[] _toSystemName_from = new[]{
                                                                       "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж",
                                                                       "З", "И", "Й", "К", "Л", "М", "Н", "О",
                                                                       "П", "Р", "С", "Т"
                                                                       , "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ"
                                                                       ,
                                                                       "Ы", "Ь", "Э", "Ю", "Я", "[\r\n]", "\\W"
                                                                       ,"_+", ":"
                                                                   };

        private static readonly string[] _toSystemName_to = new[]{
                                                                     "A", "B", "V", "G", "D", "E", "YO", "ZH",
                                                                     "Z", "I", "J", "K", "L", "M", "N", "O",
                                                                     "P"
                                                                     , "R", "S",
                                                                     "T", "U", "F", "H", "TS", "CH", "SH",
                                                                     "SCH"
                                                                     , "_", "Y", "_", "E", "YU", "YA",
                                                                     String.Empty,"_","_","_"
                                                                 };

        ///<summary>
        /// Возвращает сжатую строку из первых букв слов и цифр, все остальное убирает
        ///</summary>
        ///<param name="str">исходная строка</param>
        ///<returns></returns>
        public static string toShort(this string str)
        {
            var result = str;
            result = result.replace(@"(?-i)(\b|^)\p{Ll}", m => m.Value.ToUpper(), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            result = result.replace(@"\W", String.Empty, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            result = result.replace(@"(?-i)\p{Ll}", String.Empty, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return result;

        }

		

        public static string simplifyBoo(string str){
            //strip inputs
            var result = Regex.Replace(str, @"^\s*(import\s[^\r\n]+\s+)+", "", RegexOptions.Compiled);
            //standartize quotes - it's not nesseccary to test quotation in most caseses
            result = result.Replace("\"", "'");

#if LIB2
            result = result.replace(@"\$\((?<x>[^)]+)\)", "${${x}}");
            result = result.Replace("\"", "~~").Replace("'", "~~").replace(@"import[^\r\n]+[\r\n]+", "");
#endif
            
            return result.Trim();
        }
    }
}