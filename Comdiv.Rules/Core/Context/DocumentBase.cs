using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;
using Comdiv.Rules.Support;

namespace Comdiv.Rules{
    public class DocumentBase : IDocument{
        private IParametersProvider<IDocument> _params;

        #region IDocument Members

        public IParametersProvider<IDocument> Params{
            get { return _params ?? (_params = new DocumentParametersContainer(this)); }
            set{
                _params = value;
                _params.Target = this;
            }
        }

        #endregion
    }
}