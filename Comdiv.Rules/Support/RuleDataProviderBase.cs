using System.Collections.Generic;
using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Support
{
	public class RuleDataProviderBase : IRuleDataProvider
	{
		private IDictionary<string, IDictionary<string, object>> data;

		#region IRuleDataProvider Members

		public object this[IRule rule, string name, object def]{
			get { return this[rule.Uid(), name, def]; }
		}

		public object this[IRule rule, string name]{
			get { return this[rule, name, null]; }
			set { GetRuleStore(rule)[name] = value; }
		}

		public object this[string ruleUid, string name, object def]{
			get{
				//@"ruleUid".contract_HasContent(ruleUid);

				var store = GetRuleStore(ruleUid);
				if (!store.ContainsKey(name)) return def;
				return store[name];
			}
		}

		public object this[string ruleUid, string name]{
			get { return this[ruleUid, name, null]; }
			set { GetRuleStore(ruleUid)[name] = value; }
		}

		public S Get<S>(IRule rule, string name, S def){
			return (S) this[rule, name, def];
		}

		public S Get<S>(IRule rule, string name){
			return (S) this[rule, name, default(S)];
		}

		public S Get<S>(string ruleUid, string name, S def){
			return (S) this[ruleUid, name, def];
		}

		public S Get<S>(string ruleUid, string name){
			return (S) this[ruleUid, name, default(S)];
		}

		#endregion

		protected IDictionary<string, object> GetRuleStore(IRule rule){
			return GetRuleStore(rule.Uid());
		}

		protected IDictionary<string, object> GetRuleStore(string ruleUid){
			if (data == null) data = new Dictionary<string, IDictionary<string, object>>();
			if (!data.ContainsKey(ruleUid))
				data[ruleUid] = new Dictionary<string, object>();
			return data[ruleUid];
		}
	}
}