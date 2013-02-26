using System.IO;
using System.Linq;
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
    public class DirectoryClearTask : ITask{
        public DirectoryClearTask(string folder, string name){
            TargetFolder = folder;
            ExecuteWhen = TaskExecuteWhen.OnAppStart;
            Name = name ?? folder;
        }

        public string TargetFolder { get; set; }

        #region ITask Members

        public TaskExecuteWhen ExecuteWhen { get; set; }
        public string Name { get; set; }


        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        public void Execute(){
            var fs = Container.get<IFilePathResolver>();
            if (null == fs){
                logger.get("comdiv.common").Warn("Нет сконфигурированной файловой системы");
            }
            else{
                var glob = fs.Resolve(TargetFolder,false);
                if (Directory.Exists(glob)){
                    foreach (var file in Directory.GetFiles(glob)){
                        File.Delete(file);
                    }
                }
            }
        }

        #endregion
    }
}