using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using MvcContrib.Comdiv.ViewEngines.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test.Framework
{
    [TestFixture]
    public class BrailCompilerTest
    {
        [Test]
        public void can_use_type_factory(){
             var resolver = new BrailSourceResolver();
            resolver.SetStatic("/test/x",@"<p>${i}</p>");
            resolver.SetStatic("/test/x2",@"bml:
    p : ""${i+i}""");
            var factory = new ViewTypeFactory(resolver, new MvcViewEngineOptions(),new BrailCompiler{AllInMemory=true});
            factory.CompileAll();
            var t1 = factory.CreateView<IView>("/test/x",new[] {typeof (BooViewEngine)}, new object[] {null});
            var t2 = factory.CreateView<IView>("/test/x", new[] { typeof(BooViewEngine) }, new object[] { null });

            var t3 = factory.CreateView<IView>("/test/x2", new[] { typeof(BooViewEngine) }, new object[] { null });
            var t4 = factory.CreateView<IView>("/test/x2", new[] { typeof(BooViewEngine) }, new object[] { null });

            Assert.AreEqual(t1.GetType().AssemblyQualifiedName,t2.GetType().AssemblyQualifiedName);
            Assert.AreEqual(t3.GetType(), t4.GetType());
            Assert.AreNotEqual(t1,t2);
            Assert.AreNotEqual(t3, t4);

        }

        [Test]
        [Ignore("Раньше билдил либу на диске - некорректно, а тест простоватенький, врядли навернется когдато")]
        public void factory_is_lead_slash_ignorance()
        {
            var resolver = new BrailSourceResolver();
            resolver.SetStatic("/test/x", @"<p>${i}</p>");
            resolver.SetStatic("/test/x2", @"bml:
    p : ""${i+i}""");
            var factory = new ViewTypeFactory(resolver, new MvcViewEngineOptions(), new BrailCompiler());
            factory.CompileAll();
            var t1 = factory.CreateView<IView>("/test/x", new[] { typeof(BooViewEngine) }, new object[] { null });
            var t2 = factory.CreateView<IView>("test/x", new[] { typeof(BooViewEngine) }, new object[] { null });

            var t3 = factory.CreateView<IView>("/test/x2", new[] { typeof(BooViewEngine) }, new object[] { null });
            var t4 = factory.CreateView<IView>("test/x2", new[] { typeof(BooViewEngine) }, new object[] { null });

            Assert.AreEqual(t1.GetType().AssemblyQualifiedName, t2.GetType().AssemblyQualifiedName);
            Assert.AreEqual(t3.GetType(), t4.GetType());
            Assert.AreNotEqual(t1, t2);
            Assert.AreNotEqual(t3, t4);

        }

        [Test]
        public void very_simple_bml_test() {
            var compiler = new BrailCompiler();
            var options = new MvcViewEngineOptions();
            var src = new ViewCodeSource
                          {
                              Key = "/test/x",
                              DirectContent =
                                  @"
bml:
    p : ""${i}"""
                          };
            var type = compiler.CompileSingle(src, options);
            Assert.NotNull(type);
            var view = (IView)type.create<BrailBase>(types: new Type[] { typeof(BooViewEngine) }, parameters: new object[] { null });
            var sw = new StringWriter();
            var vc = new ViewContext();
            vc.ViewData = new ViewDataDictionary();
            vc.ViewData["i"] = 3;
            view.Render(vc,sw);
            Assert.AreEqual("<p>3</p>",sw.ToString());
        }

        [Test]
        public void ZE_BUG_can_call_compiler_in_bml_repeatedly()
        {
            var compiler = new BrailCompiler();
            var options = new MvcViewEngineOptions();
            var src = new ViewCodeSource
            {
                Key = "/test/x",
                DirectContent =
                    @"
bml:
    p : ""${i}"""
            };
            compiler.CompileSingle(src, options);
            src = new ViewCodeSource
            {
                Key = "/test/x2",
                DirectContent =
                    @"
bml:
    p : ""${i}"""
            };
            compiler.CompileSingle(src, options);
        }

        [Test]
        public void timestamp_test()
        {
            var compiler = new BrailCompiler();
            var options = new MvcViewEngineOptions();
            var src = new ViewCodeSource
            {
                Key = "/test/x",
                DirectContent =
                    @"
bml:
    p : ""${i}"""
            };
            var type = compiler.CompileSingle(src, options);
            var attr = type.getFirstAttribute<TimeStampAttribute>();
            Assert.AreEqual(src.LastModified.ToString("dd.MM.yyyy HH:mm:ss"), attr.DateString);
        }
        [Test]
        public void very_simple_wsa_test()
        {
            var compiler = new BrailCompiler();
            var options = new MvcViewEngineOptions();
            var src = new ViewCodeSource
            {
                Key = "/test/x",
                DirectContent =@"<p>${i}</p>"};
            var type = compiler.CompileSingle(src, options);
            Assert.NotNull(type);
            var view = (IView)type.create<BrailBase>(types: new Type[] { typeof(BooViewEngine) }, parameters: new object[] { null });
            var sw = new StringWriter();
            var vc = new ViewContext();
            vc.ViewData = new ViewDataDictionary();
            vc.ViewData["i"] = 3;
            view.Render(vc, sw);
            Assert.AreEqual("<p>3</p>", sw.ToString());
        }

        [Test]
        public void very_simple_mixed_test()
        {
            var compiler = new BrailCompiler();
            var options = new MvcViewEngineOptions();
            var src = new ViewCodeSource
            {
                Key = "/test/x",
                DirectContent = @"<p>${i}</p>"
            };
            var src2 = new ViewCodeSource
            {
                Key = "/test/x2",
                DirectContent = @"bml:
    p : ""${i+i}"""
            };
            var info = new ViewCompilerInfo {Sources = new[] {src, src2}, InMemory = true, Options = options};
            var ass = compiler.Compile(info);
            var type1 = ass.GetType("/test/x".Replace("/", "_0_"));
            var type2 = ass.GetType("/test/x2".Replace("/", "_0_"));
            var view1 = (IView)type1.create<BrailBase>(types: new Type[] { typeof(BooViewEngine) }, parameters: new object[] { null });
            var view2 = (IView)type2.create<BrailBase>(types: new Type[] { typeof(BooViewEngine) }, parameters: new object[] { null });
            var sw = new StringWriter();
            var vc = new ViewContext();
            vc.ViewData = new ViewDataDictionary();
            vc.ViewData["i"] = 3;
            view1.Render(vc, sw);            
            Assert.AreEqual("<p>3</p>", sw.ToString());
            sw = new StringWriter();
            view2.Render(vc, sw);
            Assert.AreEqual("<p>6</p>", sw.ToString());
        }
    }
}
