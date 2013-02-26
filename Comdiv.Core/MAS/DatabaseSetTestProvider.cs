using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Audit;
using Comdiv.Extensions;
using Qorpent.Bxl;

namespace Comdiv.MAS
{
    public class DatabaseSetTestProvider : IMasAuditTestProvider
    {
        private XElement cfg;
        private string connection;
        private string schema;
        private TestProvider provider;
        private IConsoleLogHost logger;
        private string db;
        private DatabaseSetTest[] tests;

        public IAuditTest[] GetAll()
        {
            return tests;
        }

        public IAuditTest Get(string code)
        {

            return tests.FirstOrDefault(x => x.Code == code);
        }

        public IAuditTest[] Search(string pattern)
        {
            if (pattern.noContent()) return GetAll();
            return tests.Where(x => x.Code.like(pattern)).ToArray();
        }


        public class DatabaseSetTest:IAuditTest
        {
            private DatabaseSetTestProvider provider;

            public DatabaseSetTest(DatabaseSetTestProvider databaseSetTestProvider, string code)
            {
                this.provider = databaseSetTestProvider;
                this.Code = code;
            }

            public string Code { get; set; }

            public IAuditTestResult[] Execute(string pattern, int mintestlevel)
            {
                provider.logger.loginfo("enter into "+this.Code);
                myapp.sql.ExecuteNoQuery("exec "+provider.schema+".["+Code+"] @provider=@provider, @pattern = @pattern, @minlevel=@minlevel",
                                         new Dictionary<string, object>{{"provider",provider.provider.Code},{"pattern",pattern},{"minlevel",mintestlevel}},
                                         provider.db,
                                         provider.connection
                    );
                provider.logger.loginfo("leave " + this.Code);
                
                return new IAuditTestResult[] {};
            }
        }

        public void Contextualize(TestProvider provider, IConsoleLogHost logger)
        {
            this.provider = provider;
            this.logger = logger;
            this.cfg = new BxlParser().Parse(provider.Config);
            this.connection = cfg.elementId("connection","Default");
            this.schema = cfg.elementId("schema", "masaudit");
            this.db = cfg.elementId("db", (string)null);
            logger.loginfo("get test procedures from "+connection+"/"+db+"/"+schema);
            var t =
                myapp.sql.ExecuteTable(
                    "select name from sys.objects where type='P' and schema_id = schema_id('" + schema + "')", null, db,
                    connection);
            IList<DatabaseSetTest> procedures = new List<DatabaseSetTest>();
            foreach (var r in t.Rows)
            {
                logger.logtrace(r.Values[0] as string);
                procedures.Add(new DatabaseSetTest(this,r.Values[0] as string));
            }

            this.tests = procedures.ToArray();
        }

        
    }
}