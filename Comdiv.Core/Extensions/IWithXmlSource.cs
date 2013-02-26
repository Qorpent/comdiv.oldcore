using System.Xml.Linq;

namespace Comdiv.Extensions {
    public interface IWithXmlSource {
        XElement Source { get; set; }
    }
}