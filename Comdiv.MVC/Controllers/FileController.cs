using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC.Security;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Controllers{
    public interface IFileListProvider : IWithIdx{
        string CustomView { get; set; }
        IList<Entity> GetFiles();
    }

    [Admin]
    public class FileController : BaseController{
        [ActionDescription(
            ActionRole.Developer,
            ActionSeverity.SecurityLack,
            "получения списка основных файлов"
            )]
        public void Index(){
            PropertyBag["folders"] = Container.all<IFileListProvider>().OrderBy(x => x.Idx);
        }

        [ActionDescription(
            ActionRole.Developer,
            ActionSeverity.Dangerous,
            "подгрузка файлов в систему"
            )]
        public void Upload(string name){
            var file = GetFile();
            var path = PathResolver.Resolve(name, false);
            file.SaveAs(path);
            RedirectToAction("index", new NameValueCollection{{"asworkspace", "1"}});
        }

        [Role("ADMIN,DESIGNER")]
        public void open(string name,int line){
            var content = myapp.files.Read(name);
            var file = name;
            PropertyBag["file"] = name;
            for (int i = 0; i <= 50; i++ ){
                content += "\r\n";
            }
                PropertyBag["content"] = content;
            
            PropertyBag["linecount"] = Regex.Matches(content, @"(\r\n)|(\r)|(\n)").Count+1;
            PropertyBag["line"] = line;
        }


        [Role("ADMIN,DESIGNER")]
        public void save(string name, string content)
        {
            myapp.files.Write(name,content);
            RenderText("OK ");
        }



        [ActionDescription(
            ActionRole.Developer,
            ActionSeverity.Dangerous,
            "выполнение команды оболочки на сервере из bat файла"
            )]
        public void ExecCmd(string code){
            var path = PathResolver.Resolve("cmd/" + code + ".bat");
            PropertyBag["result"] = "";
            if (null != path){
                PropertyBag["result"] = path.toCmdProcess().run().toHtml();
            }
            RenderView("exec");
        }

        [ActionDescription(
            ActionRole.Developer,
            ActionSeverity.Dangerous,
            "выполнение команды консоли для удаленного администрирования"
            )]
        public void Exec(string cmd){
            var g = Guid.NewGuid().ToString();
            var path = PathResolver.Resolve("~/tmp/cmd/" + g + ".bat", false);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, cmd);
            try{
                PropertyBag["result"] = path.toCmdProcess().run(5000).toHtml();
            }
            finally{
                File.Delete(path);
            }
        }
    }
}