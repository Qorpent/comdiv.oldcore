using Comdiv.IO.FileSystemScript;
using Qorpent.Bxl;

namespace Comdiv.Controllers {
    public class BxlScriptExecutor:DefaultScriptExecutor {
        protected override System.Xml.Linq.XElement parseTxt(string script) {
            return new BxlParser().Parse(script);
        }
    }
}