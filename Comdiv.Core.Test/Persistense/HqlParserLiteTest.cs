using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Persistence;
using NUnit.Framework;

namespace Comdiv.Core.Test.Persistense
{
    [TestFixture]
    public class HqlParserLiteTest
    {
        HqlParseResult get(string hql) {
            return new HqlParserLite().Parse(hql);
        }
        void processed(string hql) {
            var result = get(hql);
            Assert.True(result.Processed,hql);
        }
        void noprocessed(string hql)
        {
            var result = get(hql);
            Assert.False(result.Processed, hql);
        }
        void issimple(string hql)
        {
            var result = get(hql);
            Assert.True(result.IsSimple, hql);
        }
        void isnotsimple(string hql)
        {
            var result = get(hql);
            Assert.False(result.IsSimple, hql);
        }
		
        [Test]
        public void PROCESSED_FALSE_on_no_from_queries() {
            noprocessed("nofrom table");
            
        }

		[Test]
		public void NOT_PARSED_TABLE_FROM_JOIN() {
			var result =
				get(
					"select x from obj x left join x.Parent p where  '' = 'Уралэ' or  x.Name like '%Уралэ%' or x.Code like 'Уралэ%'  or  p.Name like '%Уралэ%'");
			Assert.AreEqual("obj",result.TableName);

		}

        [Test]
        public void PROCESSED_TRUE_on_valid_query()
        {
            processed("from table");
            processed("from table x");
            processed("from table x, table2 as x");
            processed("select Id from table");
            processed("from table where Id = 1");
            processed("select Id from table where Id = 1");
        }

        [Test]
        public void ISSIMPLE_works_well()
        {
            issimple("from table");
            issimple("from table x");
            issimple("from table as x");
            isnotsimple("from table join table2");
            isnotsimple("from table, table2");
            
            
        }

        [Test]
        public void FIELDS_works() {
            var result = get("select Id,Code from table");
            CollectionAssert.AreEquivalent(new[]{"Id","Code"},result.Fields);
            result = get("select Id,x.Code, Name from table x");
            CollectionAssert.AreEquivalent(new[] { "Id", "Code", "Name" }, result.Fields);
        }

    }
}
