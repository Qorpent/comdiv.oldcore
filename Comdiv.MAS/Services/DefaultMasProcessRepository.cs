using System;
using System.Linq;
using System.Web;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.MAS.Model;
using Comdiv.Persistence;
using NHibernate.Criterion;

namespace Comdiv.MAS {
    [SetupProvider(typeof(DefaultRepositorySetupProvider))]
    public class DefaultMasProcessRepository:IMasProcessRepository {
        private StorageWrapper<Process> _storage;

  

        public StorageWrapper<Process> Storage {
            get { return _storage ?? (_storage = myapp.storage.Get<Process>(Configuration.System,true)); }
            protected set { _storage = value; }
        }

        public MasConfiguration Configuration { get; set; }

        public T Update<T>(T target) {
            //using (new TemporarySimpleSession(this.Configuration.System)) {
                this.Storage.Save(target);
                return target;
            //}
        }

        private App _cachedapp = null;
        public App MyApp () {
            if(null==_cachedapp) {
                if(HttpContext.Current!=null) {
                    var allapps = Storage.All<App>();
                    foreach (var app in allapps) {
                        if (Environment.MachineName != app.getConfigParameter("machinename")) continue;
                        if (HttpContext.Current.Request.ApplicationPath!= app.getConfigParameter("appname")) continue;
                        _cachedapp = app;
                        break;

                    }
                }
            }
            if(null==_cachedapp) {
                _cachedapp = new App {Code = "NONE", Name = "NONE"};
            }
            return _cachedapp;
        }

        public Process[] Query(Process query) {
          //  using (new TemporarySimpleSession(this.Configuration.System)) {
                var crit = DetachedCriteria.For<Process>();
                if (query.Code.hasContent()) {
                    crit.Add(Restrictions.Like("Code", query.Code));
                }
                if (query.Name.hasContent()) {
                    crit.Add(Restrictions.Like("Name", query.Name));
                }
                if (query.Args.hasContent())
                {
                    crit.Add(Restrictions.Like("Args", query.Args));
                }
                if (query.Host.hasContent()) {
                    crit.Add(Restrictions.Eq("Host", query.Host));
                }
                if (query.Pid != 0) {
                    crit.Add(Restrictions.Eq("Pid", query.Pid));
                }
                
                if (query.State.hasContent()) {
                    crit.Add(Restrictions.Like("State", query.State));
                }
                crit.setByCondition(query.IsActiveCondition, "IsActive", query.IsActive);
                crit.setByCondition(query.ResultCondition, "Result", query.Result);
                crit.setByCondition(query.StartTimeCondition, "StartTime", query.StartTime);
                return Storage.Query(crit).ToArray();
            //}
        }

        public void Clean(Process query) {
            var processestodelete = Query(query);
            foreach (var process in processestodelete) {
                Storage.Delete(process);
            }
        }

        public Process Get(Process query) {
            object key = query.Id != 0 ? (object) query.Id : query.Code;
            return Storage.Load(key);
        }

        public Process Start(Process processinfo) {
            //using (new TemporarySimpleSession(this.Configuration.System)) {
                processinfo.StartTime = DateTime.Now;
                processinfo.EndTime = DateExtensions.End;
                processinfo.IsActive = true;
                processinfo.Result = -10;
                processinfo.ResultDescription = "in execution";
                processinfo.State = "started";
                Storage.Save(processinfo);
                return processinfo;
        //    }
        }

        public Process Refresh(Process process) {
            return Get(process);
        }

        public ProcessMessage Refresh(ProcessMessage message) {
            //using (new TemporarySimpleSession(this.Configuration.System)) {
                return Storage.Load<ProcessMessage>(message.Id);
           // }
        }

        public void Finish(Process processinfo) {
            //using (new TemporarySimpleSession(this.Configuration.System)) {
                var realinfo = Get(processinfo);
                realinfo.EndTime = DateTime.Now;
                processinfo.EndTime = DateTime.Now;
                processinfo.IsActive = false;
                realinfo.Result = processinfo.Result;
                realinfo.ResultDescription = processinfo.ResultDescription;
                realinfo.IsActive = false;
                Storage.Save(realinfo);
            //}
        }

        public ProcessMessage Send(ProcessMessage messageinfo) {
            //using (new TemporarySimpleSession(this.Configuration.System)) {
                messageinfo.SendTime = DateTime.Now;
                Storage.Save(messageinfo);
                return messageinfo;
           // }
        }

        public ProcessLog Log(ProcessLog processLog) {
           // using (new TemporarySimpleSession(this.Configuration.System)) {
                if (((int) processLog.Type) >= (int) Configuration.DefaultLogLevel) {
                    //using (new TemporarySimpleSession(this.Configuration.System)) {
                    //processLog.Process = Storage.Load(processLog.Process.Id);
                        Storage.Save(processLog);
                    //}

                }
                return processLog;
            //}//
        }

        public ProcessMessage GetNext(Process process,  ProcessMessage lastprocessed=null) {
            //using (new TemporarySimpleSession(this.Configuration.System)) {
                var lastsendtime = lastprocessed == null ? DateExtensions.Begin : lastprocessed.SendTime;
                var lastid = lastprocessed == null ? 0 : lastprocessed.Id;
                var hql =
                    "from ENTITY where (?=0 or (Id > ?)) and SendTime >= ? and Process = ? and Accepted = 0 order by Id";
                var q = Storage.First<ProcessMessage>(hql, lastid, lastid, lastsendtime, process);

                //HACK: to force hibernate to lazy
                if (null != q) {
                    var p = q.Process;
                    var c = q.Message;
                }
                //
                return q;
            //}

        }
    }
}