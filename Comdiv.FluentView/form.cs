using System.IO;
using Castle.MonoRail.Views.Brail;

namespace Comdiv.FluentView{
    public class form : genericNode<form>
    {
        public form() {}

        public form(params string[] id_and_classes) : base(id_and_classes) {}

        public form(nodeBase genericNode, params string[] id_and_classes) : base(genericNode, id_and_classes) {}

        public form(BrailBase view, params string[] id_and_classes) : base(view, id_and_classes) {}

        public form(TextWriter outWriter, params string[] id_and_classes) : base(outWriter, id_and_classes) {}

        public form(nodeBase genericNode, TextWriter outWriter, params string[] id_and_classes) : base(genericNode, outWriter, id_and_classes) {}

        protected override void init()
        {
            
            base.init();
            named("form");
            var a = get;
        }

        public form action(string href)
        {
            this.attributes["action"] = href;
            return this;
        }
        public form post
        {
            get{
                this.attributes["method"] = "post";
                return this;
            }
        }
        public form get
        {
            get
            {
                this.attributes["method"] = "get";
                return this;
            }
        }
        
        public form target(string target)
        {
            if (string.IsNullOrEmpty(target)) return this;
            attr("target", target);
            return this;
        }
        public form newpage()
        {

            attr("target", "_blank");
            return this;
        }

    }
}