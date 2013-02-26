using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class div:genericNode<div>{
        public div() {}
        public div(params string[] id_and_classes) : base(id_and_classes) {}
        public div(nodeBase genericNode, params string[] id_and_classes) : base(genericNode, id_and_classes) {}
        public div(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}
        public div(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}
        public div(nodeBase genericNode, TextWriter outWriter, params string[] id_and_classes) : base(genericNode, outWriter, id_and_classes) {}
        protected override void init()
        {
            named("div");
            base.init();
        }
    }
}