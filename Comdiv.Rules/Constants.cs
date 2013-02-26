namespace Comdiv.Rules
{
	public static class Constants
	{
		#region Nested type: Context

		public static class Context
		{
			public const string BoundedRules = "sys.contextbound.rules";

			#region Nested type: RuleData

			public static class RuleData
			{
				public const string EngineHalt = "sys.halt";
				public const string EngineMaxSteps = "sys.engine.maxsteps";
				public const string GroupRawActivations = "sys.group.rawactivations";
			}

			#endregion
		}

		#endregion

		#region Nested type: Meta

		public static class Meta
		{
			public const string CRFriendshipIdentityString = "sys.resolver.friendship.identitystring";
			public const string CRFriendshipMatcher = "sys.resolver.friendship.mask";
			public const string CRSafeProduction = "sys.resolver.safeproduction";
			public const string IsContextBoundHandler = "sys.contextboundhandler";
			public const string IsTrigger = "sys.istrigger";
			public const string IsWithCountHints = "sys.counthints";
			public const string Module = "sys.module";
			public const string Priority = "sys.priority";
			public const string Uid = "sys.uid";
		}

		#endregion
	}
}