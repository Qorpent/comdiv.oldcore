using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Model;
using Comdiv.Model.Mapping;
using NUnit.Framework;

namespace Comdiv.Dependency.Test {
    [TestFixture]
    public  class new_attribute_based_mapping {
        public interface I {
            [Map("ip1")]
            string IP { get; set; }
        }
       
        public class C:I{
            public virtual string IP { get; set; }
            [Map("ip2")]
            public virtual string CP { get; set; }
            public virtual string NP { get; set; }
        }
#if !LIB2
        [Test]
        public void must_map_by_direct_property(){
            var map = new ExtendedClassMap<C>();
            map.standard();
            var p = map.GetAllProperties().First(x => x.Property.Name == "CP");
            Assert.AreEqual("ip2", p.ColumnNames.List().FirstOrDefault());
        }
         [Test]
        public void must_not_map_not_mapped_property(){
            var map = new ExtendedClassMap<C>();
            map.standard();
            Assert.Null(map.GetAllProperties().FirstOrDefault(x => x.Property.Name == "NP"));
        }

        [Test]
        public void must_map_by_interface_property(){
            var map = new ExtendedClassMap<C>();
            map.standard();
            var p = map.GetAllProperties().First(x => x.Property.Name == "IP");
            Assert.AreEqual("ip1", p.ColumnNames.List().FirstOrDefault());
        }
#else
        [Test]
        public void must_map_by_direct_property()
        {
            var map = new ExtendedClassMap<C>();
            map.standard();
            var p = map.GetAllProperties().First(x => x.Name == "CP");
            Assert.AreEqual("ip2", p.Columns.FirstOrDefault().Name);
        }
        [Test]
        public void must_not_map_not_mapped_property()
        {
            var map = new ExtendedClassMap<C>();
            map.standard();
            Assert.Null(map.GetAllProperties().FirstOrDefault(x => x.Name == "NP"));
        }

        [Test]
        public void must_map_by_interface_property()
        {
            var map = new ExtendedClassMap<C>();
            map.standard();
            var p = map.GetAllProperties().First(x => x.Name == "IP");
            Assert.AreEqual("ip1", p.Columns.FirstOrDefault().Name);
        }
#endif
    }
}
