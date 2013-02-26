namespace Comdiv.Dom {
    public class Img : Node
    {
        public string Href
        {
            get { return Attributes["href"]; }
            set { Attributes["href"] = value; }
        }
    }
}