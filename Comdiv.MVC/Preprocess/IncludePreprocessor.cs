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

namespace Comdiv.MVC.Preprocess{
    public class IncludePreprocessor : RegexBasedBrailPreprocessor{
        public IncludePreprocessor(){
            Pattern = @"\#include\s(?<n>[/\w]+)";
            Evaluator = m =>{
                            var name = m.Groups["n"].Value;
                            var file = myapp.files.Resolve("views/includes/" + name + ".brail");
                            var content = myapp.files.Read(file);
                            return content + "\r\n\r\n";
                        };
        }
    }
}