using System.Collections.Generic;
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
    /// <summary>
    /// Простая базовая реализация источника подстановки
    /// </summary>
    public abstract class BaseLookupSource : ILookupSource{
        #region ILookupSource Members

        public string Alias { get; set; }
        public string Comment { get; set; }
        public abstract IEnumerable<ILookupItem> Select(ILookupQuery query);

        #endregion
    }
}