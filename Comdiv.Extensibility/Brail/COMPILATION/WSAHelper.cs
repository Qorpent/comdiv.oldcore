using System;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Brail {
    public static class WSAHelper
    {
        public static bool IsWSA(string code) {
            if(code.like(@"^\s*\#pragma\sboo")) return false;
            if (code.like(@"\bbml\s*:")) return false;
            if(code.like(@"<%[\s\S]+%>")) return true;
            return true;
        }

        public static bool IsDuck(string code) {
            if (code.like(@"^\s*\#pragma\sboo\snoduck")) return false;
            return true;
        }
    }
}