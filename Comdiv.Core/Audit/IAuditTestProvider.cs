using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Model.Interfaces;

namespace Comdiv.Audit
{
    public interface IAuditTestProvider
    {
       IAuditTest[] GetAll();
       IAuditTest Get(string  code);
        IAuditTest[] Search(string pattern);
    }

    public interface IAuditTest
    {
        string Code { get; set; }
        IAuditTestResult[] Execute(string pattern, int mintestlevel);
    }

    public interface IAuditTestResult:IWithId,IWithCode,IWithName,IWithVersion
    {
        string Type { get; set; }
        int Level { get; set; }
        string Message { get; set; }
        string XmlData { get; set; }
        bool CanBeIgnored { get; set; }
        bool Ignored { get; set; }
        bool Fixed { get; set; }
        bool Active { get; set; }
        string Test { get; set; }
        string System { get; set; }
        string Application { get; set; }
    }

}
