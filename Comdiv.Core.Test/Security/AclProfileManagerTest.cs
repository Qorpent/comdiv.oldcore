using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using NUnit.Framework;
using Comdiv.IO;

namespace Comdiv.Security.Tests
{
    [TestFixture]
    public class AclProfileManagerTest
    {
        
        [Test]
        public void exists_in_default_security_config(){
            ioc.finish();
            myapp.reload(5);
            ioc.Container.setupSecurity();
            ioc.get<IAclProfileManager>();
        }

        [Test]
        public void enumeration_works()
        {
            ioc.finish();
            ioc.Container.setupSecurity().setupFilesystem();
            prepareProfiles();
            var m = ioc.get<IAclProfileManager>();
            Assert.AreEqual(5, m.Enumerate().Count());
            Assert.AreEqual("c1", m.Enumerate().First(x => x.Code == "t1").Comment);
        }
        [Test]
        public void activation_and_get_current_works()
        {
            ioc.finish();
            ioc.Container.setupSecurity().setupFilesystem();
            prepareProfiles();
            var m = ioc.get<IAclProfileManager>();
            myapp.files.Delete("~/usr/acl.config");
            Assert.Null(m.GetCurrent());
            m.Activate("t3");
            Assert.AreEqual("t3", m.GetCurrent().Code);
            Assert.AreEqual("~/usr/acl.config", m.GetCurrent().Name);
            Assert.AreEqual("c3", m.GetCurrent().Comment);
            
        }
        [Test]
        public void save_as_works(){
            ioc.finish();
            ioc.Container.setupSecurity().setupFilesystem();
            myapp.files.NoUseCache = true;
            myapp.reload(5);
			Assert.NotNull(myapp.files);
            prepareProfiles();
            myapp.files.Delete("~/usr/aclprofiles/test.acl.config");
            Assert.False(myapp.files.Exists("~/usr/aclprofiles/test.acl.config"));
            var m = ioc.get<IAclProfileManager>();
            m.Activate("t3");
            m.SaveCurrentAs("test", "commenz");
            Assert.True(myapp.files.Exists("~/usr/aclprofiles/test.acl.config"));
            Assert.AreEqual("commenz", m.Enumerate().First(x => x.Code == "test").Comment);
            Assert.AreEqual("~/usr/aclprofiles/test.acl.config".mapPath().normalizePath(), m.Enumerate().First(x => x.Code == "test").Name.normalizePath());
        }

        private void prepareProfiles() {
            myapp.files.NoUseCache = true;
            myapp.files.DeleteDirectory("mod/aclprofiles");
            myapp.files.DeleteDirectory("usr/aclprofiles");
            myapp.files.DeleteDirectory("sys/aclprofiles");
            myapp.files.Delete("~/usr/acl.config");
            myapp.files.Write("usr/aclprofiles/1.acl.config", "<rules><name>t1</name><comment>c1</comment></rules>");
            myapp.files.Write("usr/aclprofiles/2.acl.config", "<rules><name>t2</name><comment>c2</comment></rules>");
            myapp.files.Write("mod/aclprofiles/1.acl.config", "<rules><name>t3</name><comment>c3</comment></rules>");
            myapp.files.Write("mod/aclprofiles/2.acl.config", "<rules><name>t4</name><comment>c2</comment></rules>");
            myapp.files.Write("sys/aclprofiles/1.acl.config", "<rules><name>t1</name><comment>c</comment></rules>");
            myapp.files.Write("sys/aclprofiles/2.acl.config", "<rules><name>t5</name><comment>c5</comment></rules>");
        }
		[Test]
		public void usual_enumetation_works(){
			ioc.finish();
		    ioc.Container.setupFilesystem();
			prepareProfiles();
			var s = myapp.files.ResolveAll("aclprofiles","*.*");
			Console.WriteLine(s.concat(";"));
			Assert.AreEqual(6, s.Count());
		}
		
		
    }
}
