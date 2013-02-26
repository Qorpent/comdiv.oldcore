// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Common;
using Comdiv.Conversations;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Qorpent.Applications;
using Qorpent.Security;

namespace Comdiv.Application{
    ///<summary>
    ///</summary>
    /// 
    [NoCover("banality")]
    public static partial class myapp{
        private static readonly IList<EventWithDataHandler<object, int>> onReloadList =
            new List<EventWithDataHandler<object, int>>();
        public static Exception reload()
        {
            lock (sync)
            {
                return reload(3);
            }
        }

		static IDictionary<int,DateTime> objdatasync = new Dictionary<int, DateTime>();
		public static void ResetGlobalDataCache() {
			lock(sync) {
				objdatasync[0] = DateTime.Now;
			}
		}
		public static void SetObjDataCache(int id) {
			lock(sync) {
				objdatasync[id] = DateTime.Now;
			}
		}
		public static DateTime GetLastChecked(int obj) {
			lock(sync) {
				var common = objdatasync.get(0, () => DateTime.MinValue);
				var objdate = objdatasync.get(obj, () => DateTime.MinValue);
				return common > objdate ? common : objdate;
			}
		}

		public static DateTime GetGlobalLastDataRefresh() {
			lock(sync) {
				return sql.ExecuteScalar<DateTime>("select comdiv.get_global_data_refresh_time()");
			}
		}
		public static DateTime GetGlobalLastRefresh() {
			lock(sync) {
				return sql.ExecuteScalar<DateTime>("select comdiv.get_global_refresh_time()");
			}
		}
		public static void SetGlobalLastDataRefresh() {
			lock(sync) {
				sql.ExecuteNoQuery("exec comdiv.set_global_data_refresh_time");
			}
		}
		public static void SetGlobalLastRefresh() {
			lock(sync) {
				sql.ExecuteNoQuery("exec comdiv.set_global_refresh_time");
			}
		}
		public static void CheckGlobalReload() {
			lock(sync) {
				if(GetGlobalLastRefresh() > LastReloadTime) {
					reload();
				}
			}
		}

		
        public static IDbConnection getConnection(string  name="Default") {
            return ioc.get<IConnectionsSource>().Get(name).CreateConnection();
        }



        public static Exception reload(int level)
        {
            lock (sync)
            {
                Exception ex = null;
                log.Info("start reload");
                if (onReloadList.Count != 0)
                {
                    var exceptions = new List<Exception>();
                    var args = new EventWithDataArgs<int>(level);
                    EventWithDataHandler<object, int> wronghandler;
                    float ms = 0;
                    foreach (var handler in onReloadList.ToArray())
                    {
                        
                        try {
                            var s = Stopwatch.StartNew();
                            handler(null, args);
                            s.Stop();
                            if(s.ElapsedMilliseconds > ms) {
	                            ms = s.ElapsedMilliseconds;
                            }
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(e);
                        }
                    }
                    if (exceptions.Count != 0)
                    {
                        ex = new GroupException("error occured during Reload", exceptions);
                        log.Error("error on reload", ex);

                    }
                }
                if (level >= 5)
                {
                    //at high level must to forcely clean cached services
                    _principals = null;
                    _impersonation = null;
                    _roles = null;
                    _files = null;
                    _storage = null;
                }
                _LastReloadStamp = Environment.TickCount;
            	LastReloadTime = DateTime.Now;
                log.Info("end reload");
                return ex;
            }
        }
        public static readonly object sync = new object();
        //public static readonly UserData UserData = new UserData();
        private static IInversionContainer _container;
        private static IConversationManager _conversation;
        private static IFilePathResolver _files;
        private static IImpersonator _impersonation;
        private static IPrincipalSource _principals;
        private static IRoleResolver _roles;
        private static IStorageResolver _storage;
        private static int _LastReloadStamp;


    	private static LongTaskList longtasks = new LongTaskList();

        private static IDictionary<string, ActiveUserRecord> lastentercache = new Dictionary<string, ActiveUserRecord>();

        public static void registerEnter(){
            lock(sync) {
                lastentercache[usrName] = new ActiveUserRecord
                                              {LastAccess = DateTime.Now, Agent = HttpContext.Current.Request.UserAgent};
            }
        }

        public static IList<KeyValuePair<string ,ActiveUserRecord>> getActiveUsers(int minutes){
            lock(sync){
                return lastentercache.Where(x => x.Value.LastAccess.AddMinutes(minutes) >= DateTime.Now)
                    .OrderByDescending(x => x.Value.LastAccess).Select(x=>new KeyValuePair<string,ActiveUserRecord>(x.Key,x.Value)).ToList();
            }
        }
		public static IList<KeyValuePair<string, ActiveUserRecord>> getActiveUsers(DateTime lastcall)
		{
			lock (sync)
			{
				return lastentercache.Where(x => x.Value.LastAccess >= DateTime.Now)
					.OrderByDescending(x => x.Value.LastAccess).Select(x => new KeyValuePair<string, ActiveUserRecord>(x.Key, x.Value)).ToList();
			}
		}

