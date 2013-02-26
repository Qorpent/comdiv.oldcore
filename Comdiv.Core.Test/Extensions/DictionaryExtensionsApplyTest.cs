using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using NUnit.Framework;

namespace Comdiv.Core.Test.Extensions {
    [TestFixture]
    public class DictionaryExtensionsApplyTest {
        public interface ISampeInterface {
            
        }
        public class SampleClass2:ISampeInterface {
            
        }
        public class SampleClass {
            public SampleClass() {
                ReadonlyStringProp = "notest";
            }

           
            public string StringProp { get; set; }
            public string ReadonlyStringProp { get; protected set; } //Для проверки того, что не публичное не трогает
            public string ReallyReadonlyStringProp { get { return ""; } } //Проверка того, что не вальнет ошибку на попытке присвоения
            public bool BoolProp { get; set; }
            public int IntProp { get; set; }
            public decimal DecProp { get; set; }
            public DateTime DateProp { get; set; }
            public ISampeInterface SampleTypeProp { get; set; }

           

//проверяем, что можно присвоить значение свойства, по унаследованному интерфейсу
        }

        [Test]
        public void ss_simple_string() {
            var result = new Dictionary<string, string> {{"stringprop", "test"}}.apply(new SampleClass());
            Assert.AreEqual("test",result.StringProp);
        }
    
         [Test]
        public void ss_must_not_apply_protected() {
            var result = new Dictionary<string, string> {{"ReadonlyStringProp", "test"}}.apply(new SampleClass());
            Assert.AreNotEqual("notest",result.StringProp);
        }

          [Test]
        public void ss_must_not_error_on_readonly_props() {
              new Dictionary<string, string> {{"ReallyReadonlyStringProp", "test"}}.apply(new SampleClass());
          }

         [Test]
        public void ss_must_not_error_on_nonexisted_props() {
              new Dictionary<string, string> {{"NotExistedProp", "test"}}.apply(new SampleClass());
          }

        [Test]
        public void so_simple_string() {
            var result = new Dictionary<string, object> {{"stringprop", "test"}}.apply(new SampleClass());
            Assert.AreEqual("test",result.StringProp);
        }

        [Test]
        public void ss_bool_int_date_string() {
            var result = new Dictionary<string, string> {{"stringprop", "test"},{"intprop","2"},{"boolprop","true"},{"decprop","10.2"},{"dateprop","2011-08-01"}}.apply(new SampleClass());
            Assert.AreEqual("test",result.StringProp);
            Assert.AreEqual(2,result.IntProp);
            Assert.AreEqual(10.2,result.DecProp);
            Assert.AreEqual(true,result.BoolProp);
            Assert.AreEqual(new DateTime(2011,8,1),result.DateProp);
        }

        [Test]
        public void so_bool_int_date_string() {
            var result = new Dictionary<string, object> {{"stringprop", "test"},{"intprop",2.3m},{"boolprop",true},{"decprop","10.2"},{"dateprop","2011-08-01"}}.apply(new SampleClass());
            Assert.AreEqual("test",result.StringProp);
            Assert.AreEqual(2,result.IntProp);
            Assert.AreEqual(10.2,result.DecProp);
            Assert.AreEqual(true,result.BoolProp);
            Assert.AreEqual(new DateTime(2011,8,1),result.DateProp);
        }
    }
}
