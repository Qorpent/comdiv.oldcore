namespace Comdiv.Persistence {
    public interface IHqlColumnValueProvider {
        object GetValue(HqlColumn column, object target);
    }
}