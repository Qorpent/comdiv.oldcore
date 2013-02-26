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
    /// �������� ������������ ��������������� �������
    /// </summary>
    public class LookupQuery : ILookupQuery{
        /// <summary>
        /// ������� - ������� ����� - ����� (�� ��������� - true)
        /// </summary>
        //�� ��������� ����� �� ����� ���� � ������ �����
        private bool nameMask = true;

        #region ILookupQuery Members

        /// <summary>
        /// ��������� �������������� ���������
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// ������� �� ���
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// ������� �� ���
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ������� - ������� ���� - �����
        /// </summary>
        public bool CodeMask { get; set; }


        public bool NameMask{
            get { return nameMask; }
            set { nameMask = value; }
        }

        /// <summary>
        /// ����������� ��������, ��������� ������� �� ���������� ����������� ���������
        /// </summary>
        public string Custom { get; set; }

        /// <summary>
        /// ������� ������� - "������ ������ ��������� ��������" - ����� �������������� ��� �����������
        /// </summary>
        public bool First { get; set; }

        #endregion
    }
}