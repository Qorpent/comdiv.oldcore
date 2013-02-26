using System;
using Comdiv.Collections;

using Comdiv.Rules;

namespace Comdiv.MVC.Rules{
    public interface IControllerRule:IRule {
        StringMap RoleMap { get; }
        StringMap UserMap { get; }
        string Area { get; set; }
        string Controller { get; set; }
        string Category { get; set; }
        string Action { get; set; }
        string Url { get; set; }
        string Role { get; set; }
        string User { get; set; }
        string Parameters { get; set; }
        Func<IMvcContext, IControllerRule, bool> CustomApplyChecker { get; set; }
        Func<IMvcContext, IControllerRule, object> CustomResultRetriever { get; set; }
        bool UseDomain { get; set; }
        object Result { get; set; }
        bool IsResultSetter { get; set; }
        string ResultContextParameter { get; set; }
    }
}