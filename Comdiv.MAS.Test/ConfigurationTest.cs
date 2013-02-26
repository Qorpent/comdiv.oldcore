using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Application;
using NUnit.Framework;

namespace Comdiv.MAS
{
    [TestFixture]
    public class ConfigurationTest
    {
        [SetUp]
        public void setup() {
            myapp.ioc.Clear();
        }
        /// <summary>
        /// NOTE: need setup environment!!!
        /// </summary>
        [Test]
        public void must_setup_on_valid_database() {
            myapp.ioc.setupMas(new MasConfiguration {System = "mas", Database = "mas", Server = "MASSERVER",CheckDatabase = true});
        }

        /// <summary>
        /// NOTE: need setup environment!!!
        /// </summary>
        [Test]
        public void must_setup_on_invalid_database_if_check_switched_off()
        {
            myapp.ioc.setupMas(new MasConfiguration { System = "mas", Database = "nomas", Server = "MASSERVER" , CheckDatabase = false});
        }

        [Test]
        public void must_fail_on_invalid_database()
        {
            Assert.Throws<MasSetupException>(()=>myapp.ioc.setupMas(new MasConfiguration { System = "mas", Database = "nomas", Server = "MASSERVER",CheckDatabase = true}));
        }
    }
}
