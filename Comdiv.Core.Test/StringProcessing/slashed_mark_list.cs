using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Comdiv.Extensions;
namespace Comdiv.Core.Test.StringProcessing {
    [TestFixture]
    public class slashed_mark_list {
        [Test]
        public void must_convert_strngs_to_mark_list(){
            //nulls must be cropped
            //empties must be cropped
            //duplicates removed
            Assert.AreEqual("/a/b/",SlashListHelper.ToString(new string[]{null,"","a","b","a"}));
        }

        [Test]
        public void must_convert_marks_to_list(){    
            CollectionAssert.AreEquivalent(new string[]{"a","b"},SlashListHelper.ReadList("/a/b/"));
        }

         [Test]
        public void mast_add_mark_to_set(){    
            Assert.AreEqual("/a/b/c/",SlashListHelper.SetMark("/a/b/","c"));
        }

		 [Test]
		 public void mast_add_mark_to_set_alt_delimiter()
		 {
			 Assert.AreEqual(";a;b;c;", SlashListHelper.SetMark(";a;b;", "c"));
		 }

        [Test]
        public void mast_remove_mark_from_set_alt_delimiter(){    
            Assert.AreEqual(";a;c;",SlashListHelper.RemoveMark(";a;b;c;","b"));
        }

        [Test]
        public void must_check_existense_of_mark(){    
            Assert.True(SlashListHelper.HasMark("/a/b/c/","c"));
        }

		[Test]
		public void must_check_existense_of_mark_alt_delimiter()
		{
			Assert.True(SlashListHelper.HasMark(";a;b;c;", "c"));
		}
    }
}
