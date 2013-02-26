using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Common;
using Comdiv.Inversion;

namespace Comdiv.Wiki {
    public class WikiRender : IWithContainer, IWikiRenderService {
        public WikiRender() {
            myapp.OnReload += new EventWithDataHandler<object, int>(myapp_OnReload);
        }

        void myapp_OnReload(object sender, EventWithDataArgs<int> args)
        {
            lock (this) {
                
            }
        }
        private IInversionContainer _container;
        private IList<IWikiRender> renders;
        public IInversionContainer Container
        {
            get
            {
                if (_container.invalid())
                {
                    lock (this)
                    {
                        if (_container.invalid())
                        {
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }


        public IList<IWikiRender> Renders {
            get {
                lock(this) {
                    if(null==renders) {

                        renders = Container.all<IWikiRender>().OrderBy(x => x.Idx).ToList();
                        foreach (var r in renders.OfType<IWikiAwared>()) {
                            r.RenderService = this;
                            r.Repository = this.Container.get<IWikiRepository>();
                        }
                    }
                }
                return renders;
            }
        }

        public WikiRenderExecutor Start() {
            return new WikiRenderExecutor(this.Renders.ToArray());
        }
    }
}