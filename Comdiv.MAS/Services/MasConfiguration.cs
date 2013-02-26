using System;
using Comdiv.Application;
using Comdiv.Inversion;

namespace Comdiv.MAS {
    public class MasConfiguration {
        public MasConfiguration() {
            this.System = "mas";
            this.Server = "MASSERVER";
            this.Database = "mas";
            this.CheckDatabase = false;
            this.DefaultLogLevel = ProcessLogMessageType.Info;
        }

        public string Database { get; set; }

        public string Server { get; set; }

        public string System { get; set; }

        public bool CheckDatabase { get; set; }

        public bool RethrowMasWorkingCheckError { get; set; }

        public ProcessLogMessageType DefaultLogLevel { get; set; }

        public Type ProcessRepositoryType { get; set; }

        public bool IsWorking() {
            try {
                var repo = myapp.ioc.get<IMasProcessRepository>();
                repo.Get(new Process {Id = -1});
                return true;
                ;
            }catch(Exception ex) {
                if (RethrowMasWorkingCheckError) {
                    throw;
                }
                return false;
            }
        }
    }
}