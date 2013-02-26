using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class input:genericNode<input>{
        public input() {}
        public input(params string[] id_and_classes) : base(id_and_classes) {}
        public input(nodeBase genericNode, params string[] id_and_classes) : base(genericNode, id_and_classes) {}
        public input(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}
        public input(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}
        public input(nodeBase genericNode, TextWriter outWriter, params string[] id_and_classes) : base(genericNode, outWriter, id_and_classes) {}
        protected override void init()
        {
            base.init();
            named("input").attr("type", "text");
            attr("name", id);
        }

        public input disable(bool doDisable){
            if(doDisable){
                return attr("disabled", "disabled");
            }
            return this;
        }
        public input typed(string type){
            return (input) attr("type", type);
            
        }
        public input valued(string value){
            return (input) attr("value", value);
        }
        public input forName(string name)
        {
            return (input)attr("name", name);
        }
    }
}