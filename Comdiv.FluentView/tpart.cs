using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class tpart:genericNode<tpart>{
        public tpart(nodeBase genericNode, params string[] id_and_classes) : base(genericNode, id_and_classes) {}
        public tpart(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}
        public tpart(nodeBase genericNode, TextWriter outWriter, params string[] id_and_classes) : base(genericNode, outWriter, id_and_classes) {}

        public Row row{
            get { return (Row) add(new Row()); }
        }

        public table table{
            get { return (table) parent; }
        }
    }
}