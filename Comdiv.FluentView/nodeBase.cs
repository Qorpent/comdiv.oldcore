using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class nodeBase {
        private IDictionary<string,Func<nodeBase, object[],string>> _snippets;
        private BrailBase _view;
        private HtmlTextWriter _wr;
        private string _prefix;
        public nodeBase() : this(new string[] {}) {}
        public nodeBase(params string[] id_and_classes) : this((nodeBase)null,id_and_classes) { }
        public nodeBase(nodeBase node,params string[] id_and_classes) : this(node,null, id_and_classes) {}

        public nodeBase(BrailBase view, params string[] id_and_classes) : this(null, view.OutputStream, id_and_classes){
            this.view = view;
        }

        public nodeBase(TextWriter outWriter, params string[] id_and_classes) : this(null, outWriter, id_and_classes) {}

        public nodeBase(nodeBase node,TextWriter outWriter, params string[] id_and_classes){
            
            this.parent = node;
            this.writer = new HtmlTextWriter(outWriter);
            
            children = new List<nodeBase>();
            this.name = "span";
            this.snippets = new Dictionary<string, Func<nodeBase, object[],string>>();
            classes = new List<string>();
            attributes = new Dictionary<string, string>();
            SubviewValues = new Dictionary<string, object>();
            if (null != id_and_classes && 0 != id_and_classes.Length){
                List<string> list = id_and_classes.ToList();
                id = id_and_classes[0];
                foreach (string clazz in id_and_classes.Skip(1)){
                    classes.Add(clazz);
                }
                ;
            }
            init();
        }

        public IDictionary<string, object> SubviewValues { get; private set; }

        public IDictionary<string,Func<nodeBase, object[],string>> snippets{
            get { return parent==null? _snippets : parent.snippets; }
            set { _snippets = value; }
        }

        public IList<string> classes { get; private set; }
        public IDictionary<string, string> attributes { get; private set; }
        public string name { get; set; }
        public bool container { get; set; }
        public nodeBase parent { get; set; }
        public string id { get; set; }

        public BrailBase view{
            get { return null==parent?_view:parent.view; }
            set{
                if (null == parent){
                    _view = value;
                }
            }
        }

        public IList<nodeBase> children { get; private set; }

        protected HtmlTextWriter writer{
            get{
                if(null!=parent){
                    return parent.writer;
                }
                return _wr;
            }
            set{
                if(null==parent){
                    this._wr = value;
                }
            }
        }

        public string text { get; set; }

        public string prefix{
            get { return _prefix ?? (null==parent?"":parent.prefix); }
            set { _prefix = value; }
        }

        public object get(string key){
            if (SubviewValues.ContainsKey(key)) return SubviewValues[key];
            if(null==parent) return null;
            return parent.get(key);
        }

        protected IList<string> getAllParameterKeys(){
            return
                this.SubviewValues.Keys.Union(parent == null ? new string[] {} : parent.getAllParameterKeys()).Distinct()
                    .ToList();
        }

        protected virtual void init(){
            
        }

        public nodeBase setSnippet(string name,Func<nodeBase, object[],string> snippet ){
            snippets[name] = snippet;
            return this;
        }

        public nodeBase render(){
            if (null != parent){
                return parent.render();
            }
            return doRender();
        }

        protected virtual nodeBase doRender(){
            try{
                writeStart();
                writeContent();
                writeEnd();
                return this;    
            }catch(Exception ex){
                throw new Exception(
                    string.Format("name: {0}, classes: {1}, type : {2}", name ?? "NULL",
                                  string.Join(" ", classes.ToArray()), this.GetType().Name),
                    ex);
            }
            
        }

        protected virtual void writeEnd(){
            writer.WriteEndTag(name);
        }

        protected virtual void writeContent(){
            if(!string.IsNullOrEmpty(text)){
                writer.WriteLine(text);
            }
            foreach (var item in this.children){
                item.doRender();
            }
        }

        protected virtual void writeStart(){
            writer.WriteBeginTag(name);
            if (!string.IsNullOrEmpty(id)){
                writer.WriteAttribute("id", (prefix ?? "") + id);
            }
            if (classes.Count != 0){
                writer.WriteAttribute("class", string.Join(" ", this.classes.ToArray()));
            }
            foreach (var attribute in attributes){
                writer.WriteAttribute(attribute.Key,attribute.Value,true);
            }
            writer.Write(">");
        }

        public nodeBase add(params nodeBase[] nodesBase)
        {
            var ns = nodesBase ?? new nodeBase[] {};
            foreach (var n in nodesBase){
                n.parent = this;
                this.children.Add(n);    
            }
            if(ns.Length!=1) return this;
            return ns[0];
        }

        public href href(){
            return (href) add( new href(this));
        }

        public table table(params string[] id_and_clases)
        {
            return(table) add( new table(id_and_clases));
        }

        public T up<T>() where T:nodeBase{
            return up<T>(null);
        }

        public T up<T>(string id)where T:nodeBase{
            if(this is T && (string.IsNullOrEmpty(id) || this.id==id)) return (T) this;
            if(null==parent)throw new Exception("cannot up to "+typeof(T).Name+" id:"+id??"NOID");
            return parent.up<T>(id);
        }

        public form form(string id,string action)
        {
            return (form)add(new form(id).action(action));
        }

        public node h(int level)
        {
            return  (node)add(new node().named("h"+level));
            
        }

        public node h2()
        {
            return h(2);
        }

        public node h1()
        {
            return h(1);
        }

        public node h3()
        {
            return h(3);
        }

        public node add(string name){
            var n = new node(this).named(name);
            this.children.Add(n);
            return n;
        }

        public node div(params string[] id_and_clases){
            return (node)add(new node(this,id_and_clases).named("div"));
        }
    }
}