#region

using System;
using System.Linq;
using System.Web;
using Castle.MonoRail.Framework;
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

#endregion

namespace Comdiv.MVC.Utils{
    public class MVCLog{
        private static readonly ILog log = logger.get(typeof (MVCLog));
        private static readonly object sync = new object();

        public static void Write(IController controller, string anevent, string result, string customData){
            var c = controller as Controller;
            var descriptor = MvcContext.Create(((Controller) controller).Context,
                                               ((Controller) controller).ControllerContext,
                                               controller);
            descriptor.Category = anevent;
            // var allowLog = ControllerExpert.Run("log", true, descriptor).ToBoolean();
            // if (allowLog){
            var newlog = new LogItem();
            newlog.Event = anevent;
            newlog.CustomData = customData;
            newlog.Result = result;
            newlog.Area = c.AreaName;
            newlog.Action = c.Action;
            newlog.Controller = c.Name;
            newlog.Time = DateTime.Now;
            newlog.Usr = myapp.usrName;
            var p =
                string.Join("&", new[]{c.Form.ToString(), c.Query.ToString()});
            if (p.StartsWith("&")){
                p = p.Remove(0, 1);
            }
            newlog.Params = p;
            newlog.RequestTime = HttpContext.Current.Timestamp;


            if (anevent.Contains("error")){
                log.Warn(newlog.ToString());
            }
            else if (anevent.Contains("start")){
                log.Info(newlog.ToString());
            }
            else{
                log.Debug(newlog.ToString());
            }
            //   }
        }

    }
}