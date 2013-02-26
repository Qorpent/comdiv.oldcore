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
    public class SetupVariablePreprocessor : RegexBasedBrailPreprocessor{
        public SetupVariablePreprocessor(){
            Pattern = @"(?<v>\w+)\s*===\s*(?<i>[^,]+?)\s*,\s*(?<d>[^\r\n]+)";
            Evaluator = m =>{
                            var vname = m.Groups["v"].Value;
                            var def = m.Groups["d"].Value;
                            var initiator = m.Groups["i"].Value;
                            if (initiator.StartsWith("?")){
                                initiator = initiator.Substring(1);
                                return string.Format("{0} = {2}\r\nif IsDefined(\"{1}\"):\r\n{0} = {1}\r\nend", vname,
                                                     initiator, def);
                            }
                            else{
                                return string.Format("{0} = {1}\r\nif null=={0} or not {0}:\r\n{0} = {2}\r\nend", vname,
                                                     initiator, def);
                            }
                        };
        }
    }
}