using System;
using System.Diagnostics;
using System.Threading;
using System.Web;
using Comdiv.Logging;

namespace Comdiv.Application{
    public static class AppRestartTimer{
        private static Timer timer;
        public static bool Started { get; private set; }
        public static DateTime RestartTime { get; private set; }

        public static void Start(int minutes){
            if (timer != null){
                timer.Dispose();
            }
            timer = null;

            Started = true;
            RestartTime = DateTime.Now.AddMinutes(minutes);
            timer = new Timer(x =>{

                
                    Process.GetCurrentProcess().Kill();
                
                                
                              }, null, TimeSpan.FromMinutes(minutes), TimeSpan.FromMilliseconds(-1));
        }

        public static void Stop(){
            if (null != timer){
                timer.Dispose();
                timer = null;
            }

            Started = false;
        }
    }
}