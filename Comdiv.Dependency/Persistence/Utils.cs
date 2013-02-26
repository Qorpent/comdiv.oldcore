#region

using System.Collections.Generic;
using Comdiv.Extensions;

#endregion

namespace Comdiv.Data
{
    public static class Utils
    {
        public static string NormalPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return string.Empty;
            if (prefix.EndsWith(".")) return prefix;
            return prefix + ".";
        }

        public static string NormalAlias(this string prefix, string alias)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return alias;
            if (!(prefix.EndsWith(".") || prefix.EndsWith("_"))) prefix += "_";
            prefix = prefix.Replace(".", "_");

            return prefix + alias;
        }

        public static string NormalAlias(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return string.Empty;
            prefix = prefix.Replace(".$", "~");
            prefix = prefix.Replace(".", "_");
            if (prefix.EndsWith("_")) prefix = prefix.Substring(0, prefix.Length - 1);
            prefix = prefix.Replace("~", ".");
            return prefix + ".";
        }

        public static decimal avg(IList<decimal> values)
        {
            if (null == values) return 0;
            decimal sum = 0;
            decimal count = 0;
            foreach (decimal value in values)
            {
                sum += value;
                count++;
            }
            if (0 == count) return 0;
            return sum/count;
        }
    }
}