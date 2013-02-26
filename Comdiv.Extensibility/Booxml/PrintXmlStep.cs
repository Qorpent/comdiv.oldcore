using Boo.Lang.Compiler.Steps;
using Comdiv.Application;

using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Booxml{
    public class PrintXmlStep : AbstractCompilerStep{
        public override void Run(){
            OutputWriter.Write(Context.Properties["xml"].ToString());
        }
    }
}