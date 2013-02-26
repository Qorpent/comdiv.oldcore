using System.Collections.Generic;
using Comdiv.Text;
using NUnit.Framework;

namespace Comdiv.Core.Test.StringProcessing {
    [TestFixture]
    public class TemplateGeneratorTest {
        void test(string expected,string template,object source)
        {
            Assert.AreEqual(expected,new TemplateGenerator(template,source).Generate());

        }
        [Test]
        public void simple_test() {
            test("1 2 3","[[one]] [[two]] [[three]]",new{One=1,Two=2,Three=3});
        }
        [Test]
        public void simple_dict_test()
        {
            test("1 2 3", "[[one]] [[two]] [[three]]", new Dictionary<string,object>{{"one",1},{"two",2},{"three",3}});
        }

        [Test]
        public void format_test()
        {
            test("1-1 2 3", "[[one:{0}-{0}]] [[two]] [[three]]", new { One = 1, Two = 2, Three = 3 });
        }
        [Test]
        public void conditional_test()
        {
            test("1-1  3", "[[one:{0}-{0}]] <<usetwo:[[two]]>> <<usethree:[[three]]>>", new { One = 1, Two = 2, Three = 3, Usetwo=false, Usethree=true });
        }

        [Test]
        public void format_codevalue_test()
        {
            test("1-one 2 3", "[[one:{0}-{1}]] [[two]] [[three]]", new { One = 1, Two = 2, Three = 3 });
        }
    }
}