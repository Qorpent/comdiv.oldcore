using System.Collections.Generic;
using Comdiv.Extensions;
using NUnit.Framework;

namespace Comdiv.Core.Test.StringProcessing{
    [TestFixture]
    public class TagHelperTest{
        private string teststr = "/a:/ /b:1/ /c:aaa/";
        [Test]
        public void parse(){
            var result = TagHelper.Parse(teststr);
            CollectionAssert.AreEquivalent(new Dictionary<string,string>{{"a",""},{"b","1"},{"c","aaa"}},result);
        }

        [Test]
        public void getvalue(){
            Assert.IsEmpty(TagHelper.Value(teststr,"a"));
            Assert.AreEqual("1",TagHelper.Value(teststr,"b"));
            Assert.AreEqual("aaa",TagHelper.Value(teststr,"c"));
        }

        [Test]
        public void set_existed_value(){
            var result = TagHelper.Parse(TagHelper.SetValue(teststr,"c","bbb"));
            CollectionAssert.AreEquivalent(new Dictionary<string,string>{{"a",""},{"b","1"},{"c","bbb"}},result);
        }


       [Test]
        public void set_non_existed_value(){
            var result = TagHelper.Parse(TagHelper.SetValue(teststr,"d","zzz"));
           CollectionAssert.AreEquivalent(new Dictionary<string,string>{{"a",""},{"b","1"},{"c","aaa"},{"d","zzz"}},result);
        }

        [Test]
        public void remove_tag(){
            var result = TagHelper.Parse(TagHelper.RemoveTag(teststr,"c"));
            CollectionAssert.AreEquivalent(new Dictionary<string,string>{{"a",""},{"b","1"}},result);
        }

        [Test]
        public void tostring(){
            var result = TagHelper.ToString(TagHelper.Parse(teststr));
            Assert.AreEqual(teststr,result);
        }

        [Test]
        public void merge(){
            var result = TagHelper.Merge(new Dictionary<string,string>{{"a",""},{"b","1"},{"c","aaa"}},new Dictionary<string,string>{{"x","2"},{"y","1"},{"c","bbb"}});
            CollectionAssert.AreEquivalent(new Dictionary<string,string>{{"a",""},{"b","1"},{"c","bbb"},{"x","2"},{"y","1"}},result);
        }
    }
}