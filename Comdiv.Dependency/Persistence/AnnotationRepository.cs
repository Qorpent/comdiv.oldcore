using System.Runtime.Remoting.Contexts;
using Comdiv.Application;

namespace Comdiv.Persistence {
	public class AnnotationRepository
	{

		public string Save(AnnotationRecord record, string system = "Default")
		{
			using (var connection = HibernateInversionExtensions.getConnection(myapp.ioc, system))
			{
				return connection.ExecuteScalar<string>("exec comdiv.annotation_set", (IParametersProvider)record);
			}
		}
		public AnnotationRecord[] Search(AnnotationRecord query, string system = "Default")
		{
			AnnotationRecord[] result = null;
			using (var connection = HibernateInversionExtensions.getConnection(myapp.ioc, system))
			{
				result = connection.ExecuteOrm<AnnotationRecord>("exec comdiv.annotation_search", query);
			}
			return result;
		}
	}
}