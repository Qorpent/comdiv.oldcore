using System.Linq;
using Comdiv.Brail;
using Comdiv.Extensibility.Brail;
using MvcContrib.Comdiv.Extensibility.TestSupport;
using MvcContrib.Comdiv.ViewEngines.Brail;
using NUnit.Framework;

namespace Comdiv.Brail.Test{
    [TestFixture]
    [Category("Brail")]
    public class CallMacroTest : BaseMacroTestFixture<CallMacro>
    {
        [Test]
        public void this_controller_action()
        {
            checkMacro(
                @"call action1",

                @"Html.RenderAction('action1')");
        }

        [Test]
        public void minimal_notation_checked()
        {
            var result = compile("call");
            Assert.NotNull(result.Errors.FirstOrDefault(x => x.Message.Contains("call macro requires")));
        }

        [Test]
        public void foreign_controller_action()
        {
            checkMacro(
                @"call action1, controller1",

                @"Html.RenderAction('action1', 'controller1')");
        }

        [Test]
        public void this_controller_with_parameters()
        {
            checkMacro(
                @"call action1, null, {@name:@n}",

                @"__rd = RouteValueDictionary()
__rd.Add(@name, @n)
Html.RenderAction('action1', null, __rd)");
        }


        [Test]
        public void this_controller_with_parameters2()
        {
            checkMacro(
                @"call action1, {@name:@n}",

                @"__rd = RouteValueDictionary()
__rd.Add(@name, @n)
Html.RenderAction('action1', null, __rd)");
        }

        [Test]
        public void foreign_controller_with_parameters()
        {
            checkMacro(
                @"call action1, controller1, {@name:@n}",

                @"__rd = RouteValueDictionary()
__rd.Add(@name, @n)
Html.RenderAction('action1', 'controller1', __rd)");
        }


    }
}