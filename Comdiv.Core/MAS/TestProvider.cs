using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Audit;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.MAS
{
    [Schema("mas")]
    public class TestProvider : IWithId, IWithCode, IWithVersion,IWithName
    {
        public virtual int Id { get; set; }
        public virtual DateTime Version { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        [Map]
        public virtual string Class { get; set; }
        [Map]
        public virtual string Config { get; set; }
        [Map("ProviderId", Cascade = true)]
        public virtual IList<TestResult> Items { get; set; }
        public virtual IDictionary<IAuditTest,TestResult[]> Execute(string testpattern, string resultpattern, int level, IConsoleLogHost logger)
        {
            var realprovider = Class.toType(new Dictionary<string,Type>
                                                {
                                                    {"dbset",typeof(DatabaseSetTestProvider)}
                                                }).create<IMasAuditTestProvider>();
            realprovider.Contextualize(this,logger);
            var tests = realprovider.Search(testpattern);
            var result = new Dictionary<IAuditTest, TestResult[]>();
            foreach (var auditTest in tests)
            {
                result[auditTest] = auditTest.Execute(resultpattern, level).OfType<TestResult>().ToArray();
            }
            return result;
        }
    }
}