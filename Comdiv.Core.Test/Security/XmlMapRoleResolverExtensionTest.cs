using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Security;
using NUnit.Framework;

namespace Comdiv.Core.Test.Security {
    [TestFixture]
    public class XmlMapRoleResolverExtensionTest {
        protected IRoleProvider r;

        private string code =
            @"<root>
    <role name='A'/>
    <role name='C'/>
    <map role='A' as='B' />
    <map role='B' as='D' />
    <map role='D' as='E' />
    <map role='C' as='F' />
    <map user='D\X' as='A' />
    <map user='D\Y' as='B' />
    <map user='G\X' as='B' />
    <map user='D' as='C' />
</root>";
  
        [SetUp]
        public virtual void setup(){
            this.r = new XmlRoleProvider<ApplicationXmlReader>(new ApplicationXmlReader(code));
                                                    
        }
     
        [Test]
        public void simple_mapping_usr_to_role_works(){
            Assert.True(r.IsInRole(@"D\X".toPrincipal(),"A"));   
            Assert.True(r.IsInRole(@"D\Y".toPrincipal(),"B"));  
            Assert.False(r.IsInRole(@"D\Y".toPrincipal(),"A"));  
        }
        [Test]
        public void deep_level_mapped_usr_to_tole_works(){
            Assert.True(r.IsInRole(@"D\X".toPrincipal(),"D"));   
            Assert.True(r.IsInRole(@"D\Y".toPrincipal(),"D")); 
        }

        [Test]
        public void domain_level_roles_resolved(){
            Assert.True(r.IsInRole(@"D\X".toPrincipal(),"C"));   
            Assert.False(r.IsInRole(@"G\X".toPrincipal(),"C")); 
        }

        [Test]
        public void domain_level_on_deep_mapping_roles_resolved(){
            Assert.True(r.IsInRole(@"D\X".toPrincipal(),"F"));   
        }

        protected string[] rolelist = new[]{"A", "C"};
        [Test]
        public void roles_list_filled(){
            CollectionAssert.AreEquivalent(rolelist,r.GetRoles());
        }

        [Test]
        [Category("INTEGRATION")]
        public void file_loading_test(){
            prepareIntegrationSource();
            simple_mapping_usr_to_role_works();
            deep_level_mapped_usr_to_tole_works();
            domain_level_roles_resolved();
            domain_level_on_deep_mapping_roles_resolved();
            roles_list_filled();

        }

        protected virtual void prepareIntegrationSource(string name =  "file_loading_test_xml"){
            var root =(this.GetType()+"_"+name).prepareTemporaryDirectory();
            ((DefaultFilePathResolver) myapp.files).Root = root;
            this.r = new XmlRoleProvider<ApplicationXmlReader>();
            myapp.files.Write("~/usr/security.map.config",@"<root>
    <role name='A'/>
    <role name='C'/>
    <map role='A' as='B' />
    <map role='B' as='D' />
    <map role='D' as='E' />
    <map role='C' as='F' />
    <map user='D\X' as='A' />
    <map user='D\Y' as='B' />
    <map user='G\X' as='B' />
    <map user='D' as='C' />
</root>");
        }
    }
}
