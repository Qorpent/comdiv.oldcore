using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Comdiv.Application;
using Comdiv.Inversion;
using Comdiv.Persistence;
using NUnit.Framework;

namespace Comdiv.MAS
{
    [TestFixture]
    public class DefaultProcessCreationTest
    {
        private DefaultMasProcessRepository repo;

        [SetUp]
        public void setup()
        {

            myapp.ioc.Clear();
            myapp.ioc.setupMas();
            myapp.ioc.setupHibernate();
            this.repo = (DefaultMasProcessRepository)myapp.ioc.get<IMasProcessRepository>();

        }

        [TearDown]
        public void tear()
        {

            var s = myapp.ioc.getSession("mas");
            if (null != s)
            {
                s.Clear();
            }
        }

        [Test]
        public void can_start_default_process_record()
        {
            var p = Process.CreateForCurrentProcess();
            p.Code = Guid.NewGuid().ToString();
            var result = repo.Start(p);
            Assert.AreNotEqual(0, result.Id);
        }

        [Test]
        public void can_finish_default_process_record()
        {
            var p = Process.CreateForCurrentProcess();
            p.Code = Guid.NewGuid().ToString();
            var result = repo.Start(p);
            result.Result = 1;
            repo.Finish(result);
            result = repo.Get(new Process { Id = result.Id });
            Assert.AreEqual(1, result.Result);
            Assert.AreEqual(false, result.IsActive);
        }

        [Test]
        public void can_pick_messages()
        {
            var p = Process.CreateForCurrentProcess();
            p.Code = Guid.NewGuid().ToString();
            var result = repo.Start(p);
            repo.Send(new ProcessMessage { Process = result, Message = "test1" });
            Thread.Sleep(100);
            repo.Send(new ProcessMessage { Process = result, Message = "test2" });
            var m1 = repo.GetNext(result, null);
            Assert.AreEqual("test1", m1.Message);
            var m2 = repo.GetNext(result, null);
            Assert.AreEqual("test1", m2.Message);
            m2 = repo.GetNext(result, m1);
            Assert.AreEqual("test2", m2.Message);
        }

        [Test]
        public void can_create_child_apptype()
        {
            AppType apptype1 = null;
            AppType apptype2 = null;
            var storage = myapp.storage.Get(typeof(AppType), "mas", true);
            apptype1 = new AppType();
            storage.Save(apptype1);
            apptype2 = new AppType();
            storage.Save(apptype2);
            apptype2.Parent = apptype1;
            storage.Save(apptype2);
            myapp.ioc.getSession("mas").Clear();
            var testtype = storage.Load<AppType>(apptype2.Id);
            Assert.AreEqual(testtype.Parent.Id, apptype1.Id);
        }
    }
}
