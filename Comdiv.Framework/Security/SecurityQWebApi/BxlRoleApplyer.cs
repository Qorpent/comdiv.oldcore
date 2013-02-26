using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Extensions;
using Qorpent.Bxl;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	public class BxlRoleApplyer : RoleApplyer {
		protected override XElement CreateNewElement(string user, string role) {
			user = user.Replace("\\", "/").ToLower();
			role = role.ToUpper();
			return new XElement("map", new XAttribute("id",user),new XAttribute("code",user),new XAttribute("name",role));
		}
		protected override IEnumerable<UserRoleMap> internalListAllUserRoleMaps(XElement lastx)
		{
			var elements = lastx.XPathSelectElements("//map");
			foreach (var element in elements)
			{
				if (element.attr("code") == element.attr("code").ToLower()) {
					var rm = new UserRoleMap {User = element.attr("code").Replace("/","\\"), Role = element.attr("name").ToUpper()};
					yield return rm;
				}
			}
		}

		protected override XElement Load(string file) {
			
			return new BxlParser().Parse(File.ReadAllText(file));
		}

		protected override void Save(XElement x, string file) {
			var c = new BxlGenerator();
			var opts = new BxlGeneratorOptions {SkipAttributes = new[] {"_line", "_file"}, NoRootElement = true};
			File.WriteAllText(file, c.Convert(x));
		}

		protected override XElement Existed(XElement file, string user, string role) {
			user = user.Replace("\\", "/").ToLower();
			role = role.ToUpper();
			var x = file.XPathSelectElement("//map[@code='" + user + "' and @name='" + role + "']");
			return x;
		}
	}
}