#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Comdiv.Extensions;

#endregion

namespace Comdiv._Extensions_Internals{
    /// <summary>
    /// »нкапсулирует единицу захвата, прив€занную к определенному месту в иерархии	разбора с сервисами дл€ удобства генерации XML (не мешают использовать	стандартные регул€рки
    /// </summary>
    /// <summary>
    /// Summary description for MatchesToXmlTransformer.
    /// </summary>
    internal class TextToXmlTransformer{
        private string _documentName = "matches";
        private string _itemName = "match";

        public TextToXmlTransformer(Regex reg, string docname, string itemname, bool uselocation){
            Regex = reg;
            DocumentName = docname;
            ItemName = itemname;
            UseLocation = uselocation;
        }

        public TextToXmlTransformer() {}

        public string Pattern{
            get { return Regex == null ? string.Empty : Regex.ToString(); }
            set{
                Regex = new
                    Regex(value, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            }
        }

        public string DocumentName{
            get { return _documentName; }
            set { _documentName = value; }
        }

        public string ItemName{
            get { return _itemName; }
            set { _itemName = value; }
        }

        public Regex Regex { get; set; }

        public bool UseLocation { get; set; }

        public void TransformFiles(string sourcepath, string targetdirpath, string
                                                                                regexpath, Encoding readencoding){
            var sdir = Path.GetDirectoryName(sourcepath);
            var pattern = Path.GetFileName(sourcepath);
            foreach (var sfn in Directory.GetFiles(sdir, pattern)){
                var tfn = targetdirpath == string.Empty
                              ? Path.GetFileName(sfn) + ".xml"
                              : string.Format(targetdirpath, Path.GetFileNameWithoutExtension(sfn));
                TransformFile(sfn, tfn, regexpath, readencoding);
            }
        }

        public void TransformFile(string sourcepath, string targetpath, string
                                                                            regexpath, Encoding readencoding){
            if (regexpath.hasContent())
                Pattern = readFile(regexpath, null);
            TransformFile(sourcepath, targetpath, readencoding);
        }

        public void TransformFile(string sourcepath, string targetpath, Encoding
                                                                            readencoding){
            var source = readFile(sourcepath, readencoding);
            ((XmlDocument) Transform(source)).Save(targetpath);
        }

// ReSharper disable MemberCanBeMadeStatic
        private string readFile(string path, Encoding e)
// ReSharper restore MemberCanBeMadeStatic
        {
            if (e == null)
                e = Encoding.UTF8;
            var s = new FileStream(path, FileMode.Open, FileAccess.Read);
            var r = new StreamReader(s, e);
            try{
                return r.ReadToEnd();
            }
            finally{
                r.Close();
                s.Close();
            }
        }

        public XmlNode Transform(string input){
            return Transform(Regex, input);
        }

        public XmlNode Transform(string input, string pattern){
            return Transform(new Regex(pattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled), input);
        }

        public static XmlNode GetXml(string input, string pattern){
            return new TextToXmlTransformer().Transform(input, pattern);
        }

        public static XmlNode GetXml(Regex reg, string input){
            return new TextToXmlTransformer().Transform(reg, input);
        }

        public XmlNode Transform(Regex reg, string input){
            //класс не €вл€етс€ потокозащищенным
            lock (this){
                var groupNames = reg.GetGroupNames();
                var ms = reg.Matches(input);
                var doc = new XmlDocument();
                var b = new StringBuilder();
                var sw = new StringWriter(b);
                var w = new XmlTextWriter(sw);
                if (DocumentName != null){
                    w.WriteStartDocument(true);
                    w.WriteStartElement(DocumentName);
                }
                var i = 0;
                foreach (Match m in ms){
                    i++;
                    if (ItemName != null) w.WriteStartElement(ItemName);
                    if (UseLocation){
                        w.WriteAttributeString("n_", i.ToString());
                        w.WriteAttributeString("s_", m.Index.ToString());
                        w.WriteAttributeString("l_", m.Length.ToString());
                    }
                    transformToXml(m, w, groupNames, reg);
                    if (ItemName != null) w.WriteEndElement();
                }
                if (DocumentName != null) w.WriteEndElement();
                doc.InnerXml = b.ToString();
                normalizeStructure(doc);
                return doc;
            }
        }

        public string TransformToString(string input){
            return Transform(input).OuterXml;
        }

        internal void transformToXml(AdressedCapture c, XmlWriter w){
            transformToXml(c, w, 0);
        }

        internal void transformToXml(AdressedCapture c, XmlWriter w, int depth) {
            if (depth == 20)
                return;
            w.WriteStartElement(c.NormalName);
            if (UseLocation){
                w.WriteAttributeString("s_", c.Start.ToString());
                w.WriteAttributeString("e_", c.End.ToString());
            }
            if (c.UseAsAttribute)
                w.WriteAttributeString("a", "+");
            if (c.GroupInCollection)
                w.WriteAttributeString("c", "+");
            if (c.Childs.Count == 0 || c.UseAlwaysWithValue)
                w.WriteString(c.Value);
            foreach (var c_ in c.Childs) transformToXml(c_, w, depth + 1);
            w.WriteEndElement();
        }

        public void transformToXml(Match m, XmlWriter w, string[] groupNames, Regex reg){
            var captures = getNamedCaptureIndex(m, groupNames, reg);
            generateStructure(captures);
            foreach (var c in captures)
                if (c.Parent == null) transformToXml(c, w);
        }

// ReSharper disable MemberCanBeMadeStatic
        internal List<AdressedCapture> getNamedCaptureIndex(Match m, string[] groupNames, Regex reg)
// ReSharper restore MemberCanBeMadeStatic
        {
            var res = new List<AdressedCapture>();
            var captureid = 1;
            var findex = 0;
            foreach (var groupName in groupNames){
                //√руппа с номером 0 всегда попадает в список имен, даже если включена опци€			ExplicitCapture, это надо отсечь, равно как любые другие не эксплицитные группы
                if (groupName == "0" || Regex.IsMatch(groupName, "^\\d+$")) continue;

                findex++;
                foreach (Capture c in m.Groups[groupName].Captures){
                    res.Add(new AdressedCapture(captureid, groupName, c, reg.GroupNumberFromName(groupName)));
                    captureid++;
                }
            }
            res.Sort();
            return res;
        }

// ReSharper disable MemberCanBeMadeStatic
        internal void generateStructure(List<AdressedCapture> captures)
// ReSharper restore MemberCanBeMadeStatic
        {
            if (captures == null) throw new ArgumentNullException("captures");
            var parents = new List<AdressedCapture>(captures);
            var childs = new List<AdressedCapture>(captures);
            while (parents.Count > 0){
                var unchanged = new List<AdressedCapture>(parents);
                foreach (var p in parents){
                    foreach (var c in childs){
                        if (p == c || p.Parent == c || c.Parent == p) continue;
                        if (p.Contains(c)){
                            if (c.Parent == null){
                                c.Parent = p;
                                p.Childs.Add(c);
                                unchanged.Remove(p);
                            }
                            else if (c.Parent.Contains(p)){
                                c.Parent.Childs.Remove(c);
                                c.Parent = p;
                                p.Childs.Add(c);
                                unchanged.Remove(p);
                            }
                        }
                    }
                }
                foreach (var obj in unchanged) parents.Remove(obj);
            }
        }

        private void normalizeStructure(XmlNode n){
            buildAttributes(n);
        }

// ReSharper disable MemberCanBeMadeStatic
        private void buildAttributes(XmlNode n)
// ReSharper restore MemberCanBeMadeStatic
        {
            if (n == null) throw new ArgumentNullException("n");
// ReSharper disable PossibleNullReferenceException
            foreach (XmlNode n_ in n.SelectNodes(".//*[@a='+']"))
// ReSharper restore PossibleNullReferenceException
            {
                var p = n_.ParentNode;
                if (p == null || !(p is XmlElement))
                    continue;
                ((XmlElement) p).SetAttribute(n_.Name, n_.InnerText);
                p.RemoveChild(n_);
            }
        }
    }
}