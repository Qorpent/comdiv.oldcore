using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Extensions{
    public static class SlashListHelper{
        public static IEnumerable<string> ReadList(string src){
            if(null==src) return new string[]{};
            return src.split(false, true, '/',';').Distinct().ToArray();
        }
		private static string getDelimiter(string src) {
			if (src.Contains(";")) return ";";
			return "/";
		}
        public static string SetMark(string src, string mark){
            if(src==null) src = "";
        	var delimiter = getDelimiter(src);
            RemoveMark(src, mark);
            src += delimiter + mark + delimiter;
            src = src.Replace(delimiter+delimiter, delimiter);
            return src;
        }
        public static string RemoveMark(string src, string  mark){
            if(string.IsNullOrWhiteSpace(src)) return "";
        	var delimiter = getDelimiter(src);
            return src.Replace(delimiter + mark + delimiter, delimiter).Replace(delimiter+delimiter, delimiter);
        }
        public static bool HasMark(string src, string  mark){
            if(string.IsNullOrWhiteSpace(src)||string.IsNullOrWhiteSpace(mark)) return false;
        	var delimiter = getDelimiter(src);
            return src.Contains(delimiter + mark + delimiter);
        }

        public static string ToString(IEnumerable<string> strings){
            if(null==strings||strings.Count()==0) return "";
            return ("/" + strings.Where(x=>!string.IsNullOrWhiteSpace(x)).Distinct().concat("/") + "/").Replace("//", "/");
        }
    }
}