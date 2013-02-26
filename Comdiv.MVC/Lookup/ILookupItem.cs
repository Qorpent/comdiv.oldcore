using System.Linq;
using Comdiv.Application;
using Comdiv.Common;
using Comdiv.Conversations;
using Comdiv.Extensibility;
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
    /// Описание единица внешних данных
    /// </summary>
    /// <remarks>
    /// Не имеют первичного ключа, а лишь виртуальный код так как характер хранилища изолирован
    /// </remarks>
    public interface ILookupItem : IWithCode, IWithName, IWithProperties{
        /// <summary>
        /// Псевдони источника, из которого получен элемент
        /// </summary>
        string Alias { get; }

        string Category { get; set; }
        int Idx { get; set; }
    }
}