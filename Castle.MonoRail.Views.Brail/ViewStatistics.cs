using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Castle.MonoRail.Views.Brail
{

    public class ViewStatisticsCollection {
        IDictionary<string ,ViewStatistics> statistics = new Dictionary<string, ViewStatistics>();
        public void Compiled(string viewname, long ticks) {
            lock(this) {
                checkStatistics(viewname);
                var s = statistics[viewname];
                s.CompileCount += 1;
                s.CompileTicks += ticks;
            }
        }

        private void checkStatistics(string viewname) {
            if(!statistics.ContainsKey(viewname)) {
                statistics[viewname] = new ViewStatistics {ViewName = viewname};
            }
        }

        public void Rendered(string viewname, bool fromcache, long ticks) {
            lock (this)
            {
                checkStatistics(viewname);
                var s = statistics[viewname];
                s.RenderCount += 1;
                s.RenderTicks += ticks;
                if(fromcache) {
                    s.CacheRenderCount += 1;
                }
            }
        }

        public void Clear() {
            statistics.Clear();
        }

        public ViewStatistics[] GetAll() {
            return statistics.Values.ToArray();
        }
        public ViewStatistics GetTotal() {
            lock (this) {
                var stat = new ViewStatistics();
                stat.ViewName = "[total]";
                foreach (var v in statistics.Values) {
                    stat.CompileCount += v.CompileCount;
                    stat.CompileTicks += v.CompileTicks;
                    stat.RenderCount += v.RenderCount;
                    stat.RenderTicks += v.RenderTicks;
                    stat.CacheRenderCount += v.CacheRenderCount;
                }
                return stat;
            }
        }

        public void IsBoo (string viewname) {
            lock (this)
            {
                checkStatistics(viewname);
                var s = statistics[viewname];
                s.IsBoo = true;
            }
        }
    }

    public class ViewStatistics
    {
        public string ViewName { get; set; }
        public int CompileCount { get; set; }
        public long CompileTicks { get; set; }
        public int RenderCount { get; set; }
        public long RenderTicks { get; set; }
        public int CacheRenderCount { get; set; }

        public bool IsBoo { get; set; }
    }
}
