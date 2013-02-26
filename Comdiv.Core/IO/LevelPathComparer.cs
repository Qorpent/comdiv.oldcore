using System.Collections.Generic;
using Comdiv.Extensions;

namespace Comdiv.IO {
    public class LevelPathComparer : IComparer<string> {
        public int Compare(string x, string y) {
            var xpro = x.like(@"[\\/]profile[\\/]");
            var xusr = x.like(@"[\\/]usr[\\/]");
            var xmod = x.like(@"[\\/]mod[\\/]");
            var xext = x.like(@"[\\/]extensions[\\/]");
            var ypro = y.like(@"[\\/]profile[\\/]");
            var yusr = y.like(@"[\\/]usr[\\/]");
            var ymod = y.like(@"[\\/]mod[\\/]");
            var yext = y.like(@"[\\/]extensions[\\/]");

            var xlevel =  (xusr ? (1) : (xmod ? (3) : (5))) - (xext?1:0) - (xpro? 10 : 0);
            var ylevel = (yusr ? (1) : (ymod ? (3) : (5))) - (yext ? 1 : 0) - (ypro ? 10 : 0);

            return xlevel.CompareTo(ylevel);

        }
    }
}