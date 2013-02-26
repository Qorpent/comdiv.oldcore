using System;
using System.Linq;
using System.Threading;
using System.Web;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Controllers{
    public static class AppRestartTimer{
        private static Timer timer;
        public static bool Started { get; private set; }
        public static DateTime RestartTime { get; private set; }

        public static void Start(int minutes){
            if (timer != null){
                timer.Dispose();
            }
            timer = null;
            logger.get("comdiv.sys").Warn("Delay restart initiated " + minutes);

            Started = true;
            RestartTime = DateTime.Now.AddMinutes(minutes);
            timer = new Timer(x =>{
                                  logger.get("comdiv.sys").Warn("Delay restart called");
                                //  myapp.UserData.DoAutoSave();
                                  
                                  HttpRuntime.UnloadAppDomain();
                                
                              }, null, TimeSpan.FromMinutes(minutes), TimeSpan.FromMilliseconds(-1));
        }

        public static void Stop(){
            if (null != timer){
                timer.Dispose();
                timer = null;
            }
            logger.get("comdiv.sys").Warn("Delay restart stopped");
            Started = false;
        }
    }
}