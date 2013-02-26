using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Timers;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Persistence;
using FluentNHibernate;

namespace Comdiv.MAS {
    public abstract class MasApplicationBase : IConsoleLogHost {
        protected IList<string> assemblyProbePaths = new List<string>();

        public MasApplicationBase() {
            this.Args = new Dictionary<string, string>();
            
        }
        protected IDictionary<string, string> Args { get; private set; }
        protected MasConfiguration MasConfig { get; set; }
        protected Process MasProcess { get; set; }
        protected IMasProcessRepository MasRepository { get; set; }

        protected virtual void setupAdvancedIoc() {
            
        }

        /// <summary>
        /// returns first valid arg with given names (allows synonyms) or empty string
        /// </summary>
        /// <param name="argnames"></param>
        /// <returns></returns>
        public string get(params string [] argnames) {
            return get("", argnames);
        }

        /// <summary>
        /// returns first valid arg with given names (allows synonyms) with type conversion, allow defaults
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="def"></param>
        /// <param name="argnames"></param>
        /// <returns></returns>
        public T get<T>(T def, params string [] argnames) {
            foreach (var argname in argnames) {
                if(this.Args.ContainsKey(argname)) {
                    return this.Args[argname].to<T>();
                }
            }
            return def;
        }
        

        protected virtual void selflog(ProcessLog processLog) {
            
        }

        public void logdebug(string message, string eventtype = "default", string  source = "self") {
            log(message, eventtype, source, ProcessLogMessageType.Debug);
        }

        public void logtrace(string message, string eventtype = "default", string  source = "self") {
            log(message, eventtype, source, ProcessLogMessageType.Trace);
        }

        public void loginfo(string message, string eventtype = "default", string  source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Info);
        }

