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
    /// Интерфейс источника подстановки
    /// </summary>
    public interface ILookupSource{
        /// <summary>
        /// Псевдоним источника
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Описание источника
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// Выполняет запрос
        /// </summary>
        IEnumerable<ILookupItem> Select(ILookupQuery query);
    }
}