using System.Collections.Generic;
using Comdiv.Application;
using Comdiv.Common;
using Comdiv.Extensions;
using Comdiv.Inversion;

namespace Comdiv.Wiki {
    public class WikiRepository : IWithContainer, IWikiRepository {
        
        private IInversionContainer _container;

        private IWikiPersistenceProvider _persister;
        public IWikiPersistenceProvider Persister {
            get {
                lock(this) {
                    if(_persister==null) {
                        _persister = myapp.ioc.get<IWikiPersistenceProvider>();
                    }
                }
                return _persister;
            }
            set { _persister = value; }
        }

        public IWikiRenderService Render { get; set; }


        #region IWithContainer Members

        public IInversionContainer Container {
            get {
                if (_container.invalid()) {
                    lock (this) {
                        if (_container.invalid()) {
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        #endregion

        
        public WikiPage Get(string code) {
            return Persister.Load(code);
        }


        public WikiPage[] Search(string mask) {
            return Persister.Search(mask);
        }

        public bool Exists(string code) {
            lock (this) {
                return Persister.Exists(code);
            }
        }

        public void Save(WikiPage page) {
            lock (this) {
                //Rerender(page);
                Persister.Write(page);
                
                
            }   
        }

        public void Rerender(WikiPage page) {
            lock (this) {
                page.RenderedContent = null;
                page.RenderedContent = Render.Start().Render(page);
            }
        }

        public WikiPage Refresh(WikiPage page) {
            Persister.Reload(true,page.Code);
            return Persister.Load(page.Code);
            
        }
    }
}