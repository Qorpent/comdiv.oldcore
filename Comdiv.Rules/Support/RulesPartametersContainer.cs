using Comdiv.Rules;

namespace Comdiv.Rules.Support
{
	public class RulesPartametersContainer : ParametersContainer<IRule>
	{
		public RulesPartametersContainer(IRule target){
			Target = target;
		}

		protected override object resolveParentOrDef(string name,object def)
		{
			if(null==Target.Parent) return def;
			return Target.Parent.Params[name, def];
		}
	}
}