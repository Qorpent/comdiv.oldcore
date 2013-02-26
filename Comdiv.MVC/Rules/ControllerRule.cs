using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
using Comdiv.MVC.Resources;
using Comdiv.Persistence;
using Comdiv.Rules;
using Comdiv.Rules.Context;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Rules;

namespace Comdiv.MVC.Rules{
    public class ControllerRule : RuleTemplated, IControllerRule{
        public ControllerRule(){
            UseResultSetterPattern(ControllerRuleStrings.Default.Context_Result);
        }

        #region IControllerRule Members

        public StringMap RoleMap{
            get { return Params.GetParented<StringMap>(ControllerRuleGroup.RolesMapParamName); }
        }

        public StringMap UserMap{
            get { return Params.GetParented<StringMap>(ControllerRuleGroup.UserMapParamName); }
        }

        #endregion

        #region PARAMETERS

        public virtual string Domain{
            get { return Params.Get<string>("Domain"); }
            set { Params["Domain"] = value; }
        }

        public virtual string Area{
            get { return Params.Get<string>("Area"); }
            set { Params["Area"] = value; }
        }

        public virtual string Controller{
            get { return Params.Get<string>("Controller"); }
            set { Params["Controller"] = value; }
        }

        public virtual string Category{
            get { return Params.Get<string>("Category"); }
            set { Params["Category"] = value; }
        }

        public virtual string Action{
            get { return Params.Get<string>("Action"); }
            set { Params["Action"] = value; }
        }

        public virtual string Url{
            get { return Params.Get<string>("Url"); }
            set { Params["Url"] = value; }
        }

        public virtual string Role{
            get { return Params.Get<string>("Role"); }
            set { Params["Role"] = value; }
        }

        public virtual string User{
            get { return Params.Get<string>("User"); }
            set { Params["User"] = value; }
        }

        public virtual string Parameters{
            get { return Params.Get<string>("Parameters"); }
            set { Params["Parameters"] = value; }
        }

        public virtual Func<IMvcContext, IControllerRule, bool> CustomApplyChecker{
            get { return Params.Get<Func<IMvcContext, IControllerRule, bool>>("CustomApplyChecker"); }
            set { Params["CustomApplyChecker"] = value; }
        }

        public virtual Func<IMvcContext, IControllerRule, object> CustomResultRetriever{
            get { return Params.Get<Func<IMvcContext, IControllerRule, object>>("CustomResultRetriever"); }
            set { Params["CustomResultRetriever"] = value; }
        }

        public virtual bool UseDomain{
            get { return Params.Get<bool>("UseDomain"); }
            set { Params["UseDomain"] = value; }
        }

        #endregion

        protected override bool innerTest(IRuleContext context){
            var ctx = context.Descriptor();
            if (null == ctx){
                return false;
            }

            if (CustomApplyChecker.yes()){
                return CustomApplyChecker(ctx, this);
            }

            if (Area.yes() && ctx.Area.ToUpper() != Area.ToUpper()){
                return false;
            }
            if (Category.yes() && !ctx.Category.like(Category, RegexOptions.IgnoreCase)){
                return false;
            }
            if (Controller.yes() && ctx.Name.ToUpper() != Controller.ToUpper()){
                return false;
            }
            if (Action.yes() && ctx.Action.ToUpper() != Action.ToUpper()){
                return false;
            }
            if (Domain.yes() && ctx.User.Identity.Name.toDomain().ToUpper() != Domain.ToUpper()){
                return false;
            }
            if (Role.yes() && !CheckIsUserInRole(ctx)){
                return false;
            }
            //FIXED: TODO : Нужна все таки проверка домена - сделано через дополнительный параметр UseDomain bool, default False
            if (User.yes() &&
                !(ctx.User.toUserName(UseDomain).ToUpper() == User.ToUpper())){
                return false;
            }
            if (Url.yes() && !ctx.Url.like(Url, RegexOptions.IgnoreCase)){
                return false;
            }
            if (Parameters.yes() && !ctx.ParametersString.like(Parameters, RegexOptions.IgnoreCase)){
                return false;
            }

            return true;
        }


        private bool CheckIsUserInRole(IMvcContext ctx){
            var roles = Role.Split(',', ';');
            foreach (var role in roles){
                if (_checkRole(ctx, role)){
                    return true;
                }
            }
            return false;
        }

        private bool _checkRole(IMvcContext ctx, string role){
            var realRoleTest = role;
            var wantedResult = true;
            if (realRoleTest.StartsWith("!")){
                wantedResult = false;
                realRoleTest = realRoleTest.Substring(1);
            }
            if (wantedResult == myapp.roles.IsInRole(ctx.User, realRoleTest)){
                return true;
            }

            if (null != UserMap &&
                (wantedResult == role.isIn(UserMap[ctx.User.toUserName(UseDomain)]))
                ){
                return true;
            }
            return false;
        }

        #region UTILITY

        protected override object GetResult(IRuleContext context){
            var result = base.GetResult(context);
            if (CustomResultRetriever.yes()){
                result = CustomResultRetriever((IMvcContext) context, this);
            }
            return result;
        }

        public static IEnumerable<IRule> FromXml(IEnumerable<XElement> rules){
            var priority = -1000;

            foreach (var r in rules){
                
            
                var rule = new ControllerRule();

                rule.SetPriority(priority);

                rule.Area = r.attr("area");
                rule.Controller = r.attr("controller");
                rule.Action = r.attr("action");
                rule.Url = r.attr("url");
                rule.Parameters = r.attr("params");
                rule.Role = r.attr("role");
                rule.User = r.attr("user");
                rule.Category = r.attr("category");
                rule.Domain = r.attr("domain");
                rule.UseDomain = r.attr("usedomain").toBool();

                string directPriority = r.attr("order");
                if (directPriority.hasContent()){
                    rule.SetPriority(-(int.Parse(directPriority)));
                }

                string custom = null;
                if ((custom = r.attr("custom")).yes()){
                    var customHolder = custom.toType().create<ICustomControllerRuleTester>();
                    rule.CustomApplyChecker = customHolder.CustomTester;
                }
                rule.Result = r.Value;

                yield return rule;

                priority -= 10;
            }
        }

        #endregion
    }
}