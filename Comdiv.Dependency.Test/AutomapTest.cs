using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Mapping;
using FluentNHibernate;
using NUnit.Framework;

namespace Comdiv.Dependency.Test
{
[Schema("test")]
    public class AutomapTestClass:IWithId {
        public virtual int Id { get; set; }

        [Map]
        public string TestProp { get; set; }

        [Map("Parent")]
        public virtual AutomapTestClass Parent { get; set; }

        [Map("Parent")]
        public virtual IList<AutomapTestClass> Children { get; set; }
    }

    public class AutomapTestModel:PersistenceModel {
        public AutomapTestModel() {
            this.Add(new Automap<AutomapTestClass>());
        }
    }

    [TestFixture]
    public class AutomapTest
    {
        [Test]
        public void can_configure() {
            var temp = "automaptest".prepareTemporaryDirectory();
            new AutomapTestModel().WriteMappingsTo(temp);
        } 
    }
}
