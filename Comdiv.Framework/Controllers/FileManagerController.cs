using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Framework.Utils;
using Comdiv.IO;
using Comdiv.Extensions;
using Comdiv.IO.FileSystemScript;
using Qorpent.Bxl;

#if SVNINTEGRATION
using SharpSvn;
#endif

namespace Comdiv.Controllers
{
    [Admin]
    public class FileManagerController : BaseController
    {
        private string root;

        [Layout("default")]
        public void index(string autosearch)
        {
            PropertyBag["autosearch"] = autosearch;
        }

        public void search(string query) {
            var queries = query.split(false, true, ' ').ToArray();
            this.root = myapp.files.Resolve("~/").ToLower();
            if(!this.root.EndsWith("/")) {
                root = root + "/";
            }
            query = query.Replace(".", "\\.").ToLower();
			var result = search(root, queries).OrderBy(x => x).ToArray();
        	PropertyBag["result"] = result;
#if SVNINTEGRATION
			IDictionary<string , SvnStatusEventArgs> svn = new Dictionary<string, SvnStatusEventArgs>();
			using (var c = new SvnClient()) {
				foreach (var r in result) {
					var ss = new Collection<SvnStatusEventArgs>();
					c.GetStatus((root+r).normalizePath(), out ss);
					svn[r] = ss[0];
				}
			}
        	PropertyBag["svn"] = svn;
#endif
        }

        public void get(string filename) {
            filename = "~/" + filename;
            var root = myapp.files.Resolve("~/").ToLower();
            var file = myapp.files.Resolve(filename,true).ToLower();
            if(!file.Contains(root)) {
                throw new SecurityException("try access paths not included into application!!!");
            }
            Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
            RenderText( myapp.files.Read(filename));
        }

        public IFileTemplateRepository Templates { get; set; }
        public IFileSystemScriptExecutor ScriptExecutor { get; set; }

        public void gettemplatejson(string code) {
            Templates = Templates ?? new FileTemplateRepository();
            RenderText("x="+Templates.GetTemplateJSON(code));
        }

        public void executescript(string code) {
            var args = ConvertPrefixedParametersToDict("a_");
            code = Path.GetFileNameWithoutExtension(code).replace(".fs$","");
            ScriptExecutor = ScriptExecutor ?? new BxlScriptExecutor();
            var sw = new StringWriter();
            ScriptExecutor.Execute(code,sw,args);
            RenderText(sw.ToString().toHtml());
        }

        [Admin]
		public void render(string file) {
			var fname = myapp.files.Resolve("~/" + file, true);
			if (fname.hasContent()) {
				var content = File.ReadAllText(fname);
				var transformer = content.find(@"\#render\s+([^\r\n]+)", 1);
				if(transformer.noContent()) {
					transformer = content.find(@"\<\?render\s+([^\r\n]+)\?\>", 1);
				}
				if(transformer.hasContent()) {
					var transformerfile = myapp.files.Resolve( transformer, true);
					if(transformerfile.noContent())throw  new Exception("cannot find render "+transformer);
					XElement src = null;
					if(Path.GetExtension(fname)==".xml") {
						src = XElement.Load(fname);
					}else {
						src = new BxlParser().Parse(File.ReadAllText(fname), fname);
					}
					XslCompiledTransform tr = new XslCompiledTransform();
					tr.Load(XmlReader.Create(new StringReader(File.ReadAllText(transformerfile))));
					var sw = new StringWriter();
					tr.Transform(src.CreateReader(),null,sw);
					RenderText(sw.ToString());
				}else {
					RenderText(content);
				}
			}else {
				throw new Exception("file not found "+file);
			}
			
		}

        public void getscriptparams(string code) {
            code = Path.GetFileNameWithoutExtension(code).replace(".fs$", "");
            var file = myapp.files.ResolveAll("~/", code + ".fs.script").FirstOrDefault();
            if(null==file) {
                RenderText("{}");
            }else {
                file += ".json";
                if (File.Exists(file)) {
                    RenderText(File.ReadAllText(file));
                }else {
                    RenderText("{}");
                }
                
            }
        }

        public void delete(string  filename) {
            var fname = myapp.files.Resolve("~/" + filename, true);
            if(fname.hasContent()) {
                File.Delete(fname);
            }
            RenderText("OK");
        }

        public void collectscripts() {
            PropertyBag["items"] =
                myapp.files.ResolveAll("~/", "*.fs.script").Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        public void getcurrentjson(string file)
        {
            Templates = Templates ?? new FileTemplateRepository();
            file = "~/" + file;
            RenderText("x="+Templates.GetCurrentFileTemplateJSON(file));
        }

        public void set(string filename, string content, string code, string json) {
            Templates = Templates ?? new FileTemplateRepository();
            filename = "~/" + filename;
            var root = myapp.files.Resolve("~/").ToLower();
            var file = myapp.files.Resolve(filename, false).ToLower();
            if ( !file.Contains(root))
            {
                throw new SecurityException("try access paths not included into application!!!");
            }

            if (code.hasContent())
            {
                Templates.ApplyTemplate(filename,code,json.fromJSON());
                RenderText("Файл успешно сгенерирован с шаблоном "+code+" с настройками "+json);
            }
            else {

                if (File.Exists(file)) {
                    var oldcontent = myapp.files.Read(filename);
                    if (oldcontent == content) {
                        RenderText("Не изменен");
                        return;

                    }
                    var bakfiledir = Path.Combine(Path.GetDirectoryName(file), ".bak/" + Path.GetFileName(file) + "/");
                    Directory.CreateDirectory(bakfiledir);
                    File.Copy(file,
                              Path.Combine(bakfiledir,
                                           DateTime.Now.ToString("YYYY-MM-dd-hh-mm-ss-") +
                                           myapp.usrName.Replace("\\", "_") + Path.GetExtension(file)));

                }

                myapp.files.Write(filename, content);
                RenderText("Файл сохранен");
            }
        }

        private IEnumerable<string> search(string dir,string[] queries) {

            foreach (var file in Directory.EnumerateFiles(dir)) {
                var normalized = file.ToLower().Replace("\\", "/");
                normalized = normalized.Replace(root, "");
                bool match = true;
                foreach (var q in queries) {
                    var test = !q.StartsWith("!");
                    var q_ = test ? q : q.Substring(1);
                    if(!(normalized.like(q_)==test)) {
                        match = false;
                        break;
                    }
                }
                if(match) {
                    yield return normalized;
                }
            }
            foreach (var subdir in Directory.EnumerateDirectories(dir)) {
                if(Path.GetFileName(subdir)==".svn") continue;
                foreach (var file in search(subdir,queries)) {
                    yield return file;
                }
            }
        }
    }
}
