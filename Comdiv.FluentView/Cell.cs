namespace Comdiv.FluentView{
    public class Cell :genericNode<Cell>{
        public Cell cell(params string[] conentAndClasses){
            return ((Row) parent).cell(conentAndClasses);
        }
        public Cell cell(nodeBase child,params string[] conentAndClasses)
        {
            return ((Row)parent).cell(child,conentAndClasses);
        }
        public Cell head(params string[] conentAndClasses)
        {
            return ((Row)parent).head(conentAndClasses);
        }
        public Cell colspan (int span){
            return this.attr("colspan", span);
        }
        public Cell rowspan(int span)
        {
            return this.attr("rowspan", span);
        }
        public Row row
        {
            get { return ((tpart) parent.parent).row; }
        }
        public tpart header
        {
            get { return ((table)parent.parent.parent).header; }
        }
        public tpart body
        {
            get { return ((table)parent.parent.parent).body; }
        }
    }
}