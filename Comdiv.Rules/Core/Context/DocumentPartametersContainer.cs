using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Support{
    public class DocumentParametersContainer : ParametersContainer<IDocument>{
        public DocumentParametersContainer(IDocument target){
            Target = target;
        }
    }
}