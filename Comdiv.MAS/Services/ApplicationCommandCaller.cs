using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Comdiv.Extensions;
using Qorpent.Bxl;

namespace Comdiv.MAS.Services
{
    public class ApplicationCommandCaller
    {
        public void Execute(App app, string  command, IDictionary<string,object > parameters) {
            if(null==app)throw new ArgumentNullException("app");
            if (command.noContent()) throw new ArgumentNullException("command");
            parameters = parameters ?? new Dictionary<string, object>();
			var commands = new BxlParser().Parse(app.Type.Commands.Trim());
            var xcmd = commands.XPathSelectElement("//command[@code='" + command + "']");
            if(null==xcmd)throw new Exception("cannot find command "+command+" on app "+app.Code);

        }
    }
}
