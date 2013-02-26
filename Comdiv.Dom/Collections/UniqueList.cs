using System.Collections.Generic;


namespace Comdiv.Dom.Collections{
    public class UniqueList<T> : List<T>{
        public new void AddRange(IEnumerable<T> items){
            foreach (var item in items)
                Add(item);
        }

        public new virtual void Add(T item){
            if (null == item) return;
            if (!Contains(item))
                base.Add(item);
        }
    }
}