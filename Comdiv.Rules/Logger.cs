using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Logging;
using Comdiv.Logging;

namespace Comdiv.Rules
{
	public class Logger
	{
		private static ILog rule;
		private static ILog kb;
		private static ILog engine;

		public static ILog Rule{
			get { return rule ??(rule = logger.get("Comdiv.Rules:RULE")); }
			set { rule = value; }
		}

		public static ILog Kb{
			get { return kb ?? (kb = logger.get("Comdiv.Rules:KB")); }
			set { kb = value; }
		}

		public static ILog Engine{
			get { return engine ?? (engine = logger.get("Comdiv.Rules:ENGINE")); }
			set { engine = value; }
		}
	}
}
