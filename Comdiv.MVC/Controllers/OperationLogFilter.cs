using System;
using System.Linq;
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

namespace Comdiv.MVC.Controllers{
    public abstract class OperationLogFilter : Filter{

        public OperationLogFilter(){
            this.storage = myapp.storage.Get<IOperationLog>();
        }

        protected const string _stopwatch = "operationlog_stopwatch";
        private static bool waserror;
        protected bool writeAfterAction;
        protected StorageWrapper<IOperationLog> storage;

        protected override bool OnBeforeAction(IEngineContext context, IController controller,
                                               IControllerContext controllerContext){
            if (waserror){
                return true;
            }
            var conversation = myapp.conversation.Current;
            conversation.Data[_stopwatch] = new DateRange(DateTime.Now, DateExtensions.Begin);
            return true;
        }

        protected override void OnAfterAction(IEngineContext context, IController controller,
                                              IControllerContext controllerContext){
            if (!writeAfterAction){
                return;
            }
            doLog(context);
        }

        protected override void OnAfterRendering(IEngineContext context, IController controller,
                                                 IControllerContext controllerContext){
            if (writeAfterAction){
                return;
            }
            doLog(context);
        }

        private void doLog(IEngineContext context){
            if (waserror){
                return;
            }
            try{
                var conversation = myapp.conversation.Current;
                var stopwatch = conversation.Data[_stopwatch] as DateRange;
                stopwatch.Finish = DateTime.Now;
                conversation.Data.Remove(_stopwatch);

                var log = storage.New();
                log.Range = stopwatch;
                log.Elapsed = (int) (stopwatch.Finish - stopwatch.Start).TotalMilliseconds;
                log.Usr = myapp.usrName;
                log.System = context.Request.Uri.GetLeftPart(UriPartial.Path).find(@"^\w+://[^/]+?/\w+");
                log.Url = context.Request.Url;
                prepareLogEntry(log, context);
                using (var s = new TemporaryTransactionSession()){
                    storage.Save(log);
                    s.Commit();
                }
            }
            catch (Exception ex){
                logger.get("comdiv.sys").Error("operation log error", ex);
                waserror = true;
            }
        }

        protected abstract void prepareLogEntry(IOperationLog log, IEngineContext context);
    }
}