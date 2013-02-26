using System;
using System.Data;
using Comdiv.Extensions;

namespace Comdiv.Useful{
    public static class DataReaderFieldAccessExtensions{
        public static object get(this IDataRecord record, String column){
            return get<object>(record, column, true, null);
        }

        public static T get<T>(this IDataRecord record, String column){
            return get(record, column, true, default(T));
        }

        public static T get<T>(this IDataRecord record, String column, T def){
            return get(record, column, true, def);
        }

        public static T get<T>(this IDataRecord record, String column, bool checkStringsForEmpty, T def){
            object value = record[column];

            if (value is DBNull){
                return def;
            }
            if (null == value){
                return def;
            }
            if (value is string && ((string) value).noContent() && checkStringsForEmpty){
                return def;
            }
            return value.to<T>();
        }
    }
}