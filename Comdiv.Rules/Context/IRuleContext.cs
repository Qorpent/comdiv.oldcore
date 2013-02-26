namespace Comdiv.Rules.Context{
    public interface IRuleContext : IWithParameters<IRuleContext>{
        IDocumentProvider Docs { get; }
        IRuleDataProvider RuleData { get; }
    }
}