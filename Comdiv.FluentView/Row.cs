using System.Linq;

namespace Comdiv.FluentView{
    public class Row:genericNode<Row>
    {
        protected override void init()
        {
            base.init();
            named("tr");
        }
        public Row row(){
            return ((tpart)parent).row;
        }
        public Cell cell(params string[] conentAndClasses){
            return addCell("td", conentAndClasses);
        }
        public Cell cell(nodeBase childNodeBase,params string[] conentAndClasses)
        {
            var result = addCell("td", conentAndClasses);
            result.add(childNodeBase);
            return result;
        }
        public Cell head(params string[] conentAndClasses)
        {
            return addCell("th", conentAndClasses);
        }
        protected  Cell addCell(string type,params string[] conentAndClasses){
            var p = conentAndClasses ?? new string[] {};
            var c = (Cell) new Cell().named(type);
            if(p.Length!=0){
                c.content(p[0]);
            }
            foreach (var clazz in p.Skip(1)){
                c.setClass(clazz);
            }
            this.add(c);
            return c;
        }
    }
}