using Comdiv.Rules;

namespace Comdiv.Rules.Support
{
	public class RuleServicesContainer : ServicesContainer
	{
		public RuleServicesContainer(IRule target){
			Target = target;
		}

		public RuleServicesContainer() {}

		public IRule Target { get; set; }

		protected override IServicesContainer findParentContainer(){
			IRule ancestor;
			if (null == Target) return null;
			while (null != (ancestor = Target.Parent)){
				if (ancestor is IWithServices)
					return ((IWithServices) ancestor).Services;
			}
			return null;
		}

		public static IWithServices GetServicesContainer(IRule rule){
			//@"rule".contract_NotNull(rule);
			var services = rule as IWithServices;
			while ((null == services && rule != null) || (null != services && rule != null && null == services.Services)){
				rule = rule.Parent;
				services = rule as IWithServices;
			}
			return services;
		}
	}
}