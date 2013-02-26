using Comdiv.ConsoleUtils;
using NUnit.Framework;

namespace Comdiv.Core.Test.ConsoleUtils {
    [TestFixture]
    public class ConsoleArgumentParserTest {
        private const string Sample1 = "--a --b --c";
        private const string Sample2 = "--a v1 --b --c v2";
        private const string Sample3 = @"--a ""val""";
        private const string Sample4 = @"--a ""val~~val""";
        private const string Sample5 = @"--b ""  -   val   -        "" --a"; /// Для выявленных касяков - про пробеол в конце и про повторный тримминг
        [Test]
        public void Test1() {
            var cap = new ConsoleArgumentParser();
            var d = cap.Parse(Sample1);
            Assert.AreEqual(d["a"], "");
            Assert.AreEqual(d["b"], "");
            Assert.AreEqual(d["c"], "");
        }

        [Test]
        public void Test2() {
            var cap = new ConsoleArgumentParser();
            var d = cap.Parse(Sample2);
            Assert.AreEqual(d["a"], "v1");
            Assert.AreEqual(d["b"], "");
            Assert.AreEqual(d["c"], "v2");
        }

        [Test]
        public void Test3() {
            var cap = new ConsoleArgumentParser();
            var d = cap.Parse(Sample3);
            Assert.AreEqual(d["a"], "val");
        }

        [Test]
        public void Test4()
        {
            var cap = new ConsoleArgumentParser();
            var d = cap.Parse(Sample4);
            Assert.AreEqual(d["a"], "val--val");
        }

         [Test]
        public void Test5_bugs()
        {
            var cap = new ConsoleArgumentParser();
            var d = cap.Parse(Sample5);
            Assert.AreEqual(d["b"], "-   val   -");
             Assert.AreEqual(d["a"], "");
        }



    }
}