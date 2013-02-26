

namespace Comdiv.Rules.Context{
    public interface IRuleDataProvider{
        object this[IRule rule, string name, object def] { get; }
        object this[IRule rule, string name] { get; set; }

        object this[string ruleUid, string name, object def] { get; }
        object this[string ruleUid, string name] { get; set; }

        S Get<S>(IRule rule, string name, S def);
        S Get<S>(IRule rule, string name);

        S Get<S>(string ruleUid, string name, S def);
        S Get<S>(string ruleUid, string name);
    }
}