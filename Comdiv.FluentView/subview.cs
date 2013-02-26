using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class subview : genericNode<subview>{
        public subview(string name):this((nodeBase)null){
            this.name = name;
        }
        public subview(string name,IDictionary parameters)
            : this((nodeBase)null)
        {
            this.name = name;
            this.parameters = parameters;
        }
        public subview(nodeBase node, params string[] id_and_classes) : base(node, id_and_classes) {}
        public subview(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}
        public subview(nodeBase node, TextWriter outWriter, params string[] id_and_classes) : base(node, outWriter, id_and_classes) {}
        public IDictionary parameters { get; set; }
        protected override nodeBase doRender()
        {
            writer.Write("<div id=\""+(prefix??"")+name+"_subview\">");
            view.OutputSubView(name, getAllParameters());
            writer.Write("</div>");
            return this;
        }
        public IDictionary getAllParameters(){
            var result = new Hashtable();
            foreach (var key in getAllParameterKeys().Union(null==parameters?new string[]{}:parameters.Keys.Cast<string>()).Distinct()){
                result[key] = get(key);
                if(null!=parameters && parameters.Contains(key)){
                    result[key] = parameters[key];
                }
            }
            return result;
        }
    }
}