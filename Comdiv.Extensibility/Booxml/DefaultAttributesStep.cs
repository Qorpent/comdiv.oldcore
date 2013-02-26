using System.Linq;
using Boo.Lang.Compiler.Ast;
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

namespace Comdiv.Booxml{
    public class DefaultAttributesStep : BooxmlMacroVisitorBase
    {
        protected override void innerOnMacro(MacroStatement node){
            var simps = Simples;
            if(simps.Count>0){
                Element.SetAttributeValue("id",Simples[0].expand().Replace("_QUOTED_USD_","$"));
                //for compatibility
				Element.SetAttributeValue("code", Simples[0].expand().Replace("_QUOTED_USD_", "$"));
            }
            if (simps.Count > 1)
            {
				Element.SetAttributeValue("name", Simples[1].expand().Replace("_QUOTED_USD_", "$"));
            }
            if(simps.Count > 2){
                foreach (var attr in simps.Skip(2).ToArray()){
                    Element.SetAttributeValue(attr.ToCodeString(),"1");
                }
            }
        }
    }

    
}