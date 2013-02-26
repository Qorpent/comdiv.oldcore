using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
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

namespace Comdiv.MVC{
    /// <summary>
    /// ¬едет реестр выданых билетов, блокирует перенос билетов между запусками приложени€
    /// </summary>
    public class AppTicketRegistry : IHttpModule{
        private static readonly IList<string> currentApplicationTickets = new List<string>();
        private static readonly object sync = new object();

        #region IHttpModule Members

        public void Init(HttpApplication context){
            context.AuthenticateRequest += _AuthenticateRequest;
        }

        public void Dispose() {}

        #endregion

        public static void Clear(){
            lock (sync){
                currentApplicationTickets.Clear();
            }
        }

        private void _AuthenticateRequest(object sender, EventArgs e){
            lock (sync){
                var app = sender as HttpApplication;
                if (app.Request.Url.AbsolutePath.ToLower() ==
                    FormsAuthentication.LoginUrl.ToLower()){
                    return;
                }
                if (app.Context.User == null){
                    app.Response.Redirect(FormsAuthentication.LoginUrl);
                }
                var cookieName = FormsAuthentication.FormsCookieName;
                var id = HttpContext.Current.User.Identity as FormsIdentity;
                if (null == id){
                    return;
                }
                var authCookie = app.Request.Cookies[cookieName];
                if (!currentApplicationTickets.Contains(authCookie.Value)){
                    app.Response.Redirect(FormsAuthentication.LoginUrl);
                }

                var ticket = id.Ticket;
                var userData = ticket.UserData;
                var roles = userData.Split(',');
                HttpContext.Current.User = new GenericPrincipal(id, roles);
            }
        }
    }
}