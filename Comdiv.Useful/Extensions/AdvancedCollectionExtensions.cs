using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Extensions;

namespace Comdiv.Useful{
    public static class AdvancedCollectionExtensions{
        public static IList<T> add<T>(this IList<T> src, params T[] item)
        {
            if (null == src) src = new List<T>();
            foreach (var i in item){
                src.Add(i);
            }
            return src;
        }

        public static IList<T> add<T>(this IList<T> src, T item){
            return add(src, item, false);
        }

        public static IList<T> add<T>(this IList<T> src, T item, bool unique){
            if (null == src )  src = new List<T>();
            if(unique && src.Contains(item)) return src;
            src.Add(item);
            return src;
        }

        public static bool containsAny(this IEnumerable source, IEnumerable values)
        {
            if (source == null || values == null) return false;
            return containsAny(source.Cast<object>(), values.Cast<object>());
        }

        public static bool containsAny<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            if (source == null || values == null ) return false;
            foreach (var t in source)
            {
                if( values.Contains(t))return true;
            }
            return false;
        }

        public static bool containsAll(this IEnumerable source, IEnumerable values){

            if (source == null || values == null) return false;
            return EnumerableExtensions.containsAll(source.Cast<object>(), values.Cast<object>());
        }

        public static IEnumerable mapNoGeneric(this IEnumerable set,Action<object> expression){
            if(null==set||expression ==null) return null;
            EnumerableExtensions.map(set.Cast<object>(), expression);
            return set;
        }
    }
}