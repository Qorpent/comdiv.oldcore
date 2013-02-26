using System.Xml.Linq;

namespace Comdiv.Xml{
    /// <summary>
    /// Формирует динамический контент для файла BooXML
    /// </summary>
    public interface IXmlGenerator{
        /// <summary>
        /// Возвращает набор любых XNode для включения на место элемента generator
        /// </summary>
        /// <param name="call">ссылка на определение элемента generate</param>
        object[] Generate(XElement call);
    }
}