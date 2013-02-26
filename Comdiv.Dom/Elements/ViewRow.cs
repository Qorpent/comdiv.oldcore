using Comdiv.Extensions;

namespace Comdiv.Dom {
    public class ViewRow : Row,IView {
        public ViewRow() {
            this.Name = "View";
        }
        public string ViewName
        {
            get { return this.Attributes.get("view"); }
            set { this.Attributes["view"] = value; }
        }

        public string Param
        {
            get { return this.Attributes.get("param"); }
            set { this.Attributes["param"] = value; }
        }
        
    }
}