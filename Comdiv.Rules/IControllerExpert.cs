using System;
using Comdiv.Collections;

using Comdiv.Rules;

namespace Comdiv.MVC.Rules{
    public interface IControllerExpert : IRule {
        StringMap RoleMap { get; }
        StringMap UserMap { get; }
        Func<IMvcContext, IControllerExpert, object> CustomDefaultResultRetriever { get; set; }
    }
}