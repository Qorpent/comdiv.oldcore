using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Extensions;
using Qorpent.Events;
using Qorpent.Mvc;
using Qorpent.Mvc.Binding;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	public abstract class RolesActionBase : ActionBase
	{

        protected IRoleApplyer applyer { get; set; }

        protected string file { get; set; }

        protected XElement matrix { get; set; }

        [Bind()]
        protected string target { get; set; }

        [Bind( UpperCase = true)]
        protected string role { get; set; }

        [Bind( LowerCase = true)]
        protected string user { get; set; }

        protected override void Prepare()
        {
            base.Prepare();
            this.matrix = (XElement)new RolesGetMatrixAction(Context).Process();
            if (null == matrix) throw new RolesActionException(this.GetType().Name + " matrix not defined");
            if (!string.IsNullOrWhiteSpace(target)) {
                this.file =
                    ConvertExtensions.toStr(
                        matrix.XPathEvaluate("string(//targets/target[@code='" + target + "']/@file)"));
                if (file.noContent())
                    throw new RolesActionException(this.GetType().Name +
                                                   " target or file not defined in matrix not defined");
                file = ResolveFileName(file);
                if (null == file) throw new RolesActionException(this.GetType().Name + " file not exists");
                this.applyer = RoleApplyer.CreateByFileName(file);
            }
        }

		protected override object MainProcess() {
           
		   
			Application.Events.Call<ResetEventResult>(new ResetEventData(new[]{"sp.roles.resover"}),Application.Principal.CurrentUser );
			return true;
		}

		
	}
}