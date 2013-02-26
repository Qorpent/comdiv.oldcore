using System;
using System.Linq;
using System.Web;
using Comdiv.Application;
using Comdiv.Cfg;
using Comdiv.Conversations;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC{
    [ShouldUseAsService(typeof (IResqueProvider))]
    public class DefaultRescueProvider : IResqueProvider{
        private readonly bool isDebugEnabled;
        private readonly ILog log = logger.get(typeof (DefaultRescueProvider));


        private readonly IWebEnvironment we;
        public string DefaultHomePage = "index.html";

        public DefaultRescueProvider(IWebEnvironment webEnvironment){
            we = webEnvironment;
            isDebugEnabled = log.IsDebugEnabled;
        }

        #region IResqueProvider Members

        public void Process(Exception ex){
            lock (this){
                var msg = ex.ToString();
                Process(msg);
            }
        }


        public void Process(string message){
            lock (this){
                var homePage = we.GetHomePage();
                if (isDebugEnabled)
                {
                    log.Debug("Process: Home Page is " + homePage);
                }
                // TODO: ошибка 
                HttpContext.Current.Response.Write(
                    string.Format(
                        "<h1>Общая ошибка приложения</h1><h2><a href='{0}/{2}'>Вернуться к стартовой странице</h2><p>{1}</p>",
                        HttpContext.Current.Request.ApplicationPath, message, homePage
                        )
                    );
                HttpContext.Current.Response.End();    
            }
            
        }

        #endregion
    }
}