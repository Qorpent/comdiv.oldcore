using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules
{
	public abstract class RuleTemplated : RuleBase, IContextInitiator
	{
		private readonly object execLock = new object();
		private readonly object testLock = new object();
		public string contextIntiatedMark = "sys.contextinitiated";

		public virtual object Result{
			get { return Params.Get<object>("Result"); }
			set { Params["Result"] = value; }
		}
		public virtual bool IsResultSetter
		{
			get { return Params.Get<bool>("IsResultSetter"); }
			set { Params["IsResultSetter"] = value; }
		}


		public virtual string ResultContextParameter
		{
			get { return Params.Get<string>("ResultContextParameter"); }
			set { Params["ResultContextParameter"] = value; }
		}

		#region IContextInitiator Members

		public void InitContext(IRuleContext context){
			if (NeedInitiation(context))
				innerInitContext(context);
		}

		#endregion

		public override bool Test(IRuleContext context){
			////@"context".contract_NotNull(context);
			lock (testLock){
				Logger.Rule.Debug("START TEST {0}",Uid);
				context.addTest(this);
				var result = false;
				var proceed = preTest(context, out result);
				if (proceed)
					result = innerTest(context);
				postTest(context, result);
				if (!result) context.addBadTest(this);
				Logger.Rule.Debug("END TEST {0} - {1}", Uid,result);
				return result;
			}
		}

		protected virtual void postTest(IRuleContext context, bool result) {}


		protected virtual bool innerTest(IRuleContext context){
			return true;
		}

		protected virtual bool preTest(IRuleContext context, out bool result){
			result = true;
			return true;
		}

		public override void Execute(IRuleContext context){
			//@"context".contract_NotNull(context);

			lock (execLock){
				Logger.Rule.Debug("START EXEC {0}", Uid);
				var result = false;
				var proceed = preExecute(context);
				if (proceed){
					context.addExec(this);
					innerExecute(context);
				}
				postExecute(context, result);
				Logger.Rule.Debug("END EXEC {0}", Uid);
			}
		}

		protected virtual void postExecute(IRuleContext context, bool result) {}


		protected virtual void innerExecute(IRuleContext context){
			if(IsResultSetter){
				context.Params[ResultContextParameter] = GetResult(context);
			}
		}

		protected virtual bool preExecute(IRuleContext context){
			return true;
		}



		protected virtual void innerInitContext(IRuleContext context) {}

		private bool NeedInitiation(IRuleContext context){
			return !(bool) context.RuleData[this, contextIntiatedMark, false];
		}

		protected virtual object GetResult(IRuleContext context){
			return Result;
		}

		protected void UseResultSetterPattern(string targetParameter) {
			ResultContextParameter = targetParameter;
			IsResultSetter = true;
		}
	}
}