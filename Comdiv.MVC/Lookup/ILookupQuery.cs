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
    /// Описывает запрос к внешним данным, на данный момент примитивен и рассчитан на типовой вариант
    /// </summary>
    public interface ILookupQuery{
        /// <summary>
        /// Псевдоним источника
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Условие на код
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Условие на имя
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Искать код в режиме маски
        /// </summary>
        bool CodeMask { get; }

        /// <summary>
        /// Искать имя в режиме маски
        /// </summary>
        bool NameMask { get; }

        /// <summary>
        /// Нерегламентированный дополнительный параметр
        /// </summary>
        string Custom { get; }

        /// <summary>
        /// Признак того, что должен возвращаться только один элемент (в целях оптимизации)
        /// </summary>
        bool First { get; }
    }
}