using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class table:genericNode<table>{
        public table() {}

        public table(params string[] id_and_classes) : base(id_and_classes) {}

        public table(nodeBase genericNode, params string[] id_and_classes) : base(genericNode, id_and_classes) {}

        public table(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}

        public table(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}

        public table(nodeBase genericNode, TextWriter outWriter, params string[] id_and_classes) : base(genericNode, outWriter, id_and_classes) {}

        protected override void init()
        {
            this.named("table");
            add(new tpart(this).named("thead"));
            add(new tpart(this).named("tbody"));
        }
        public tpart header{
            get { return (tpart) children[0]; }
        }
        public tpart body
        {
            get { return (tpart)children[1]; }
        }

        public Row row{
            get{
                return body.row;
            }
        }
    }
}