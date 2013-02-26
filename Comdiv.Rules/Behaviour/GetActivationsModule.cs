using System.Collections.Generic;
using Comdiv.Rules;
using Comdiv.Rules.Context;
using Comdiv.Rules.KnowlegeBase;

namespace Comdiv.Rules.Activation
{
	public abstract class GetActivationsModule : ITargetedModule<IRuleCollection>, IGetActivationsModule
	{
        public IRuleCollection Target { get; set; }
		#region IGetActivationsModule Members

		public abstract IList<IRule> Filter(IRuleContext context, IList<IRule> ruleset);

		#endregion
	}
}