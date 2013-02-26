using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class href : genericNode<href>{
        public href(nodeBase node, params string[] id_and_classes) : this(node,null, id_and_classes) {}
        public href(BrailBase view, params string[] id_and_classes) : this(null,view.OutputStream, id_and_classes) {}
        public href(nodeBase node, TextWriter outWriter, params string[] id_and_classes)
            : base(node, outWriter, id_and_classes)
        {
            name = "a";
            to("#");
        }
        public href to(string href){
            this.attributes["href"] = href;
            return this;
        }
        public href onclick(string js){
            if (string.IsNullOrEmpty(js)) return this;
            to("#").attr("onclick", js + ";return false;");
            return this;
        }
        public href target(string target){
            if (string.IsNullOrEmpty(target)) return this;
            attr("target", target);
            return this;
        }
        public href newpage()
        {

            attr("target", "_blank");
            return this;
        }

    }
}