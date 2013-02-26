using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Extensions;

namespace Comdiv.Xml.Smart {
    public class SmartXml {
        public SmartXml(XElement target){
            this.Element = target;
            this.SrcContent = this.Element.ToString();
        }

        public XElement Element { get; private set; }
        public string SrcContent { get; private set; }

        public IDictionary<string, string> Globals{
            get { return globals; }
        }

        public IList<Substitution> Substitutions{
            get { return substitutions; }
        }

        /// <summary>
        /// cleans given attributes, inspired by BxlTester where _line and _file attributes disrupts readability of results
        /// </summary>
        /// <param name="target"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static XElement CleanAttributes(XElement target, params string [] attributes) {
            var attrs = ((IEnumerable) target.XPathEvaluate("//@*")).OfType<XAttribute>().ToArray();
            foreach (var attr in attrs) {
                if(attributes.Contains(attr.Name.LocalName)) {
                    attr.Remove();
                }
            }
            return target;

        }

        /// <summary>
        /// Method to quickly return Elements in DocElement filtered by XPATH, supports pseudo XPATHS 
        /// with notations ID == ./*[@id=ID] and TYPE ID == ./TYPE[@id=ID]
        /// </summary>
        /// <param name="target"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static XElement FilterByXPath(XElement target, string  xpath) {
            if(xpath.noContent()) return target;
            xpath = xpath.Trim();
            var result = new XElement("root");
            if(xpath.like(@"^[\w\d]+$")) {
                xpath = string.Format("./*[@id='{0}']", xpath);
            }else if(xpath.like(@"^([\w\d]+)\s+([\w\d]+)$")) {
                var m = Regex.Match(xpath, @"^([\w\d]+)\s+([\w\d]+)$");
                var t = m.Groups[1].Value;
                var id = m.Groups[2].Value;
                xpath = string.Format("./{0}[@id='{1}']", t, id);
            }
            var elements = target.XPathSelectElements(xpath);
            foreach (var e in elements) {
                result.Add(new XElement(e));
            }
            return result;
        }

        private readonly IDictionary<string,string > globals = new Dictionary<string, string>();
        private IList<Substitution> substitutions = new List<Substitution>();

        public SmartXml CollectGlobals(){
            var globalelements = Element.XPathSelectElements("./global").ToArray();
            foreach (var globalelement in globalelements){
                globalelement.Remove();
                var id = globalelement.attr("id").ToUpper();
                //это позволяет сохранить перекрытие файлами (USR при обходе файлов оказывается сверху)
                //а также позволяет указать значения глобалок до процессинга с их перекрытием
                if(globals.ContainsKey(id))continue;
                var value = globalelement.attr("value");
                if(string.IsNullOrWhiteSpace(value)){
                    value = globalelement.Value;
                }
                globals[id] = value;
                

            }
            return this;
        }

        public SmartXml NormalizeGlobals(){
            foreach (var key in new List<string>(globals.Keys)){
                globals[key] = ResolveGlobalsInString(globals[key]);
            }
            return this;
        }

        public SmartXml ApplyGlobalsToElement(){
            ApplyGlobalsToElement(Element);
            return this;
        }

        public SmartXml ProcessEmbeds(){
            Element.processIncludes("embed", Element);
            return this;
        }

        public SmartXml CollectSubstitutions(){
            substitutions.addRange(Element.Elements("subst").ToArray().Select(
                x =>{
                    x.Remove();
                    var result = new Substitution();
                    x.applyTo(result);
                    return result;
                }
                ));
            return this;
        }
        public SmartXml ApplySubstitutions(){
             foreach (var substitution in substitutions){
                var targets = Element.XPathSelectElements(substitution.From).ToArray();
                foreach (var target in targets){
                    var id = target.attr("id");
                    var newval = target.Value;
                    var xpath = substitution.To + "[@id='" + id + "']";
                    XElement source = null;
                    if (substitution.UseFirst) {
                       source = Element.XPathSelectElements(xpath).FirstOrDefault();
                    }
                    else{
                        source =  Element.XPathSelectElements(xpath).LastOrDefault();
                    }
                 
                    if (null == source){
                        throw new Exception("cannot substitute " + target.ToString().Replace("<", "&lt;"));
                    }
                    if (string.IsNullOrWhiteSpace(substitution.As)){
                        substitution.As = source.Name.LocalName;
                    }
                    if (substitution.Elements){
                        target.ReplaceWith(source.Elements());
                    }
                    else{
                        var newe = new XElement(substitution.As);
                        foreach (var attr in source.Attributes()){
                            newe.Add(attr);
                        }
                        foreach (var node in source.Nodes()){
                            newe.Add(node);
                        }
                        if (!String.IsNullOrEmpty(newval)){
                            newe.Value = newval;
                        }
                        foreach (var attribute in target.Attributes()){
                            if (attribute.Name.LocalName.isIn("From", "To", "As", "Elements")){
                                continue;
                            }
                            newe.SetAttributeValue(attribute.Name, attribute.Value);
                        }
                        target.ReplaceWith(newe);
                    }
                }
            }
            return this;
        }
        
        public SmartXml ApplyGenerators(){
            
             var gens = Element.XPathSelectElements(".//generator").ToArray();
            foreach (var gen in gens){
                try
                {
                    var cls = gen.attr("id");
                    var generator = cls.toType().create<IXmlGenerator>();
                    var production = generator.Generate(gen);
                    if (production == null)
                    {
                        gen.Remove();
                    }
                    else
                    {
                        gen.ReplaceWith(production);
                    }
                }catch(Exception exception)
                {
                    if(IgnoreGeneratorErrors)
                    {
                        gen.Add(new XElement("error", new XText(exception.ToString()),new XAttribute("iserror",1)));
                    }else
                    {
                        throw;
                    }
                }
            }
            return this;
        }

        public bool IgnoreGeneratorErrors { get; set; }


        public SmartXml ApplyGlobalsToElement(XElement e){
            foreach (var s in e.Attributes()){
                if (e.Name.LocalName == "global" && (s.Name.LocalName == "id" || s.Name.LocalName == "code")){
                    continue;
                }
                s.Value = ResolveGlobalsInString(s.Value);
            }
            foreach (var text in e.Nodes().OfType<XText>()){
                text.Value = ResolveGlobalsInString(text.Value);
            }


            foreach (var element in e.Elements()){
                ApplyGlobalsToElement(element);
            }
            return this;
        }

        public string ResolveGlobalsInString(string value){
            if (-1 == value.IndexOf("_")) return value;
            return value.replace(@"(?-i)@?((\p{Lu}|_)[\p{Lu}\d_]+)",
                                 m =>{
                                     var name = m.Groups[1].Value;
                                     if (globals.ContainsKey(name)){
                                         return globals[name];
                                     }
                                     return m.Value;
                                 });
        }


        public SmartXml Process(){
            CollectGlobals();
            NormalizeGlobals();
            ApplyGlobalsToElement();
            CollectSubstitutions();
            ApplySubstitutions();
            ResolveElements();
            ApplyGenerators();
            ProcessEmbeds();
            return this;
        }

        public void RemoveDuplicates() {
            foreach (var e in this.Element.Elements().ToArray()) {
                if(null!=e.ElementsAfterSelf().FirstOrDefault(x=>x.Name.LocalName==e.Name.LocalName && x.attr("id","__x__")==e.attr("id","__e__"))) {
                    e.Remove();
                }
            }
        }

        public void ResolveElements() {
            var resolvers = Element.XPathSelectElements(".//resolve").ToArray();
            foreach (var resolver in resolvers.ToArray()) {
                var xpath = resolver.attr("code");
                var name = resolver.attr("name");
                var attrs = resolver.Attributes().Where(x => !x.Name.LocalName.isIn("id", "code", "name")).ToArray();
                var elements = Element.XPathSelectElements(xpath);
                foreach (var element in elements.ToArray()) {
                    var newe = new XElement(element);
                    newe.Name = name;
                    foreach (var attr in attrs) {
                        var n = attr.Name.LocalName.Replace("new.", "");
                        var v = attr.Value;
                        if(v.StartsWith("@")) {
                            v = element.attr(v.Substring(1));
                        }
                        newe.SetAttributeValue(n, v);
                    }
                    element.ReplaceWith(newe);
                }
                resolver.Remove();
            }
        }

        public void BindTo(IDictionary<string, string> target) {
            foreach (var element in this.Element.Elements()) {
                var code = element.attr("id");
                if(code.noContent()) {
                    code = element.attr("code");
                }
                if(code.noContent()) {
                    code = element.Name.LocalName;
                }
                var value = element.attr("value");
                if(value.noContent()) {
                    value = element.Value;
                }
                target[code] = value;
            }
        }

        public void BindTo<T>(IDictionary<string, T> target)where T:class,new()
        {
            foreach (var element in this.Element.Elements())
            {
                var code = element.attr("id");
                if (code.noContent())
                {
                    code = element.attr("code");
                }
                if (code.noContent())
                {
                    code = element.Name.LocalName;
                }
                var value = new T();
                element.applyTo(value);
                target[code] = value;
            }
        }
    }
}
