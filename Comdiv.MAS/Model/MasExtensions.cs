using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Extensions;
using Qorpent.Bxl;

namespace Comdiv.MAS.Model
{
    public static class MasExtensions
    {
        public static string getConfigParameter(this App app,string name) {
            var cp = app.CachedConfig ?? ( app.CachedConfig = new BxlParser().Parse(app.Config.Trim(),""));
            var e = cp.Element(name);
            string result = null;
            if(null!=e) {
                result = e.idorvalue().toStr();
            }
            if(null==result) {
                result = app.Server.getConfigParameter(name);
            }
            if(null==result) {
                result = app.Type.getConfigParameter(name);
            }
            return result;
        }

        public static string getConfigParameter(this Server server,string name) {
			var cp = server.CachedConfig ?? (server.CachedConfig = new BxlParser().Parse(server.Config.Trim(),""));
            var e = cp.Element(name);
            if(null==e) {
                return null;
            }
            return e.idorvalue().toStr();
        }
        public static string getConfigParameter(this AppType type,string name) {
			var cp = type.CachedConfig ?? (type.CachedConfig = new BxlParser().Parse(type.Config.Trim(),""));
            var e = cp.Element(name);
            if(null!=e) {
                return e.idorvalue().toStr();
            }
            if(null!=type.Parent) {
                return type.Parent.getConfigParameter(name);
            }
            return null;
        }

        public static string getRootUrl(this App app) {
            
            var localping = app.getConfigParameter("localping");
            var url = app.Server.LNetUrl;
            if(app.getConfigParameter("machinename")== Environment.MachineName) {
                var uri = new Uri(url);

                url = uri.Scheme + "://localhost:" + uri.Port;
            }else
            if(localping.hasContent()) {
                var ping = new Ping();
                var reply= ping.Send(localping, 300);
                if(reply.Status!=IPStatus.Success) {
                    url = app.Server.INetUrl;
                }
            }
            return url + app.getConfigParameter("appname");
        }

        public static string getFullCommands(this AppType type) {

            var result = type.Commands ?? "";
            if(type.Parent!=null) {
                result = type.Parent.getFullCommands() + Environment.NewLine + result;
            }
            return result;
        }

        public static XElement getCommand(this AppType type, string  command) {
            var cc = type.CachedCommands ??
					 (type.CachedCommands = new BxlParser().Parse((type.getFullCommands() ?? "").Trim(),""));
            var e = cc.XPathSelectElements("//command[@code='" + command + "']").LastOrDefault();
            if(null==e) {
                throw  new Exception("cannot find command "+command+" in type "+type.Code);
            }
            return e;
        }

        public static string getWebCommand(this App app , string  command, IDictionary<string ,string > parameters) {
            XElement xcommand = app.Type.getCommand(command);
            var url = app.getRootUrl()+"/"+xcommand.attr("action")+".rails?";
            foreach (var parameter in parameters) {
                url += parameter.Key + "=" + parameter.Value + "&";
            }
            return url;
        }
    }
}
