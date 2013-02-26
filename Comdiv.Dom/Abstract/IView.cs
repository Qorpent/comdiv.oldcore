namespace Comdiv.Dom {
    public interface IView : INode {
        string ViewName { get; set; }
        string Param { get; set; }
    }
}