        public void logwarn(string message, string eventtype = "default", string  source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Warn);
        }

        public void logerror(string message, string eventtype = "default", string  source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Error);
        }

        public void logfatal(string message, string eventtype = "default", string  source = "self")
        {
            log(message, eventtype, source, ProcessLogMessageType.Fatal);
        }

        public virtual ProcessLog log(string message, string eventtype = "default", string  source = "self", ProcessLogMessageType type = ProcessLogMessageType.Debug, bool onlyself = false) {

            var log = new ProcessLog
            {
                Event = eventtype,
                Sender = source,
                Type = type,
                Message = message,
                Process = MasProcess,
               // Version = DateTime.Now,
            };
            if(SelfLogLevel <= type)
            {
                selflog(log);
            }
            if(!IgnoreMas && !onlyself && null!=MasConfig && MasConfig.DefaultLogLevel <= type) {
                try {

                    return MasRepository.Log(log);
                }catch(Exception ex) {
                    this.log(ex.ToString(),  "CANNOTLOG", type : ProcessLogMessageType.Error, onlyself : true);
                }
            }
            return null;
        }

        public bool HasActualMas { get; set; }

        protected virtual void setupAssemblyLoader() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var dir = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", "")),
                    "..\\..\\bin"));
            if(Directory.Exists(dir))assemblyProbePaths.Add(dir);
            dir = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", "")),
                    "..\\..\\extensions"));
            if (Directory.Exists(dir))
            {
                assemblyProbePaths.Add(dir);
            }
            
            dir = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", "")),
                    "..\\..\\tmp\\dlls"));
            if (Directory.Exists(dir)) {
                assemblyProbePaths.Add(dir);
            }
            dir = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", "")),
                    "..\\extensions"));
            if (Directory.Exists(dir))
            {
                assemblyProbePaths.Add(dir);
            }
            dir = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", "")),
                    "..\\tmp\\dlls"));
            if (Directory.Exists(dir))
            {
                assemblyProbePaths.Add(dir);
            }
            
            if(this.Args.ContainsKey("probe-paths")) {
                var paths = Args["probe-paths"].split();
                foreach (var path in paths) {
                    var p = path;
                    if(!Path.IsPathRooted(p)) {
                        p = Path.Combine(
                            Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Replace("file:///", "")),
                            p);

                    }
                    if(Directory.Exists(p)) {
                        this.assemblyProbePaths.Add(p);
                    }
                }
            }
            foreach (var probePath in assemblyProbePaths) {
                 logdebug("probe: "+probePath);
            }
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (var dir in assemblyProbePaths) {

                var file =  args.Name.Split(',')[0] + ".dll";
                file = Directory.GetFiles(dir, file, SearchOption.AllDirectories).FirstOrDefault();
                if(file.hasContent()) {
                    var assembly = Assembly.UnsafeLoadFrom(file);
                    return assembly;
                }
            }
            return null;
        }

        protected virtual MasConfiguration setupMasConfig() {
            var result = new MasConfiguration();
            result.Server = Args.get("mas-server", result.Server);
            result.Database = Args.get("mas-database", result.Database);
            result.System = Args.get("mas-system", result.System);
            result.CheckDatabase = Args.get("mas-checkdatabase", "").toBool();
            if(Args.ContainsKey("mas-repositorytype")) {
                var type = Args["mas-repositorytype"].toType();
                result.ProcessRepositoryType = type;
            }

            result.DefaultLogLevel =
                Args.get("mas-loglevel", () => ProcessLogMessageType.Info);
            return result;

        }

        protected virtual void initialize() {
            setupAssemblyLoader();
            setupFileSystem();
            this.MasConfig = setupMasConfig();
            this.SelfLogLevel = Args.get("self-loglevel", () => MasConfig.DefaultLogLevel);
            var cs = getConnectionSource();
            myapp.ioc.ensureService(cs, "default.connectionssource");
            
            setupMas();
            
            setupHibernate(cs);
           
            setupAdvancedIoc();
            if (!IgnoreMas)
            {
                checkMasWork();
            }
            setupMasProcess();
            setupSingleton();
        }

        private void setupHibernate(IConnectionsSource cs) {
            myapp.ioc.setupHibernate(cs,getModels());
        }

        protected virtual PersistenceModel[] getModels() {
            return new PersistenceModel[] {};
        }

        protected virtual IConnectionsSource getConnectionSource() {
            return new DefaultConnectionsSource();
        }

        protected void setupFileSystem() {
            myapp.ioc.AddSingleton("filepath.resolver", typeof(IFilePathResolver),
                                       new DefaultFilePathResolver(getRoot()));
        }

        protected string getRoot() {
            if (Args.ContainsKey("root")) return Args["root"];
            return FilePathResolverExtensions.GetDefaultRoot();

        }

        private void checkMasWork() {
            this.MasConfig.RethrowMasWorkingCheckError = Args.get("rethrow-mas-checking").toBool();
            if (!this.MasConfig.IsWorking())
            {
                if (CanIgnoreMas)
                {
                    this.IgnoreMas = true;
                    logwarn("MAS was not configured, proceed without MAS due to application settings");
                }
                else
                {
                    logfatal("MAS was not configured, cannot proceed without MAS due to application settings");
                    throw new MasException("this process must use MAS, which is not working or is not well configured");
                }
            }
        }

        private void setupMasProcess() {
            if (!IgnoreMas) {
                this.MasRepository = myapp.ioc.get<IMasProcessRepository>();
            }
            this.MasProcess = setupProcess();
            if (!IgnoreMas) {
                this.MasProcess = this.MasRepository.Start(this.MasProcess);
                this.EventRate = Args.get("mas-eventrate", () => 5000);
            }
        }

        private void setupSingleton() {
            if (!this.Singleton) {
                this.Singleton = Args.get("singleton", () => false);
            }
            this.SingletonLockCode = Args.get("singleton-lock", getSingletonLock);
        }

        private void setupMas() {
            this.IgnoreMas = (Args.get("mas-ignore", () => false) || this.IgnoreMasByDefault ) && !Args.get("mas-use",()=>false);
            
            if (!this.IgnoreMas) {
               
                myapp.ioc.setupMas(this.MasConfig);
                
            }
        }

        protected bool IgnoreMasByDefault { get; set; }

        protected bool IgnoreMas { get; set; }

        protected int EventRate { get; set; }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            this.log(e.ExceptionObject.ToString(), "FATAL", type: ProcessLogMessageType.Fatal, onlyself: true);
            //throw (Exception)e.ExceptionObject;
            
        }

        protected string SingletonLockCode { get; set; }

        protected virtual string getSingletonLock() {
            return MasProcess.Name;
        }

        public bool Singleton { get; set; }

        protected ProcessLogMessageType SelfLogLevel { get; set; }
        protected SqlBasedLockObject SingletonLock { get; set; }
        protected bool Finished { get; set; }

        protected virtual void Finalize() {
            if (null != EventTimer)
            {
                EventTimer.Stop();
            }
            if(null!=SingletonLock) {
                SingletonLock.Release();
            }
            if(!Finished && MasProcess!=null) {
                MasProcess.Result = -1;
                MasProcess.ResultDescription = "general error";
                if(!IgnoreMas && null!=MasRepository){
                    MasRepository.Finish(MasProcess);
                }
                
            }
            
        }

        protected bool getCanProceed() {
            var runrecord = getCanRunVerdict(new CanRunRecord());
            if(!runrecord.CanRun) {
                MasProcess.Result = 2;
                MasProcess.ResultDescription = runrecord.Description;
                MasRepository.Finish(MasProcess);
                Finished = true;
            }
            return runrecord.CanRun;
        }

        protected virtual CanRunRecord  getCanRunVerdict(CanRunRecord canRunRecord) {
            if(this.Singleton) {
                this.SingletonLock = SqlBasedLockObject.FullControl(this.SingletonLockCode,MasConfig.System,MasProcess.Code);
                if(!this.SingletonLock.NativeGet()) {
                    canRunRecord.CanRun = false;
                    canRunRecord.Description = "another instance of this singleton executed";
                }
            }
            return canRunRecord;
        }

        protected void normalProcessFinish() {
            if (EventTimer != null) {
                EventTimer.Stop();
            }
            if(MasProcess.Result==-10) {
                MasProcess.Result = 0;
            }
            if(MasProcess.ResultDescription.noContent()) {
                MasProcess.ResultDescription = "successfull";
            }
            if (!IgnoreMas) {
                MasRepository.Finish(MasProcess);
            }
            this.Finished = true;
        }

        public void Run() {
            try {

                if (this.Args.ContainsKey("debug-break"))
                {
                    Debugger.Break();
                }
                ConsoleLogHost.Current = this;
                initialize();
                validate();
                logdebug("initilized");
                bool canproceed = getCanProceed();
                logdebug("canrun " + canproceed);
                if (canproceed) {
                    logdebug("start execution");
                    if (!IgnoreMas) {
                        prepareEventListener();
                    }
                    this.execute();
                    normalProcessFinish();
                    logdebug("end execution");
                }
                logdebug("exit");
            }catch(ReflectionTypeLoadException rex) {
				if (null != EventTimer)
				{
					EventTimer.Stop();
				}
				logerror(rex.ToString());
            }
			catch(Exception ex){
                if(null!=EventTimer) {
                    EventTimer.Stop();
                }
                logerror(ex.ToString());
                //throw;
            }finally {
                this.Finalize();
            }
        }

        /// <summary>
        /// this method designed to make Assert-like code after initialization to check consostentry of parameters
        /// and environment
        /// </summary>
        protected virtual void validate() {
            
        }

        protected abstract void execute();

        protected void prepareEventListener() {
            this.EventTimer = new Timer(EventRate);
            EventTimer.Elapsed += getevent;
            EventTimer.Start();
        }

        protected Timer EventTimer { get; set; }

        void getevent(object sender, System.Timers.ElapsedEventArgs e) {
            lock (this) {
                try {
                    EventTimer.Stop();
                    this.LastMessage = MasRepository.GetNext(MasProcess, LastMessage);
                    if (LastMessage != null) {
                        logtrace("message: " + LastMessage.Message + " from " + LastMessage.Sender + " recieved");
                        try {
                            dispatchMessage(LastMessage);
                            logtrace("message ("+LastMessage.Type+") processed : " + LastMessage.Processed + " answer : '" +
                                     LastMessage.Answer+"'");
                        }
                        finally {
                            if (null != LastMessage) {
                                this.LastMessage.Accepted = true;
                                if (this.LastMessage.Processed) {
                                    this.LastMessage.AnswerTime = DateTime.Now;
                                }
                                this.LastMessage = MasRepository.Update(LastMessage);

                            }
                        }
                    }
                    EventTimer.Start();
                }catch(MasKillException ex) {
                    logfatal(ex.Message,"ERROR ON MESSAGE PROCESSING","SELF");
                    MasProcess.Result = -2;
                    MasProcess.ResultDescription = "killed";
                    MasRepository.Finish(MasProcess);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }
        }

        protected virtual void dispatchMessage(ProcessMessage lastMessage) {
            if(lastMessage.Type=="quit") { //normal exit - force it without exception, depends on
                this.ForceQuit = true;
                lastMessage.Processed = true;
                lastMessage.Answer = "force quit mark seted";
            }else if (lastMessage.Type=="kill") {
                lastMessage.Processed = true;
                lastMessage.Answer = "kill signal - force to throw Exception";
                throw new MasKillException("KILL MESSAGE OCCURED");
            }
            else if (lastMessage.Type == "echo")
            {
                lastMessage.Processed = true;
                lastMessage.Answer = lastMessage.Message;
            }
        }

        protected bool ForceQuit { get; set; }

        protected ProcessMessage LastMessage { get; set; }
        public bool CanIgnoreMas { get; set; }

        protected virtual Process setupProcess() {
            var process = Process.CreateForCurrentProcess();
            if(Args.ContainsKey("mas-process-code")) {
                process.Code = Args["mas-process-code"];
            }
            process.Name = Args.get("mas-process-name", process.Name);
            return process;
        }
    }

    [Serializable]
    public class MasKillException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MasKillException() {
        }

        public MasKillException(string message) : base(message) {
        }

        public MasKillException(string message, Exception inner) : base(message, inner) {
        }

        protected MasKillException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }
    }
}