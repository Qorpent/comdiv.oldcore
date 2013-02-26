using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Extensions;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	public class ClassicXmlRoleApplyer : RoleApplyer {
		protected override IEnumerable<UserRoleMap> internalListAllUserRoleMaps(XElement lastx) {
			var elements = lastx.XPathSelectElements("//map[@user and @as]");
			foreach (var element in elements) {
				var rm = new UserRoleMap {User = element.attr("user"), Role = element.attr("as")};
				yield return rm;
			}
		}

		protected override XElement CreateNewElement(string user, string role) {
			user = user.Replace("/", "\\").ToLower();
			role = role.ToUpper();
			return new XElement("map", new XAttribute("user", user), new XAttribute("as", role));
		}

		protected override XElement Load(string file) {
			return XElement.Load(file);
		}

		protected override void Save(XElement x, string file) {
			x.Save(file);
		}

		protected override XElement Existed(XElement file, string user, string role) {
			user = user.Replace("/","\\").ToLower();
			role = role.ToUpper();
			var x = file.XPathSelectElement("//map[@user='" + user + "' and @as='" + role + "']");
			return x;
		}
	}
}