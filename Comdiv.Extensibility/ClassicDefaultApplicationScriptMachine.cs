using System.Linq;
using Comdiv.Application;

using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Extensibility.Boo{
    public class ClassicDefaultApplicationScriptMachine : BooScriptMachine, IDefaultScriptMachine{
        public ClassicDefaultApplicationScriptMachine() : this("") {}
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
        public ClassicDefaultApplicationScriptMachine(string ext){
            var extPath = ("~/usr/extensions" + ext).mapPath();
            var extPath2 = ("~/mod/extensions" + ext).mapPath();
            var extPath3 = ("~/sys/extensions" + ext).mapPath();
            var extPath4 = ("~/extensions" + ext).mapPath();

            RootDirs = new[]{extPath3, extPath2, extPath};
            //myapp.OnReload += (s, a) => Reload();
        }

        #region IDefaultScriptMachine Members

        public override void Reload(){
            lock (this){
                Apply(Container);    
            }
            
        }

        public void Apply(IInversionContainer locator){
            lock (this){
                base.Reload();
                foreach (var pair in Registry){
                    if (pair.Value != null){
                        Container.set(pair.Key, pair.Value);
                    }
                }
            }
        }

        #endregion
    }
}