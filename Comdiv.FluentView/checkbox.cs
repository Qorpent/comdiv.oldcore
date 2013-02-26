using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class checkbox:input{
        public checkbox() {}
        public checkbox(params string[] id_and_classes) : base(id_and_classes) {}
        public checkbox(nodeBase genericNode, params string[] id_and_classes) : base(genericNode, id_and_classes) {}
        public checkbox(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}
        public checkbox(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}
        public checkbox(nodeBase genericNode, TextWriter outWriter, params string[] id_and_classes) : base(genericNode, outWriter, id_and_classes) {}

        protected override void init()
        {
            base.init();
            nocheck().typed("checkbox");
        }
        public checkbox nocheck(){
            return check(false);
        }

        public checkbox check(){
            return check(true);
        }

        public checkbox check(bool ch){
            if(!ch){
                attributes.Remove("checked");
            }else{
                attributes["checked"] = "checked";
            }
            return this;
        }
    }
}