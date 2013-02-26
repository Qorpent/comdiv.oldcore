using System.Xml.Linq;

namespace Comdiv.Xml{
    /// <summary>
    /// ��������� ������������ ������� ��� ����� BooXML
    /// </summary>
    public interface IXmlGenerator{
        /// <summary>
        /// ���������� ����� ����� XNode ��� ��������� �� ����� �������� generator
        /// </summary>
        /// <param name="call">������ �� ����������� �������� generate</param>
        object[] Generate(XElement call);
    }
}