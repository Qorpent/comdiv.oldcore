using System;
using System.Collections.Generic;
using Qorpent.Serialization;


namespace Comdiv.Persistence {
	[Serialize(CamelNames = true)]
	public class AnnotationRecord : IParametersProvider
	{
		public AnnotationRecord()
		{
			Active = true;
			this.Type = "txt";
		}
		public int Id { get; set; }
		[Param]
		public string Code { get; set; }
		[Param]
		public string Name { get; set; }
		[Param]
		public string Tag { get; set; }
		[Param]
		public string TargetType { get; set; }
		[Param]
		public string TargetId { get; set; }
		[Param]
		public string Type { get; set; }
		[Param]
		public string Role { get; set; }
		[Param]
		public string Content { get; set; }

		[Param]
		public int Idx { get; set; }
		
		[Param]
		public bool Active { get; set; }

		

		public bool UseCustomMapping
		{
			get { return false; }
		}

		public IDictionary<string, object> GetParameters()
		{
			throw new NotImplementedException();
		}
	}
}