using Comdiv.Extensions;
using Qorpent.Mvc;
using Qorpent.Mvc.Binding;

namespace Comdiv.Persistence {
	[Action("annotation.search", Role = "ADMIN")]
	public class AnnotationSearchQWeb : ActionBase
	{
		private AnnotationRepository repository;
		[Bind("Default")]private string system;
		public AnnotationSearchQWeb()
		{
			this.repository = new AnnotationRepository();
		}
		protected override object MainProcess()
		{
			var record = new AnnotationRecord();
			Context.Parameters.apply(record);
			return repository.Search(record, system);
		}
	}
}