using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Comdiv.Application;
using Comdiv.Audit;
using Comdiv.Inversion;
using Comdiv.Model;
using Comdiv.Persistence;

namespace Comdiv.MAS
{
    ///<summary>
    ///</summary>
    [Schema("mas")]
    public class TestResult : IAuditTestResult,IParametersProvider
    {
        public TestResult() {
            Fixed = false;
            CanBeIgnored = true;
            Ignored = false;
            Level = 3;
            XmlData = "<empty/>";
            Active = true;
            System = Environment.MachineName;
            Version = DateTime.Now;
            AutoFix = "";
            Link = "";
            Name = "";
            Code = Guid.NewGuid().ToString();
            Type = "/UNTYPED";
            Test = "UNKNOWN";
            TestProviderId = 0;
           
        }
        public virtual int Id { get; set; }
        [Param]
        public virtual string Code { get; set; }
        [Param]
        public virtual  string Name { get; set; }
        [Map]
        [Param]
        public virtual string Type { get; set; }
        [Map]
        [Param]
        public virtual int Level { get; set; }
        [Map]
        [Param]
        public virtual string Message { get; set; }
        [Map]
        [Param]
        public virtual string XmlData { get; set; }
        [Map]
        [Param]
        public virtual bool CanBeIgnored { get; set; }
        [Map]
        [Param]
        public virtual bool Ignored { get; set; }
        [Map]
        [Param]
        public virtual bool Fixed { get; set; }
        [Map]
        [Param]
        public virtual bool Active { get; set; }
        [Map]
        [Param]
        public virtual string Test { get; set; }
        [Map]
        [Param]
        public virtual string System { get; set; }
        [Map]
        [Param]
        public virtual string Var1 { get; set; }
        [Map]
        [Param]
        public virtual string Var2 { get; set; }
        [Map]
        [Param]
        public virtual string Var3 { get; set; }
        [Map]
        [Param]
        public virtual string Var4 { get; set; }

        [Map]
        [Param]
        public virtual string Link { get; set; }
        [Map]
        [Param]
        public virtual string AutoFix { get; set; }
        [Map]
        [Param]
        public virtual string Application { get; set; }
        [Map("ProviderId")]
        public virtual TestProvider TestProvider { get; set; }

        [Param]
        public virtual int TestProviderId { get; set; }

        public virtual DateTime Version { get; set; }

        public static void Deactivate(TestResult matrix) {
            if(null!=matrix.TestProvider) {
                matrix.TestProviderId = matrix.TestProvider.Id();
            }
            using(var c = myapp.getConnection("mas")) {
                c.WellOpen();
                c.ExecuteNonQuery("exec mas.audit_deactivate @system=@system, @application=@application, @type=@type, @level=@level",matrix);
            }
        }
        public static void Define(TestResult matrix) {
            if (null != matrix.TestProvider)
            {
                matrix.TestProviderId = matrix.TestProvider.Id();
            }
            using (var c = myapp.getConnection("mas")) {
                c.WellOpen();
                c.ExecuteNonQuery("insert mas.testresultupdate (code, name, version, type, level, message, xmldata, canbeignored, ignored, fixed, active, test, system, application, providerid, var1, var2, var3, var4, link, autofix) "+
                "values (@code, @name, getdate(), @type, @level, @message, @xmldata, @canbeignored, @ignored, @fixed, @active, @test, @system, @application, @testproviderid, @var1, @var2, @var3, @var4, @link, @autofix)", matrix);
            }
        }

        public virtual bool UseCustomMapping
        {
            get { return false; }
        }

        public virtual IDictionary<string, object> GetParameters()
        {
            throw new NotImplementedException();
        }
    }
}