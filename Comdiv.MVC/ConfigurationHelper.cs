using System.Linq;
using Comdiv.Application;
using Comdiv.Caching;
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
    public class ConfigurationHelper{
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
        public string this[string path]{
            get { return Container.get<IFilePathResolver>().ReadXml(path); }
        }

        public string this[string path, string def]{
            get { return Container.get<IFilePathResolver>().ReadXml(path) ?? def; }
        }

        public object fromSession(string key){
            return Container.get<IApplicationCache>().Get(key);
        }
    }
}