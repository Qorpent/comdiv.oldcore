using Comdiv.Extensions;

namespace Comdiv.Dom {
    public class View : Node, IView {
        public string ViewName {
            get { return this.Attributes.get("view"); }
            set { this.Attributes["view"] = value; }
        }

        public string Param {
            get { return this.Attributes.get("param"); }
            set { this.Attributes["param"] = value; }
        }
    }
}