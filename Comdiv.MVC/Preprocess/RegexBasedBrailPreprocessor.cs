using System.Linq;
using System.Text.RegularExpressions;
using Castle.MonoRail.Views.Brail;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC{
    public abstract class RegexBasedBrailPreprocessor : BrailPreprocessorBase{
        public string Pattern { get; set; }
        public string Replace { get; set; }
        public MatchEvaluator Evaluator { get; set; }

        public override string Preprocess(string code){
            //~img(help [,'Тут строка помощи'])
            if (Evaluator != null){
                return code.replace(Pattern, Evaluator);
            }
            return code.replace(Pattern, Replace);
        }
    }
}