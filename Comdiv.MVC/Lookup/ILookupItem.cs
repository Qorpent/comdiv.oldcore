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
    /// �������� ������� ������� ������
    /// </summary>
    /// <remarks>
    /// �� ����� ���������� �����, � ���� ����������� ��� ��� ��� �������� ��������� ����������
    /// </remarks>
    public interface ILookupItem : IWithCode, IWithName, IWithProperties{
        /// <summary>
        /// �������� ���������, �� �������� ������� �������
        /// </summary>
        string Alias { get; }

        string Category { get; set; }
        int Idx { get; set; }
    }
}