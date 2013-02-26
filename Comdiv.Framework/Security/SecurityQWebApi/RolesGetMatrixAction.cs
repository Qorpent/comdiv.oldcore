using System;
using System.IO;
using System.Xml.Linq;
using Qorpent.Bxl;
using Qorpent.Events;
using Qorpent.Mvc;


namespace Comdiv.Framework.Security.SecurityQWebApi
{
	[Action("roles.matrix",Role="ADMIN")]
	public class RolesGetMatrixAction : ActionBase
	{

		public RolesGetMatrixAction(IMvcContext context):this() {
			this.Transient = true;
			this.SetApplication(context.Application);
			this.SetContext(context);
		}

		class Roles_GetMatrixAction_Reseter:EventHandlerBase<ResetEvent> {
			private RolesGetMatrixAction a;
			public string Option;
			public string Type;
			public int Id;
			public DateTime LastMod {
				get { return a.lastmod; }
			}
			public Roles_GetMatrixAction_Reseter(RolesGetMatrixAction a)
			{
				this.a = a;
				this.Id = a.id;
				this.Option = "roles.matrix";
				this.Type = "GetMatrix_Reseter";
			}
			public override void Process(ResetEvent e) {
				if (e.Data.IsSet(Option)) {
					cached = null;
					a.lastmod = new DateTime(1900, 1, 1);
					e.Result.InvokeList.Add(new ResetResultInfo { Info = LastMod, Name = "Roles_GetMatrixAction_Reseter" });
				}
			}
		}

		private static int _id;
		private int id;
		public RolesGetMatrixAction() {
			id = _id++;
			
		}
		public override void SetApplication(Qorpent.Applications.IApplication app)
		{
 			 base.SetApplication(app);
			 if(!Transient) Application.Events.Add(new Roles_GetMatrixAction_Reseter(this));
		}

		DateTime lastmod = new DateTime(1900,1,1);
		protected override bool GetSupportNotModified() {
			return true;
		}
		protected override DateTime EvalLastModified() {
			return lastmod;
		}

		private static XElement cached = null;
		private bool Transient  = false;

		protected override object MainProcess() {
			cached = null; //STUB TO SIMPLIFY DOING FILE CHANGES DUE TO SOME REQUESTS (AVS)
			if (null == cached) {
				var matrixfile = ResolveFileName("roles.matrix.bxl");
				if (null == matrixfile) throw new Exception("roles.matrix.bxl file not configured");
				cached = new BxlParser().Parse(File.ReadAllText(matrixfile));
				lastmod = DateTime.Now;
			}
			return cached;
		}
	}
}
