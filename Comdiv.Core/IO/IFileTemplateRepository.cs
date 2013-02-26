using System;

namespace Comdiv.IO {
    public interface IFileTemplateRepository {
        string GetTemplateJSON(string code);
        string GetCurrentFileTemplateJSON(string file);
        string GetTemplate(string code);
        void ApplyTemplate(string filename, string template, object datasource);
    }
}