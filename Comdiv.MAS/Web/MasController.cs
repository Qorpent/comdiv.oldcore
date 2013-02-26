using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Web;
using Comdiv.Controllers;
using Comdiv.Extensions;
using Comdiv.Framework.Utils;
using Comdiv.MAS.Model;
using Comdiv.Persistence;
using Comdiv.Security;

namespace Comdiv.MAS
{

    [Role("MASMANAGER")]
    public class MasController:BaseController
    {
        public IMasProcessStarter ProcessStarter { get; set; }
        public IMasProcessRepository Repository { get; set; }

        public void executecommand(string app, string command) {
            var oldclallback = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback =
            new RemoteCertificateValidationCallback(
                delegate
                { return true; }
            );
            var args = ConvertPrefixedParametersToDict("p_");
            var url = Repository.Storage.Load<App>(app).getWebCommand(command, args);
            var wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Credentials =  CredentialCache.DefaultCredentials;
            var result = wc.DownloadString(url);
            RenderText(result);
            ServicePointManager.ServerCertificateValidationCallback = oldclallback;
        }
        public void run(string name, string args, string targetapp) {
            if (targetapp.hasContent()) {
                var me = Repository.MyApp();
                if(me.Name=="NONE")throw new Exception("cannot run targeted and maybe remote application from anonymous MAS host");

                var target = Repository.Storage.Load<App>(targetapp);
                if(target==null)throw new Exception("cannot load app with code "+targetapp);

                if(target.Server.Id!=me.Server.Id) {
                    //производится удаленный запуск
                    App sadm = null;
                    foreach(var app in target.Server.Applications) {
                        if(app.Type.Code==me.Type.Code) {
                            sadm = app;
                            break;
                            
                        }
                    }
                    if(null==sadm) {
                        throw new Exception("cannot find app with type " + me.Type.Code + " on server " +
                                            target.Server.Code);
                    }
                    remoterun(name, args,sadm,target);
                }else {
                    localrun(name,args);
                }
            }else {
                localrun(name, args);
            }
        }

        private void remoterun(string name, string args,App sadm, App target) {
            var url = sadm.getRootUrl()+"/mas/run.rails?name="+name+"&args="+args+"&targetapp="+target.Code;
            var wcl = new WebClient();
            wcl.Credentials = CredentialCache.DefaultCredentials;//new NetworkCredential("sfo", "zaq1!QAZ", "ugmk");
            wcl.Encoding = Encoding.UTF8;
            var result = wcl.DownloadString(url);
            RenderText("r_"+target.Code+"_"+result);
        }

        private void localrun(string name, string args) {
            var info = new MasProcessStartInfo {Name = name, Args = args};
            info = ProcessStarter.Run(info);
            RenderText(info.Code);
        }

        public void send(string type, string message, int id, string  sender) {
            sender = sender.hasContent() ? sender : "mas/send" ;
            sender = Environment.MachineName + "::" + Request.ApplicationPath + "::" +sender;
            var pm = new ProcessMessage();
            pm.Message = message;
            pm.Type = type;
            pm.Process = Repository.Get(new Process {Id = id});
            pm.Sender = sender;
            pm = Repository.Send(pm);
            RenderText(pm.Id.ToString());
        }


        public void myapp() {
            PropertyBag["app"] = Repository.MyApp();
            RenderText(Repository.MyApp().toJSON());
        }
        


        
        public void getstate(string processcode) {
            if (processcode.StartsWith("r_")) {
                var me = Repository.MyApp();
                var targetapp = processcode.find(@"^r_([\w]+?)_", 1);
                var target = Repository.Storage.Load<App>(targetapp);
                var sadm = target.Server.Applications.First(x => x.Type.Code == me.Type.Code);
                var url = sadm.getRootUrl() + "/mas/getstate.rails?processcode=" +
                          processcode.find(@"^r_[\w]+?_([\s\S]+)$", 1);
                var wcl = new WebClient();
                wcl.Credentials = CredentialCache.DefaultCredentials;
                wcl.Encoding = Encoding.UTF8;
                var result = wcl.DownloadString(url);
                RenderText(result);

            }   
            else {
                getlocalstate(processcode);
            }
        }

        private void getlocalstate(string processcode) {
            var process = Repository.Get(new Process {Code = processcode});
            if(null==process) {
                RenderText("{}");
                return;
                ;
            }
            RenderText(process.toJSON());
        }

        public void clean(int result, bool active, string resultcondition,string name, string host, string args, string time, string timecondition) {
            if (resultcondition.noContent()) resultcondition = "Eq";
            if (timecondition.noContent()) timecondition = "None";
            var query = new Process
                            {
                                IsActiveCondition = active ? Condition.None : Condition.Eq,
                                ResultCondition = (Condition) Enum.Parse(typeof (Condition),
                                                                         resultcondition, true),
                                Result = result,
                                
                                Host = host,
                                Args = "%" + args + "%",
                            };
            if(name.hasContent()) {
                query.Name = "%" + name + "%";
            }
            if(args.hasContent()) {
                query.Args = "%" + args + "%";
            }
            if ((Condition)Enum.Parse(typeof(Condition),timecondition, true) != Condition.None) {
                query.StartTimeCondition = (Condition) Enum.Parse(typeof (Condition), timecondition, true);
                query.StartTime = time.toDate();
            }
            Repository.Clean(query);
            RenderText("OK");
        }



        }
}
