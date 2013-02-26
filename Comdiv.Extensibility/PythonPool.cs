using System.Collections.Generic;
using Comdiv.Logging;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Comdiv.Extensibility{
    public static class PythonPool{
		static ILog log = logger.get("python");
        private static object sync = new object();
        private static readonly IList<ScriptEngine> pool = new List<ScriptEngine>();
        public static ScriptEngine Get()
        {
            lock (sync) {
                if (pool.Count != 0) {
                    var result = pool[0];
                    pool.Remove(result);
              //      log.debug(() => ("python returned from pool"));
                    return result;
                }
            }
            //log.debug(()=>("python engine created"));
            return Python.CreateEngine();
            
        }

        public static void Release(ScriptEngine e)
        {
            lock (sync) {
                if (null != e && !pool.Contains(e)) {
                    //    log.debug(()=>("python engine returned, pool size "+pool.Count));
                    pool.Add(e);
                }
            }
        }

    }
}