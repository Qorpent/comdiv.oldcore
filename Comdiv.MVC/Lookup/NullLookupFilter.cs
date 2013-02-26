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

namespace Comdiv.Model.Lookup{
    public class NullLookupFilter : ILookupFilter{
        #region ILookupFilter Members

        public ILookupItem Filter(ILookupItem item){
            return item;
        }

        #endregion
    }
}