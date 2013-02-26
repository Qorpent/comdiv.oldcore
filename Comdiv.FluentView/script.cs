using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class script:genericNode<script>
    {
        public script() {}
        public script(params string[] id_and_classes) : base(id_and_classes) {}
        public script(nodeBase genericNode, params string[] id_and_classes) : base(genericNode, id_and_classes) {}
        public script(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}
        public script(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}
        public script(nodeBase genericNode, TextWriter outWriter, params string[] id_and_classes) : base(genericNode, outWriter, id_and_classes) {}
        protected override void init()
        {
            base.init();
            named("script").attr("type", "text-javascrip");
        }
    }
}