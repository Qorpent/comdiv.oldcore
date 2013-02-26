namespace Comdiv.FluentView{
    public class content:genericNode<content>{
        protected override nodeBase doRender()
        {
            writer.WriteLine(text);
            return this;
        }
    }
}