        public static ILog log = new StubLog();// Logging.logger.get("comdiv.application.center");

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof (myapp)){
                        if (_container.invalid()){
                            Container = Inversion.ioc.Container;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }



        public static string usrName{
            get{
                lock (sync){
                    return usr.Identity.Name;
                }
            }
        }


        private static string _system;
        public static string System{
            get{
                lock (sync){
                    if (_system.noContent()){
                        _system = files.ReadXml("app.xml?$sys");
                        if (_system.noContent()){
                            _system = "default";
                        }
                    }
                    return _system;
                }
            }
            set { _system = value; }
        }

        public static string usrDomain{
            get{
                lock (sync){
                    var result = "(local)";
                    if (usrName.Contains("\\")){
                        result = usrName.Split('\\')[0];
                    }
                    return result;
                }
            }
        }


        public static IPrincipal usr{
            get { return principals.CurrentUser; }
            set { principals.BasePrincipal = value; }
        }

        public static IPrincipalSource principals{
            get{
                if (null == _principals){
                    lock (sync){
                        if (null == _principals){
                            _principals = Container.get<IPrincipalSource>() ??
                                          new PrincipalSource(){Impersonator = Impersonator};
                        }
                    }
                }
                return _principals;
            }
            set { _principals = value; }
        }

        public static IStorageResolver storage{
            get{
                if (null == _storage){
                    lock (sync){
                        if (null == _storage){
                            _storage = new DefaultStorageResolver();
                        }
                    }
                }
                return _storage;
            }
            set { _storage = value; }
        }

        public static IImpersonator Impersonator{
            get{
                if (null == _impersonation){
                    lock (sync){
                        if (null == _impersonation){
                            _impersonation = Container.get<IImpersonator>() ?? new Impersonator();
                        }
                    }
                }
                return _impersonation;
            }
            set { _impersonation = value; }
        }


        public static IFilePathResolver files{
            get{
                if (null == _files){
                    lock (sync){
                        if (null == _files){
                            _files = Container.get<IFilePathResolver>() ?? new DefaultFilePathResolver();
                        }
                    }
                }
                return _files;
            }
            set { _files = value; }
        }

        public static IRoleResolver roles{
            get{
                if (null == _roles){
                    lock (sync){
                        if (null == _roles){
                            _roles = Container.get<IRoleResolver>() ?? new RoleResolver();
                        }
                    }
                }
                return _roles;
            }
            set { _roles = value; }
        }

        public static IInversionContainer ioc{
            get { return Container; }
        }

        public static ILogManager logger{
            get { return Logging.logger.Manager; }
        }

        public static IConversationManager conversation{
            get{
                if (null == _conversation){
                    lock (DefaultConversationManager.sync){
                        if (null == _conversation){
                            _conversation = Container.get<IConversationManager>() ?? new DefaultConversationManager();
                        }
                    }
                }
                return _conversation;
            }
            set { _conversation = value; }
        }

        public static int LastReloadStamp{
            get { return _LastReloadStamp; }
        }


        public static LongTaskList Longtasks
		{
            get { return longtasks; }
            set { longtasks = value; }
        }

    	public static DateTime starttime { get; set; }

	    public static IApplication QorpentApplication {
		    get {
				lock(sync) {
					if(null==_qorpentApplication) {
						_qorpentApplication = Qorpent.Applications.Application.Current;
					}
					return _qorpentApplication;
				}
		    }
		    set { _qorpentApplication = value; }
	    }


	    public static readonly CurrentError errors = new CurrentError();
    	public static DateTime LastReloadTime;
	    private static IApplication _qorpentApplication;


	    [TestPropose]
        internal static void reset(){
            lock (sync){
                onReloadList.Clear();
            }
        }

        public static T resolve<T>() {
            return ioc.get<T>();
        }

        public static event EventWithDataHandler<object, int> OnReload{
            add{
                if (!onReloadList.Contains(value)){
                    onReloadList.Add(value);
                }
            }
            remove { onReloadList.Remove(value); }
        }

        [MigrationPropose]
        [TestPropose]
        public static void SetCurrentUser(IPrincipal principal){
            usr = principal;
        }

        [MigrationPropose]
        [TestPropose]
        public static void SetCurrentUser(string name){
            SetCurrentUser(new GenericPrincipal(new GenericIdentity(name), new string[]{}));
        }
    }
}