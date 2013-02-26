using Comdiv.Extensions;
using Qorpent.Mvc;
using Qorpent.Mvc.Binding;

namespace Comdiv.Persistence {
	[Action("annotation.save",Role="ADMIN")]
	public class AnnotationSaveQWeb:ActionBase {
		private AnnotationRepository repository;
		[Bind("Default")] private string system;
		public AnnotationSaveQWeb() {
			this.repository = new AnnotationRepository();
		}
		protected override object MainProcess()
		{
			var record = new AnnotationRecord();
			Context.Parameters.apply(record);
			return repository.Save(record,system);
		}
	}
}