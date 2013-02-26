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
    /// Основаня инкапсуляция подстановочного запроса
    /// </summary>
    public class LookupQuery : ILookupQuery{
        /// <summary>
        /// Признак - условие имени - маска (по умолчанию - true)
        /// </summary>
        //по умолчанию поиск по имени идет в режиме маски
        private bool nameMask = true;

        #region ILookupQuery Members

        /// <summary>
        /// Псевдоним запращиваемого источника
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Условие на код
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Условие на имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Признак - условие кода - маска
        /// </summary>
        public bool CodeMask { get; set; }


        public bool NameMask{
            get { return nameMask; }
            set { nameMask = value; }
        }

        /// <summary>
        /// Расширенный параметр, обработка зависит от реализации конкретного источника
        /// </summary>
        public string Custom { get; set; }

        /// <summary>
        /// Признак запроса - "только первое встречное значение" - может использоваться для оптимизации
        /// </summary>
        public bool First { get; set; }

        #endregion
    }
}