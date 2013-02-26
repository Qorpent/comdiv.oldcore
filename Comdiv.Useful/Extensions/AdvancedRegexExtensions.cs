using System;
using System.Text.RegularExpressions;
using Comdiv._Extensions_Internals;

namespace Comdiv.Useful{
    public static class AdvancedRegexExtensions{
        public static string toXml(this Regex regex, string input)
        {
            return toXml(regex, input, "", "", false);
        }

        public static string toXml(this Regex regex, string input, string rootName, string itemName,
                                   bool useLocation)
        {
            return toXml(regex, input, rootName, itemName, useLocation, 0);
        }



        public static string toXml(this Regex regex, string input, int timeout)
        {
            return toXml(regex, input, null, null, false, timeout);
        }
        public static int DefaultRegexTimeout = 2000;

        public static void safely(this Regex regex, Action<Regex> action)
        {
            regex.safely(DefaultRegexTimeout, action);
        }

        public static void safely(this Regex regex, int timeoutInMilliseconds, Action<Regex> action)
        {
            new TimeoutRun(timeoutInMilliseconds, () => action(regex)).Run();
        }

        public static string toXml(this Regex regex, string input, string rootName, string itemName,
                                    bool useLocation, int timeout)
        {
            string result = null;
            rootName = string.IsNullOrEmpty(rootName) ? "matches" : rootName;
            itemName = string.IsNullOrEmpty(itemName) ? "match" : itemName;
            regex.safely(timeout, r =>
            {
                result = new TextToXmlTransformer(regex, rootName, itemName, useLocation)
                    .TransformToString(input);
            });
            return result;
        }

    }
}