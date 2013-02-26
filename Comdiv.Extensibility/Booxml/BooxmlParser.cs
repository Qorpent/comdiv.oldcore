using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.MetaProgramming;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Booxml{

    public class BooxmlParser
    {
        private CompilerPipeline _pipeline;

        public CompilerPipeline Pipeline{
            get{
                if(null==_pipeline){
                    _pipeline = new BooxmlPipeline();
                }
                return _pipeline;
            }
            set { _pipeline = value; }
        }

        private BooCompiler _compiler;

        public BooCompiler Compiler{
            get{
                if(null==_compiler){
                    _compiler = new BooCompiler();
                    _compiler.Parameters.Pipeline = Pipeline;
                    _compiler.Parameters.GenerateInMemory = true;
                }
                return _compiler;
            }
            set { _compiler = value; }
        }

        public XElement Parse(string content){
            return Parse("main", content);
        }

        public XElement Parse(string name,string content){
            return Parse(name, content, null);
        }

        public XElement Parse(string content, IDictionary<string,string > defines){
            return Parse("main", content, defines);
        }

        public XElement Parse(string name,string content, IDictionary<string,string > defines){
            if(content.noContent())return new XElement("empty");
            Compiler.Parameters.Input.Clear();
            Compiler.Parameters.Input.Add(new StringInput(name, content));
            if(defines.yes()){
                foreach (var define in defines){
                    Compiler.Parameters.Defines[define.Key] = define.Value;
                }
            }
            
            var result = Compiler.Run();
            if(result.Errors.Count>0){
                throw  new CompilationErrorsException(result.Errors);
            }
            return result.Properties["xml"] as XElement;
        }
    }
}