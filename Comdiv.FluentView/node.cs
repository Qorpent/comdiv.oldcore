using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class node : genericNode<node>{
        public node() {}
        public node(params string[] id_and_classes) : base(id_and_classes) {}
        public node(nodeBase node, params string[] id_and_classes) : base(node, id_and_classes) {}
        public node(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}
        public node(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}
        public node(nodeBase node, TextWriter outWriter, params string[] id_and_classes) : base(node, outWriter, id_and_classes) {}
    }
}