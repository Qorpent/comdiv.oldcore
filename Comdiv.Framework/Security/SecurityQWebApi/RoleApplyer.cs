using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	public abstract class  RoleApplyer:IRoleApplyer {
		public static IRoleApplyer CreateByFileName(string filename) {
			if (Path.GetExtension(filename) == ".config") return new ClassicXmlRoleApplyer();
			if (Path.GetExtension(filename) == ".bxl") return new BxlRoleApplyer();
			throw new RolesActionException("cannot define role applyer for given file "+filename);
		}

		public  IEnumerable<UserRoleMap> GetAllUserRoleMaps(string file) {
			checkFile(file);
			return internalListAllUserRoleMaps(_lastx);
		}

		protected abstract IEnumerable<UserRoleMap> internalListAllUserRoleMaps(XElement lastx);

		public void Add(string file, string user, string role) {
			if (_lastfile != file || null == _lastx)
			{
				_lastx = File.Exists(file) ? Load(file) : new XElement("config");
			}
			var existed = Existed(_lastx, user, role);
			if(null==existed) {
				_lastx.Add(CreateNewElement(user,role));
				Save(_lastx,file);
			}
		}

		

		protected abstract XElement CreateNewElement(string user, string role);
		private string _lastfile = "";
		private XElement _lastx ;
		public void Remove(string file, string user, string role) {
			if (_lastfile != file || null == _lastx) {
				_lastx = File.Exists(file) ? Load(file) : new XElement("config");
			}
			var existed = Existed(_lastx, user, role);
			if (null!=existed) {
				existed.Remove();
				Save(_lastx,file);
			}
		}

		public bool Exists(string file, string user, string role) {
			checkFile(file);
			var existed = Existed(_lastx, user, role);
			return null != existed;
		}

		protected void checkFile(string file) {
			if (_lastfile != file || null == _lastx) {
				_lastfile = file;
				_lastx = File.Exists(file) ? Load(file) : new XElement("config");
			}
		}

		protected abstract XElement Load(string file);
		protected abstract void Save(XElement x, string file);
		protected abstract XElement Existed(XElement file, string user, string role);
	}
}