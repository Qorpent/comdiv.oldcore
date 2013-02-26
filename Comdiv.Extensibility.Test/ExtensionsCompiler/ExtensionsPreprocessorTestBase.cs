using System;
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using NUnit.Framework;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    public abstract class ExtensionsPreprocessorTestBase : BooCompilerTestBase {
        protected CompilerContext cu;
        protected Module pr;
        protected Module srcmodule;

        [SetUp]
        public virtual void setup() {
            string code = @"import System.Data
class X :
    def Z() as string :
        return  ''
def B() as bool :
    return false
registry['x'] = X
if B() :
    pass";
            this.cu = Compile("test",code, getPipeline() , prepareCompiler);
            Console.WriteLine(cu.Errors);
            Assert.True(cu.Errors.Count==0);
            this.srcmodule = cu.CompileUnit.Modules[0].Clone() as Module;
            Console.WriteLine(srcmodule.ToCodeString());
            this.pr = postProcess();
            if(pr.ToCodeString()!=srcmodule.ToCodeString()) {
                Console.WriteLine(pr.ToCodeString());
            }
        }

        protected virtual void prepareCompiler(CompilerParameters parameters) {
            
        }

        protected abstract Module postProcess();
        protected abstract CompilerPipeline getPipeline();

        [Test]
        public void have_IRegistryLoader_import() {
            var ns = pr.Imports.FirstOrDefault(x =>x.Alias != null && x.Alias.ToCodeString() == "_IRL_");
            Assert.NotNull(ns);
        }

        [Test]
        public void have_IDictionary_import() {
            var ns = pr.Imports.FirstOrDefault(x => x.Namespace=="System.Collections.Generic");
            Assert.NotNull(ns);
        }

        [Test]
        public void has_test_class() {
            var tc = pr.Members.OfType<ClassDefinition>().FirstOrDefault(x => x.Name == "L0__test");
            Assert.NotNull(tc);
        }

        [Test]
        public void test_class_has_load_method() {
            var tc = pr.Members.OfType<ClassDefinition>().First(x => x.Name == "L0__test");
            var m =
                tc.Members.OfType<Method>().FirstOrDefault(
                    x =>
                    x.Name == "Load" && x.Parameters.Count == 1 && x.Parameters[0].Type.ToCodeString() == "System.Collections.Generic.IDictionary[of string, object]" &&
                    (x.ReturnType == null || x.ReturnType.ToCodeString()=="void"));
            Assert.NotNull(m);
        }

        [Test]
        public void body_empty() {
            Assert.True(pr.Globals.IsEmpty);
        }
    }
}