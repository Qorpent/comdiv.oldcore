using System;
using System.IO;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using MvcContrib.Comdiv.ViewEngines.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test.Framework {
    public class BrailCompilerBasedTestBase {
        private BrailCompiler compiler;
        [SetUp]
        public void setup() {
            this.compiler = new BrailCompiler();
        }

        public CompilerContext compile(string code,string check, bool runonly = true) {
            var info = new ViewCompilerInfo();
            info.Options = new MvcViewEngineOptions();
            info.InMemory = true;
            info.ProcessingTest = true;
            info.Sources = new[] {new ViewCodeSource {DirectContent = code, Key = "CUSTOM"}};
            compiler.Compile(info);
            compiler.SetOutput(new StringWriter());
            var result = compiler.LastResult;
            Console.WriteLine("source code:");
            Console.WriteLine(code);
            Console.WriteLine("----------------------------------------------------------------------------------------------");
            if(result.Errors.Count>0) {
                Console.WriteLine("errors: ");
                Console.WriteLine(result.Errors.ToString());
                Console.WriteLine("----------------------------------------------------------------------------------------------");
            }
            string srctochech = result.CompileUnit.Modules[0].ToCodeString();
            if (runonly)
            {
                srctochech =
                    ((Method)((ClassDefinition)result.CompileUnit.Modules[0].Members[0]).Members["Run"]).Body.
                        ToCodeString();
            }
            Console.WriteLine("result code:");
            Console.WriteLine(srctochech);
            Console.WriteLine("----------------------------------------------------------------------------------------------");
            Assert.AreEqual(0,result.Errors.Count);
           
            var equal = StringExtensions.simplifyBoo(srctochech) ==
                        StringExtensions.simplifyBoo(check);
            if (!equal) {
                Console.WriteLine("but waits:");
                Console.WriteLine(check);
                Console.WriteLine("----------------------------------------------------------------------------------------------");

            }
            Assert.AreEqual(StringExtensions.simplifyBoo( srctochech),StringExtensions.simplifyBoo( check));
            return result;
        }
    }
}