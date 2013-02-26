using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Comdiv.Extensions;

namespace Comdiv.ConsoleUtils {
    /// <summary>
    /// преобразует входную строку параметров консоли в словарь имя->значение
    /// </summary>
    public class ConsoleArgumentParser {
        private const string Pattern = @"(--(?<name>\S+))((?<value>[\s\S]+?)(?=((--)|($))))?";
        public  X ToObject<X>(string [] args) where X:class,new() {
            return Parse(args.concat(" ")).toObject<X>();
        }
        public IDictionary<string ,string >Parse(string [] args) {
            return Parse(args.concat(" "));
        }
        public IDictionary<string, string> Parse(string args)
        {
            var rx = new Regex(Pattern);
            var matches = rx.Matches(args);
            var d = new Dictionary<string, string>();

            foreach (var results in from Match match in matches select match.Groups) {
                d.Add(results["name"].Value, results["value"].Value);
            }

            foreach (var key in d.Keys.ToArray()) {
                var value = d[key];
                value = value.Trim();
                value = Regex.Replace(value, @"^""([\S\s]*)""$", "$1");
                value = value.Replace("~~", "--");
                value = value.Trim();
                d[key] = value;
            }
            return d;
        }
    }
}