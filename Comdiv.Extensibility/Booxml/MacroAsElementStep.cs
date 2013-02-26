using System;
using System.Xml.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;

using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Booxml{
    public class MacroAsElementStep : BooxmlMacroVisitorBase{
        protected override void innerOnMacro(MacroStatement node){
            Element = new XElement(node.Name,
                new XAttribute("_line",node.LexicalInfo.Line),
                new XAttribute("_file",node.LexicalInfo.FileName)
                );
        }
    }
}