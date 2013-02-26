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
    public class ForCyclePreprocessor : RegexBasedBrailPreprocessor{
        public ForCyclePreprocessor(){
            Pattern = @"\{\{(?<i>\w+)?(?<b>[\s\S]+?)\}\}";
            Evaluator = m =>{
                            var from = m.Groups["i"].Value;
                            if (from.noContent()){
                                from = "items";
                            }
                            var variable = from.Substring(0, 1);
                            var body = m.Groups["b"].Value;
                            var result = string.Format("<%for {0} in {1}:%>\r\n\t{2}\r\n<%end%>", variable, from, body);
                            return result;
                        };
        }
    }
}