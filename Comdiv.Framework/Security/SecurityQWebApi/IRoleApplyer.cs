using System.Collections.Generic;

namespace Comdiv.Framework.Security.SecurityQWebApi {
	public interface IRoleApplyer {
		void Add(string file, string user, string role);
		void Remove(string file, string user, string role);
		bool Exists(string file, string user, string role);
		IEnumerable<UserRoleMap> GetAllUserRoleMaps(string  file);
	}
}