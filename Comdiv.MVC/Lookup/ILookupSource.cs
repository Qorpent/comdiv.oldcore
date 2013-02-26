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
    /// ��������� ��������� �����������
    /// </summary>
    public interface ILookupSource{
        /// <summary>
        /// ��������� ���������
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// �������� ���������
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// ��������� ������
        /// </summary>
        IEnumerable<ILookupItem> Select(ILookupQuery query);
    }
}