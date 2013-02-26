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
#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Useful;
using Comdiv.Xml;

#endregion

//using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Comdiv.Xslt{

    #region

    #endregion

    #region

    #endregion

#pragma warning disable 1591
    [Obsolete]
    public class XsltStandardExtension{
        #region Common

        private readonly Dictionary<string, object> instances = new Dictionary<string, object>();
        private int id = 1;

        public object onempty(string val, object other){
            if (val.noContent()) return other;
            return val;
        }

        public string guid(){
            return Guid.NewGuid().ToString();
        }

        //ÔÂÂ‚Â‰ÂÌÓ

        public string getid(){
            return (id++).ToString();
        }

        public int type_of(object obj){
            if (obj is string) return 0;
            if (obj is double) return 1;
            if (obj is bool) return 2;
            if (obj is XPathNodeIterator) return 3;
            if (obj is XPathNavigator) return 4;
            return -1;
        }

        public string create(string name, string clsRef){
            var obj = Type.GetType(clsRef).GetConstructor(Type.EmptyTypes).Invoke(null);
            instances[name] = obj;
            return string.Empty;
        }

        public string callstr(string name, string method, string parameters, string delimiter){
            object obj = null;
            if (instances.ContainsKey(name)) obj = instances[name];
            else{
                obj = name.toType().create<object>();
                instances[name] = obj;
            }
            var args = new[]{parameters};
            if (delimiter.hasContent())
                args = parameters.Split(delimiter[0]);
            var result = obj.GetType().InvokeMember(method,
                                                    BindingFlags.Public | BindingFlags.InvokeMethod |
                                                    BindingFlags.Instance, null, obj, args);
            return result == null ? string.Empty : result.ToString();
        }

        #endregion

        #region Web

        private readonly Encoding defaultEncodoing = Encoding.GetEncoding(1251);

        public string quickHttp(string url){
            return quickHttp(url, defaultEncodoing);
        }

        public string quickHttp(string url, string encoding){
            int code;
            if (!int.TryParse(encoding, out code)) code = 1251;
            return quickHttp(url, Encoding.GetEncoding(code));
        }

        public string http(string url, string enc, string prx, string prxName, string prxPass, string prxDomain,
                           string userName, string password){
            var proxy = (WebProxy) WebRequest.DefaultWebProxy;
            proxy.Credentials = new NetworkCredential(prxName, prxPass, prxDomain);
            proxy.Address = new Uri(prx);
            WebRequest.DefaultWebProxy = proxy;
            using (var cl = new WebClient()){
                /*if(Cookies!=null)
				{
					string h = Cookies.GetCookieHeader(new Uri(url));
					if(h!=null&&h.Length!=0)	cl.Headers.Add("Cookie",h);
				}*/
                cl.Credentials = new NetworkCredential(userName, password);
                var data = cl.DownloadData(url);

                /*if(Cookies!=null&&Comdiv.Common.Extensions.HasContent(cl.ResponseHeaders["Set-Cookie"]))
				{
					if(Cookies is MT.Web.IPersistentCookieContainer)
					{
						((MT.Web.IPersistentCookieContainer)Cookies).SetCookiePersisted(new Uri(url),cl.ResponseHeaders["Set-Cookie"]);
					}
					else
					{
						Cookies.SetCookies(new Uri(url),cl.ResponseHeaders["Set-Cookie"]);
					}
				}
*/
                return Encoding.GetEncoding(enc).GetString(data);
            }
        }

// ReSharper disable MemberCanBeMadeStatic
        protected string quickHttp(string url, Encoding encoding)
// ReSharper restore MemberCanBeMadeStatic
        {
            //   MT.Web.ProxySelector.SelectProxy();
            var request = (HttpWebRequest) WebRequest.Create(new Uri(url));
            try{
                var response = (HttpWebResponse) request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            catch (Exception ex){
                return string.Format("<http_error>{0}</http_error>", ex);
            }
        }

        #endregion

        #region Cache

        private readonly Hashtable _nst = new Hashtable();
        private readonly Hashtable cash = new Hashtable();

        private readonly Hashtable joins = new Hashtable();

        public bool setkey(string key, string val){
            var hk = key + "~" + val;
            if (!cash.ContainsKey(hk))
                cash.Add(hk, val);
            return true;
        }

        public bool exists(string key, string val){
            var hk = key + "~" + val;
            return cash.ContainsKey(hk);
        }

        public bool delete(string key){
            var deletes = new ArrayList();
            foreach (string k in cash.Keys){
                if (k.Split('~')[0] == key)
                    deletes.Add(k);
            }
            foreach (var obj in deletes) cash.Remove(obj);
            return true;
        }

        public XPathNodeIterator values(string key){
            var res = new StringBuilder();
            var sw = new StringWriter(res);
            var w = new XmlTextWriter(sw);
            w.WriteStartElement("root");
            foreach (string k in cash.Keys){
                if (k.Split('~')[0] == key)
                    w.WriteElementString("value", cash[k] as string);
            }
            w.WriteEndElement();
            w.Flush();
            var doc = new XPathDocument(new StringReader(res.ToString()));
            return doc.CreateNavigator().Select("//value");
        }

        public string val(string key, int num){
            return string.Empty;
        }

        public double count(string key){
            var c = 0;
            foreach (string k in cash.Keys){
                if (k.Split('~')[0] == key)
                    c++;
            }
            return c;
        }

        public object push(int key, object obj){
            getStack(key).Push(obj);
            return null;
        }

        public object peek(int key){
            var s = getStack(key);
            if (s.Count == 0) return null;
            return getStack(key).Peek();
        }

        public object pop(int key, int count){
            object res = null;
            var c = count;
            var s = getStack(key);
            while ((--c) >= 0 && s.Count > 0) res = s.Pop();
            return res;
        }

        public object cl_stack(int key){
            getStack(key).Clear();
            return null;
        }

        public string join_stack_str(int key){
            var res = string.Empty;
            object c;
            var st = (Stack) getStack(key).Clone();
            while (st.Count > 0) res += (c = st.Pop()) == null ? string.Empty : c + "; ";
            return res;
        }

        public string join_stack_str(){
            return join_stack_str(0);
        }

        public string join(XPathNodeIterator iter, string joiner, string select){
            var res = string.Empty;
            while (iter.MoveNext()){
                res += joiner;
                res += (string) iter.Current.Evaluate(select);
            }
            return res;
        }

        public string join(XPathNodeIterator iter, string next, string joiner_, string select, int reverse, int distinct){
            joins.Clear();
            var res = string.Empty;
            while (iter.MoveNext()){
                var joiner = (string) iter.Current.Evaluate(joiner_);
                var sub = (string) iter.Current.Evaluate(select);
                var line = iter.Current.Select(next);
                if (distinct == 0 || (!joins.Contains(sub))){
                    joins.Add(sub, sub);
                    if (reverse == 0){
                        res += joiner;
                        res += sub;
                        res += join(line, next, joiner_, select, reverse, distinct);
                    }
                    else{
                        res = joiner + sub + res;
                        res = join(line, next, joiner_, select, reverse, distinct) + res;
                    }
                }
                else{
                    if (reverse == 0)
                        res += join(line, next, joiner_, select, reverse, distinct);
                    else
                        res = join(line, next, joiner_, select, reverse, distinct) + res;
                }
            }
            return res;
        }

        public object push(object obj){
            return push(0, obj);
        }

        public object peek(){
            return peek(0);
        }

        public object pop(int count){
            return pop(0, count);
        }

        public object pop(){
            return pop(0, 1);
        }

        public object cl_stack(){
            return cl_stack(0);
        }

        protected Stack getStack(int key){
            if (_nst[key] == null) _nst[key] = new Stack();
            return _nst[key] as Stack;
        }

        #endregion

        #region Strings

        private const string trto = "A B V G D E YO ZH Z I J K L M N O P R S T U F H TS CH SH SCH _ Y _ E YU YA";
        private const string trwhat = "¿¡¬√ƒ≈®∆«»… ÀÃÕŒœ–—“”‘’÷◊ÿŸ⁄€‹›ﬁﬂ";
        private readonly ArrayList trtos = new ArrayList();
        private readonly ArrayList trwats = new ArrayList();
        private Encoding renc_from;
        private string renc_old_from = string.Empty;
        private string renc_old_to = string.Empty;
        private Encoding renc_to;

        private void checkSubs(){
            if (trwats.Count == 0){
                foreach (var c in trwhat) trwats.Add(c.ToString());
                foreach (var s in trto.Split(' ')) trtos.Add(s);
            }
        }

        public string sql_dtp_between_condition(string sd, string ed, string sc, string ec){
            var p = DateRange.Parse(sd, ed, sc, ec);
            return string.Format("between '{0}' and '{1}'", p.Start.ToString("yyyy-MM-dd HH:mm"),
                                 p.Finish.ToString("yyyy-MM-dd HH:mm"));
        }

        public string firstlower(string input){
            if (input.noContent()) return input;
            var a = input[0];
            var b = char.ToLower(a);
            return a == b ? input : b + input.Remove(0, 1);
        }

        public string firstupper(string input){
            if (input.noContent()) return input;
            var a = input[0];
            var b = char.ToUpper(a);
            return a == b ? input : b + input.Remove(0, 1);
        }

        public string isnull(string src, string def){
            return src.hasContent() ? src : def;
        }


        public string reencode(string data){
            return reencode(data, "Windows-1252", "Windows-1251");
        }

        public string reencode(string data, string from, string to){
            if (renc_old_from != from) renc_from = Encoding.GetEncoding(renc_old_from = from);
            if (renc_old_to != to) renc_to = Encoding.GetEncoding(renc_old_to = to);
            var res = renc_to.GetString(renc_from.GetBytes(data));
            return res;
        }

        public string tokey(string data){
            checkSubs();
            data = data.Trim().ToUpper();
            data = Regex.Replace(data, "\\s+", "_");
            data = Regex.Replace(data, "[^\\s\\w]+", string.Empty);
            for (var i = 0; i < trwats.Count; i++){
                data = i > trtos.Count - 1
                           ? data.Replace((string) trwats[i], string.Empty)
                           : data.Replace((string) trwats[i], (string) trtos[i]);
            }
            return data;
        }

        public string safed(string data){
            return sqlsafed(websafed(data));
        }

        public string sqlsafed(string data){
            return data.toSqlString();
        }

        public string websafed(string data){
            return data.toHtml();
        }

        public string sqlsdate(string data){
            return data.Length == 10 ? data : string.Empty;
        }

        public string string_join(XPathNodeIterator source, string joiner){
            return XmlUtil.Join(source, joiner);
        }

        public string upper(string input){
            return input.ToUpper();
        }

        public string lower(string input){
            return input.ToLower();
        }

        public string convert_to_paragraphs(string htmlToConvert){
            var result = "<p>" + htmlToConvert + "</p>";
            result = result.replace(@"(?i)<br\s*/?>", m => "</p><p>");
            return result;
        }

        
        public string tostring(XPathNodeIterator i){
#if OLD
			return XmlUtil.NodeSetToString(i);
#else

            var sw = new StringWriter();
            var sets = new XmlWriterSettings{Indent = true, OmitXmlDeclaration = true};
            using (var w = XmlWriter.Create(sw, sets)){
                while (i.MoveNext())
                    if (w != null) i.Current.WriteSubtree(w);
            }
            return sw.ToString();

#endif
        }

        public string format(string pattern, string arg){
            return string.Format(pattern, arg);
        }

        public string format(string pattern, string arg, string arg2){
            return string.Format(pattern, arg, arg2);
        }

        public string format(string pattern, string arg, string arg2, string arg3){
            return string.Format(pattern, arg, arg2, arg3);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4){
            return string.Format(pattern, arg, arg2, arg3, arg4);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4, string arg5){
            return string.Format(pattern, arg, arg2, arg3, arg4, arg5);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4, string arg5, string arg6){
            return string.Format(pattern, arg, arg2, arg3, arg4, arg5, arg6);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4, string arg5, string arg6,
                             string arg7){
            return string.Format(pattern, arg, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4, string arg5, string arg6,
                             string arg7, string arg8){
            return string.Format(pattern, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4, string arg5, string arg6,
                             string arg7, string arg8, string arg9){
            return string.Format(pattern, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4, string arg5, string arg6,
                             string arg7, string arg8, string arg9, string arg10){
            return string.Format(pattern, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public string format(string pattern, string arg, string arg2, string arg3, string arg4, string arg5, string arg6,
                             string arg7, string arg8, string arg9, string arg10, string arg11){
            return string.Format(pattern, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public string format(string pattern, XPathNodeIterator iter){
            return XmlUtil.XPathFormatString(pattern, iter);
        }

        public XPathNodeIterator fromstring(string data){
            var iter = XmlUtil.NodeSetFromString(data);
            return iter;
        }

        public string normaldate(string date){
            return normaldate(date, true, true);
        }

        public string normaldate(string date, bool withTime, bool cutDateForToday){
            var d = DateTime.Parse(date);
            if (!withTime) return d.ToShortDateString();
            if (cutDateForToday && (d.Date == DateTime.Today)) return d.ToShortTimeString();
            return d.ToString();
        }

        #endregion

        #region Data Operations

        public XPathNodeIterator distinct(XPathNodeIterator source, string by){
            return XmlAggregator.Distinct(source, by, string.Empty, this);
        }

        public XPathNodeIterator distinct(XPathNodeIterator source, string by, string ns){
            return XmlAggregator.Distinct(source, by, ns, this);
        }

        public XPathNodeIterator rel_join(XPathNodeIterator one, XPathNodeIterator two, string by, string name,
                                          string prefix, string ns, string fp, string sp){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            w.WriteStartElement("root");
            w.WriteAttributeString("xmlns", "xes", "http://www.w3.org/2000/xmlns/", "MT-xml-expert-systems");

            while (one.MoveNext()){
                var t = two.Clone();
                while (t.MoveNext()){
                    w.WriteStartElement(prefix, name, ns);
                    var attrs = one.Current.Clone();
                    while (attrs.MoveToFirstAttribute() || attrs.MoveToNextAttribute()){
                        if (attrs.Prefix == "xes" && attrs.LocalName == "UID") continue;
                        w.WriteAttributeString(attrs.Prefix, fp + attrs.LocalName, attrs.NamespaceURI, attrs.Value);
                    }
                    attrs = t.Current.Clone();
                    while (attrs.MoveToFirstAttribute() || attrs.MoveToNextAttribute()){
                        if (attrs.Prefix == "xes" && attrs.LocalName == "UID") continue;
                        w.WriteAttributeString(attrs.Prefix, sp + attrs.LocalName, attrs.NamespaceURI, attrs.Value);
                    }
                    w.WriteEndElement();
                }
            }
            w.WriteEndElement();
            w.Flush();
            var nav = XmlUtil.GetNavigator(sw.ToString());
            var res = nav.Select("/root/*[" + by + "]");
            return res;
        }

        public string min(XPathNodeIterator iter){
            return min(iter, "string");
        }

        public string max(XPathNodeIterator iter){
            return max(iter, "string");
        }

        public string min(XPathNodeIterator iter, string type){
            return minmax(iter, type, null).Split('®')[0];
        }

        public string max(XPathNodeIterator iter, string type){
            return minmax(iter, type, null).Split('®')[1];
        }

        public string min(XPathNodeIterator iter, string type, string format){
            return minmax(iter, type, format).Split('®')[0];
        }

        public string max(XPathNodeIterator iter, string type, string format){
            return minmax(iter, type, format).Split('®')[1];
        }

        public string minmax(XPathNodeIterator iter, string type, string format){
            object min_ = null;
            object max_ = null;
            var min = string.Empty;
            var max = string.Empty;
            while (iter.MoveNext()){
                var v = iter.Current.Value;
                object val;
                switch (type){
                    case "string":
                        val = v;
                        if (min_ == null || ((string) val).CompareTo((string) min_) < 0){
                            min_ = val;
                            min = v;
                        }
                        if (max_ == null || ((string) val).CompareTo((string) max_) > 0){
                            max_ = val;
                            max = v;
                        }
                        break;
                    case "number":
                        val = double.Parse(v);
                        if (min_ == null || (double) val < (double) min_){
                            min_ = val;
                            min = v;
                        }
                        if (max_ == null || (double) val > (double) max_){
                            max_ = val;
                            max = v;
                        }
                        break;
                    case "datetime":
                        val = DateTime.Parse(v);
                        if (min_ == null || (DateTime) val < (DateTime) min_){
                            min_ = val;
                            min = v;
                        }
                        if (max_ == null || (DateTime) val > (DateTime) max_){
                            max_ = val;
                            max = v;
                        }
                        break;
                }
            }
            if (format.hasContent()){
                min = string.Format(format, min_);
                max = string.Format(format, max_);
            }
            return min + "®" + max;
        }

        public double sum(XPathNodeIterator iter){
            double res = 0;
            while (iter.MoveNext())
                res += double.Parse(iter.Current.Value);
            return res;
        }

        #endregion

        #region Regex

        public bool like(string input, string regex) {
           return Regex.IsMatch(input, regex);
			
        }

        public string match(string input, string regex){
            return Regex.Match(input, regex).Value;
        }

        public string match(string input, string regex, string group){
            return Regex.Match(input, regex).Groups[group].Value;
        }

        public string replace(string input, string regex, string pattern){
            return Regex.Replace(input, regex, pattern);
        }

        public XPathNodeIterator text_to_xml(string text, string regex){
            var doc = new XPathDocument(new StringReader(regex.toRegex().toXml(text,2000)));
            return doc.CreateNavigator().Select("//matches");
        }

        public string text_to_xml_string(string text, string regex){
            return regex.toRegex().toXml(text, 2000);
        }

        public XPathNodeIterator matches(XPathNodeIterator input, string pattern, string replace, bool distinct){
            if (distinct) return matches(input, pattern, replace);
            return null;
        }

        public XPathNodeIterator matches(XPathNodeIterator input, string pattern, string replace){
            if (replace == null || replace.Length == 0) return matches(input, pattern);
            return null;
        }

        public XPathNodeIterator matches(XPathNodeIterator input, string pattern){
            var res_stream = new MemoryStream();
            var res_text_writer = new StreamWriter(res_stream);
            var res_writer = new XmlTextWriter(res_text_writer);
            res_writer.WriteStartElement("root");
            var test = new Regex(pattern);
            var testval = false;
            foreach (var gn in test.GetGroupNames()){
                if (gn == "val"){
                    testval = true;
                    break;
                }
            }
            var result_list = new ArrayList();
            while (input.MoveNext()){
                var matches_ = test.Matches(input.Current.Value);
                foreach (Match match in matches_){
                    if (match.Success){
                        var res_ = testval ? match.Groups["val"].Value : match.Value;
                        if (!result_list.Contains(res_)){
                            result_list.Add(res_);
                            res_writer.WriteElementString("value", res_);
                        }
                    }
                }
            }
            res_writer.WriteEndElement();
            res_writer.Flush();
            res_stream.Position = 0;
            var res_doc = new XPathDocument(res_stream);
            return res_doc.CreateNavigator().Select("//value");
        }

        #endregion

        #region IO

        private string basedir = Environment.CurrentDirectory;

        public string resolvepath(string dir){
            if (Path.IsPathRooted(dir)) return dir;
            else return basedir + "\\" + dir;
        }

        public bool setbase(string bdir){
            basedir = bdir;
            return true;
        }

        public bool writefile(string data, string path){
            return writefile(data, path, 0);
        }

        public bool writefile(string data, string path, int encoding){
            return writefile(data, path, encoding, true);
        }

        public bool localize(string url, string path){
            //MT.Web.ProxySelector.SelectProxy();
            var cl = new WebClient();
            var data = cl.DownloadData(url);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var s = new FileStream(path, FileMode.Create);
            try{
                s.Write(data, 0, data.Length);
                s.Flush();
            }
            finally{
                s.Close();
            }
            return true;
        }

        public bool writefile(string data, string path, int encoding, bool throwException){
            var e = encoding == 0 ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var w = new StreamWriter(path, false, e);
            try{
                w.Write(data);
                w.Flush();
            }
            catch (Exception exc){
                Trace.Write(exc.ToString(), "error");
                if (throwException)
                    throw;
                return false;
            }
            finally{
                w.Close();
            }
            return true;
        }

        public int fexec(string fn1_, string fn2_, string ev, bool ex){
            var fn1 = resolvepath(fn1_);
            var fn2 = resolvepath(fn2_);
            try{
                Directory.CreateDirectory(Path.GetDirectoryName(fn2));
                switch (ev){
                    case "move":
                        File.Move(fn1, fn2);
                        break;
                    case "copy":
                        File.Copy(fn1, fn2);
                        break;
                    case "del":
                        File.Delete(fn1);
                        break;
                }
                return 0;
            }
            catch{
                if (!ex) return -1;
                throw;
            }
        }

        public int fmove(string sfile, string tfile){
            return fexec(sfile, tfile, "move", true);
        }

        public int fcopy(string sfile, string tfile){
            return fexec(sfile, tfile, "copy", true);
        }

        public int fdel(string sfile){
            return fexec(sfile, string.Empty, "del", true);
        }

        public bool createdir(string dir){
            Directory.CreateDirectory(resolvepath(dir));
            return true;
        }

        public string path_part(string path, string part){
            switch (part){
                case "fp":
                    return Path.GetFullPath(path);
                case "d":
                    return Path.GetDirectoryName(path);
                case "n":
                    return Path.GetFileName(path);
                case "e":
                    return Path.GetExtension(path);
                case "ne":
                    return Path.GetFileNameWithoutExtension(path);
                case "r":
                    return Path.GetPathRoot(path);
            }
            return string.Empty;
        }

        #endregion

        #region sql

        private string defDb = "UNI";

        public bool setdb(string db){
            defDb = db;
            return true;
        }

        public XPathNodeIterator uni(string db, string proc, XPathNodeIterator i){
            return uni(db, proc, "<?xml version=\"1.0\" encoding=\"windows-1251\"?><root>" + tostring(i) + "</root>");
        }

        public XPathNodeIterator uni(string db, string proc, string text){
            if (text.IndexOf("utf-16") != -1)
                text = Regex.Replace(text, "<\\?xml[^6>]+16[\\s\\S]+?\\?>", string.Empty);
            var b = new StringBuilder();
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString)){
                con.Open();
                var command = new SqlCommand(proc);
                command.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(command);
                command.Parameters[1].Value = text;


                using (IDataReader reader = command.ExecuteReader())
                    while (reader.Read()) b.Append(reader[0].ToString());
            }
            return XmlUtil.GetIterator(XmlUtil.WrapAsXml(b.ToString()));
        }

        public XPathNodeIterator uni(string sql){
            return this.sql(defDb, sql);
        }

        public XPathNodeIterator uni(string proc, string text){
            return uni(defDb, proc, text);
        }

        public XPathNodeIterator sql(string db, string query){
            var commands = query.Split('^');
            var b = new StringBuilder();
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings[db].ConnectionString)){
                con.Open();
                foreach (var command in commands){
                    if (!command.hasContent()) continue;
                    var c = new SqlCommand(command, con);
                    using (IDataReader reader = c.ExecuteReader()){
                        bool nr_;
                        while ((nr_ = reader.Read()) || (reader.NextResult()))
                            if (nr_) b.Append(reader[0]);
                    }
                }
            }
            return XmlUtil.GetIterator(XmlUtil.WrapAsXml(b.ToString()));
        }

        public XPathNodeIterator sql(string query){
            return sql(defDb, query);
        }

        public string sqlv(string query){
            return sqlv("uni", query);
        }

        public string sqlv(string db, string query){
            var cons = ConfigurationManager.ConnectionStrings[db];
            using (var con = new SqlConnection(cons.ConnectionString)){
				query = "set dateformat ymd;\r\n"+query;
                var command = new SqlCommand(query, con);
                con.Open();
                var val = command.ExecuteScalar();
                var ret = (val == null || val is DBNull) ? string.Empty : val.ToString();
                return ret;
            }
        }

//
//        public string sql_dtp_between_condition(string sd, string ed, string sc, string ec)
//        {
//            DateRange p = DateRange.Parse(sd, ed, sc, ec);
//            return string.Format("between '{0}' and '{1}'", p.StartDate.ToString("yyyy-MM-dd HH:mm"),
//                                 p.EndDate.ToString("yyyy-MM-dd HH:mm"));
//        }

        #endregion

        #region ASP.NET

        public bool setsession(string name, string value){
            HttpContext.Current.Session[name] = value;
            return true;
        }

        public string getsession(string name){
            return getsession(name, string.Empty);
        }

        public string getsession(string name, string def){
            var res = (string) HttpContext.Current.Session[name];
            return res ?? def;
        }

        public string request(string name, string def){
            var res = HttpContext.Current.Request[name];
            return res ?? def;
        }

        public string html_to_text(string html){
            return html.toText();
        }

        public string html_clear(string html){
            return html.cleanHtml();
        }

        public XPathNodeIterator html_pars(string text_){
            var sw = new StringWriter();
            XmlWriter w = new XmlTextWriter(sw);
            w.WriteString(text_);

            var xml = string.Format("<root><p>{0}</p></root>",
                                    Regex.Replace(sw.ToString(), "[\r\n]+", "</p><p>"));
            return XmlUtil.GetNavigator(xml).Select("//p");
        }

        #endregion

        #region DATES

        public string today(){
            return DateTime.Today.ToShortDateString();
        }

        public string now(){
            return DateTime.Now.ToString();
        }

        public string today(double delta){
            return DateTime.Today.AddDays((int) delta).ToShortDateString();
        }

        public string date(){
            return DateTime.Today.ToString("dd.MM.yyyy");
        }

        public string get_date_format(string date){
            return get_date_format(date, "d.M.yyyy");
        }

        public string get_date_format(string date, string format){
            return dtParse(date).ToString(format);
        }

        public string get_date_format(string date, string format, string informat){
            return DateTime.ParseExact(date.Trim(), informat, DateTimeFormatInfo.InvariantInfo).ToString(format);
            //return dtParse(date).ToString(format);
        }

        public string get_gmt(string date, int timezone){
            var dt = dtParse(date);
            dt = dt.AddHours(-timezone);
            return dt.ToString("R");
        }

        public double date_value(string date){
            return dtParse(date).ToOADate();
        }

        public double date_value(string date, string format){
            date = Regex.Replace(date, "\\s+", " ");
            return DateTime.ParseExact(date, format, null).ToOADate();
        }

        public string add_days(string date, int add){
            return dtParse(date).AddDays(add).ToShortDateString();
        }

        
        public string normalDate(string date, string format){
            date = Regex.Replace(date, "\\s+", " ");
            return DateTime.ParseExact(date, format, null).ToString();
        }


// ReSharper disable MemberCanBeMadeStatic
        private DateTime dtParse(string date)
// ReSharper restore MemberCanBeMadeStatic
        {
            return date.toDate();
        }

        public double stamp(){
            return Environment.TickCount;
        }

        public string xml_date(string date){
            return date.toXmlDateString();
        }

		public string xml_date_or_today(string date) {
			var d = date.toDate();
			if (d.Year <= 1900) d = DateTime.Today;
			return d.ToString("yyyy-MM-ddThh:mm:ss");
			
		}

        public string sql_date(string date){
            return date.toSqlDateString();
        }

        #endregion

        #region MATH

        public double log10(double val){
            return Math.Log10(val);
        }

        public double ln(double val){
            return Math.Log(val);
        }

        public double log(double val, double bas){
            return Math.Log(val, bas);
        }

        public double round(double val){
            return Math.Round(val, 0);
        }

        public double floor(double val){
            return Math.Floor(val);
        }

        #endregion

        //ÔÂÂÌÂÒÂÌÓ

        //ÔÂÂÌÂÒÂÌÓ
        private static CookieContainer _container;
        private static string _namespace = "urn:ariis:xslt:system";
        //ÔÂÂÌÂÒÂÌÓ
        public static string Namespace{
            get { return _namespace; }
            set { _namespace = value; }
        }


        [Obsolete]
        public static CookieContainer Cookies{
            get { return _container; }
            set { _container = value; }
        }

        public static XsltArgumentList PrepareArgs(){
            var args = new XsltArgumentList();
            PrepareArgs(args);
            return args;
        }

        //ÔÂÂÌÂÒÂÌÓ
        public static void PrepareArgs(XsltArgumentList args){
            args.RemoveExtensionObject(Namespace);
            args.AddExtensionObject(Namespace, new XsltStandardExtension());
        }
    }
}

#pragma warning restore 1591

#region OUTDATE

/*
public string trace(string template,string message)
        {
            GlobalXsltTrasser.Write(this,"template:"+template,message);
            return string.Empty;
        }
 */

#endregion