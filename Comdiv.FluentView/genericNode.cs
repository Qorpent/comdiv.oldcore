using System.Collections;
using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class genericNode<T> : nodeBase, IGenericNode<T> where T :nodeBase, IGenericNode<T>
    {
        public genericNode() : base() {}
        public genericNode(params string[] id_and_classes) : base(id_and_classes) {}
        public genericNode(nodeBase node,params string[] id_and_classes) : base(node, id_and_classes) {}

        public genericNode(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes){}

        public T set(IDictionary dictionary){
            foreach (var item in dictionary.Keys){
                set(item.ToString(), dictionary[item]);
            }
            return this as T;
        }
        public T set(string key,object value){
            SubviewValues[key] = value;
            return this as T;
        }

        public genericNode(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}

        public genericNode(nodeBase node,TextWriter outWriter, params string[] id_and_classes) : base(node, outWriter, id_and_classes){}

        public T call(string name, params object[] parameters){
            var snippet = snippets[name];
            var text = snippet(this, parameters);
            if (!string.IsNullOrEmpty(text)){
                writer.Write(text);
            }
            return this as T;
        }

        public T named(string name)
        {
            this.name = name;
            return this as T;
        }

        public T isSingle()
        {
            container = false;
            return this as T;
        }

        public T isContainer()
        {
            container = true;
            return this as T;
        }


        public T setClass(string name){
            if(!classes.Contains(name)){
                classes.Add(name);
            }
            return this as T;
        }
        public T attr(string name,object value){
            this.attributes[name] = value==null?"":value.ToString();
            return this as T;
        }

        public T prefixed(string prefix){
            this.prefix = prefix;
            return this as T;
        }

        public T content(string text){
            add(new content {text = text});
            return this as T;
        }

        public T hide(string name,object text){
            add(new input(name).typed("hidden").valued(null==text?"":text.ToString()));
            return this as T;
        }

        public T subview(string name){
            return subview(name,    null);
        }

        public T h3(string text)
        {
            return h(3, text);
        }

        public T h2(string text)
        {
            return h(2, text);
        }

        public T h1(string text){
            return h(1, text);
        }
        public T h(int level,string text){
            h(level).content(text);
            return this as T;
        }

        public T subview(string name, IDictionary parameters){
            this.children.Add(new subview(this){name=name,parameters = parameters});
            return this as T;
        }

        public T href(string to,string content){
            return href(to, "", content);
        }

        public T href(string to, string onclick,string content){
            return href(to, onclick, "", content);
        }
        public T jsref(string js,string content){
            return href("#", js, "", content);
        }
        public T href(string to, string onclick, string target,string content){
            add(new href(this).to(to).onclick(onclick).target(target).content(content));
            return this as T;
        }

        public T empty(params string[] id_and_clases)
        {
            this.div(id_and_clases);
            return this as T;
        }
    }
}