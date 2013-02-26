namespace Comdiv.Text {
    public interface ITemplateParameterValue {
        string GetValue(string valuename, object datasource, string template);
    }
}