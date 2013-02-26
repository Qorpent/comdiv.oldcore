using System.Collections.Generic;
using Comdiv.Application;
using Comdiv.Inversion;

namespace Comdiv.Extensibility.Brail{
    public class PreprocessorFactory : IBrailPreprocessorFactory{
        private IInversionContainer _container;
    	private IEnumerable<IBrailPreprocessor> _cached;

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

        #region IBrailPreprocessorFactory Members


		
        public IEnumerable<IBrailPreprocessor> GetPreprocessors() {
        	return _cached ?? (_cached = Container.all<IBrailPreprocessor>());
        }

        #endregion
    }
}