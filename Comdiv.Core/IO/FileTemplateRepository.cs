using System.IO;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Text;
using Enumerable = System.Linq.Enumerable;

namespace Comdiv.IO {
    public class FileTemplateRepository : IFileTemplateRepository {
        public string GetTemplateJSON(string code) {
            var file = Enumerable.FirstOrDefault<string>(myapp.files.ResolveAll("~/", code + ".template.json", true, true));
            if (null != file) return File.ReadAllText(file);
            var template = GetTemplate(code);
            var tg = new TemplateGenerator(template);
            var substs = tg.GetSubstitutions();
            var condtions = tg.GetConditions();
            var s = "{\r\n";
            foreach (var subst in substs) {
                s += string.Format("'{0}':{type:'string', value='',},\r\n", subst);
            }
            foreach (var cond in condtions)
            {
                s += string.Format("'{0}':{type:'bool', value=false,},\r\n", cond);
            }
            s += "}";
            return s;
        }

        public string GetCurrentFileTemplateJSON(string file) {
            var path = file + ".file.tg.json";
            path = myapp.files.Resolve(path, true);
            if(null!=path) {
                return File.ReadAllText(path);
            }
            return "{}";
        }

        public string GetTemplate(string code) {
            var file = myapp.files.ResolveAll("~/", code + ".template", true, true).FirstOrDefault();
            if (null != file) return File.ReadAllText(file);
            return "";
        }

        public void ApplyTemplate(string filename, string templatecode, object datasource) {
            var templatetxt = GetTemplate(templatecode);
            var jsonfilename = filename + ".file.tg.json";
        	var json = myapp.ioc.get<IJsonTransformer>().ToJson(datasource);
            var newtxt = new TemplateGenerator(templatetxt, datasource).Generate();
            myapp.files.Write(filename,newtxt);
            myapp.files.Write(jsonfilename,json);
        }
    }
}