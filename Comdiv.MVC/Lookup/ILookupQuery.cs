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
    /// ��������� ������ � ������� ������, �� ������ ������ ���������� � ��������� �� ������� �������
    /// </summary>
    public interface ILookupQuery{
        /// <summary>
        /// ��������� ���������
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// ������� �� ���
        /// </summary>
        string Code { get; }

        /// <summary>
        /// ������� �� ���
        /// </summary>
        string Name { get; }

        /// <summary>
        /// ������ ��� � ������ �����
        /// </summary>
        bool CodeMask { get; }

        /// <summary>
        /// ������ ��� � ������ �����
        /// </summary>
        bool NameMask { get; }

        /// <summary>
        /// �������������������� �������������� ��������
        /// </summary>
        string Custom { get; }

        /// <summary>
        /// ������� ����, ��� ������ ������������ ������ ���� ������� (� ����� �����������)
        /// </summary>
        bool First { get; }
    }
}