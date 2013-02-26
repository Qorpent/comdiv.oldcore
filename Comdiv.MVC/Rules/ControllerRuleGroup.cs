using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Application;
using Comdiv.Collections;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Rules.Context;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Rules;

namespace Comdiv.MVC.Rules{
    public class ControllerRuleGroup : RuleGroup, IControllerExpert{
        public const string RolesMapParamName = "RoleMap";
        public const string UserMapParamName = "UserMap";

        public ControllerRuleGroup(){
            UseLinearOneStepPattern();
        }

        #region IControllerExpert Members

        public StringMap RoleMap{
            get { return Params.Get(RolesMapParamName, new StringMap(), true); }
        }

        public StringMap UserMap{
            get { return Params.Get(UserMapParamName, new StringMap(), true); }
        }

        public virtual Func<IMvcContext, IControllerExpert, object> CustomDefaultResultRetriever{
            get { return Params.Get<Func<IMvcContext, IControllerExpert, object>>("CustomDefaultResultRetriever"); }
            set { Params["CustomDefaultResultRetriever"] = value; }
        }

        #endregion

        public static ControllerRuleGroup FromXml(XElement xml){
            var result = new ControllerRuleGroup();
            result.LoadFromXml(xml);
            return result;
        }

        public void LoadFromXml(XElement xml){
            AddRules(ControllerRule.FromXml(xml .XPathSelectElements("on")));
            xml.XPathSelectElements("map[@role]")
                .Select(n => new{From = n.attr("role").ToUpper(), To = n.attr("as").ToUpper()})
                .map(i => RoleMap.Add(i.From, i.To));
            xml.XPathSelectElements("map[@user]")
                .Select(n => new{From = n.attr("user").ToUpper(), To = n.attr("as").ToUpper()})
                .map(i => UserMap.Add(i.From, i.To));

            var def = xml.First("default");
            if (def.yes()){
                DefaultResult = def.Value;
            }
        }

        protected override bool innerTest(IRuleContext context){
            var baseResult = base.innerTest(context);
            if (baseResult){
                return true;
            }

            if (Missing.Value == DefaultResult){
                return false;
            }
            SetRawActivations(context, null);
            return true;
        }

        protected override void innerExecute(IRuleContext context){
            var activations = ExtractActivations(context);
            if (null == activations){
                var result = DefaultResult;
                if (CustomDefaultResultRetriever.yes()){
                    result = CustomDefaultResultRetriever(context.Descriptor(), this);
                }
                context.SetControllerRuleResult(result);
            }
            else{
                base.innerExecute(context);
            }
        }
    }
}