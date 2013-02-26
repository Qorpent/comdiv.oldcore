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
    public class ResourceBrailPreprocessor : RegexBasedBrailPreprocessor{
        public ResourceBrailPreprocessor(){
            Pattern = @"~img\((?<src>\w+)(,['""](?<txt>[\s\S]+?)['""])?\)";
            Replace = @"<img src='$${resx['${src}.png']}' alt='${txt}' title='${txt}' />";
        }
    }
}