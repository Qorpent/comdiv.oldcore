using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Persistence;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace Comdiv.Dependency.Test
{
    [TestFixture]
    public class bug_temporary_sessions_not_work
    {

        public const string DefaultConnection = "Data Source=testbd; Initial Catalog=testbd; Persist Security Info=True; User ID=testuser; Password=bgt5%TGB";
        public class test {
            public virtual int id { get; set; }
            public virtual string code { get; set; }
        }
        public class testmap : ClassMap<test> {
            public testmap() {
#if !LIB2
                WithTable("bug_temporary_sessions_not_work");
#else
                Table("bug_temporary_sessions_not_work");
#endif
                Id(x => x.id);
                Map(x => x.code);
            }
        }
        public class model : PersistenceModel {
            public model() {
                Add(new testmap());
            }
        }
        [Test]
        public void must_not_execute_any_code() {
            myapp.storage = null;
            myapp.ioc.Clear();
            myapp.ioc.setupHibernate(new NamedConnection("Default", DefaultConnection), new model());
            using(new TemporaryTransactionSession()) {
                var db = myapp.storage.Get<test>();
                var i = db.New();
                i.code = "dsds";
                db.Save(i);
            }
            using (var c = new SqlConnection(DefaultConnection))
            {
                c.Open();
                var com = c.CreateCommand();
                com.CommandText =
                    "select count(*) from bug_temporary_sessions_not_work";
                var r = com.ExecuteScalar().toInt();
                Assert.AreEqual(0,r,"Это значит, что реально временная сессия не использовалась!!!");
            }
        }
        [SetUp]
        public void setup() {
            using(var c = new SqlConnection(DefaultConnection)) {
                c.Open();
                var com = c.CreateCommand();
                com.CommandText =
                    "create table bug_temporary_sessions_not_work (id int identity(1,1), code nvarchar(255))";
                com.ExecuteNonQuery();
            }
        }
        [TearDown]
        public void teardown() {
            using (var c = new SqlConnection(DefaultConnection))
            {
                c.Open();
                var com = c.CreateCommand();
                com.CommandText =
                    "drop table bug_temporary_sessions_not_work";
                com.ExecuteNonQuery();
            }
        }
    }
}
