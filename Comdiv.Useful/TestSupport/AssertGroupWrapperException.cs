using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Comdiv.Test.Extensions{
    internal static class importHelperSubstitutes{
        public static string rReplace(this string str, string pattern, string replacer){
            return Regex.Replace(str, pattern, replacer, RegexOptions.Compiled);
        }
    }

    public class AssertGroupWrapperException : Exception{
        public AssertGroupWrapperException(string message) : base(message) {}

        public AssertGroupWrapperException(IEnumerable<Exception> exceptions)
            : this(string.Join("\r\n", exceptions.Select(e => e.ToString()
                                                                  .rReplace(@"\s+в\sNUnit[^\r\n]+", "")
                                                                  .rReplace(@"[\r\n]+[^\r\n]+\.FixtureExtensions\.test[^\r\n]+","")
                                                                  .rReplace(@"в\s+(\w:\\)", "\r\n$1")
                                                                  .rReplace(@"(\.cs):строка\s(\d+)", "$1($2,0)")).
                                           ToArray())) {}

        public AssertGroupWrapperException(string message, Exception inner) : base(message, inner) {}
        protected AssertGroupWrapperException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}