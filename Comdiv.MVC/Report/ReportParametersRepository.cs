using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Reporting;
using Qorpent.Bxl;

namespace Comdiv.MVC.Report
{
    public class ReportParametersRepository
    {
        static IDictionary<string,Parameter> _cache = new Dictionary<string, Parameter>();
        static ReportParametersRepository() {
            myapp.OnReload += myapp_OnReload;
        }

        static void myapp_OnReload(object sender, Common.EventWithDataArgs<int> args)
        {
            _cache.Clear();
        }


        public Parameter Get(string code) {
            return _cache.get(code, () => internalGet(code));
        }

        public Parameter internalGet(string code) {
            var bxls = myapp.files.ResolveAll("data", "*.bxl", true, null);
			var parser = new BxlParser();
            foreach (string bxl in bxls) {
				if(bxl.Contains(".bak"))continue;
                var x = parser.Parse(File.ReadAllText(bxl));
                var pxs = x.XPathSelectElements("//paramlib/param");
                foreach (XElement px in pxs) {

                        var result = new Parameter();
                        if (bxl.Contains("/sys/"))
                        {
                            result.Level = "sys";
                        }
                        else if (bxl.Contains("/usr/"))
                        {
                            result.Level = "usr";
                        }
                        else if (bxl.Contains("/mod/"))
                        {
                            result.Level = "mod";
                        }
                        px.applyTo(result);
                    if(!_cache.ContainsKey(result.Code)) {
                        _cache[result.Code] = result;
                    }
                    if(result.Code==code) {
                        return result;
                    }
                }
                
            }
            return new Parameter {Code = code, Name = code,Level = "usr"};
        }
    }
}
