using System;

namespace Comdiv.Xml {
	/// <summary>
	/// describes XSLT extesnsion for XmlDsl infrastructure
	/// </summary>
	public class XmlDslExtensionDefinition {
		/// <summary>
		/// code for extension, will be replaced with ns prefix
		/// </summary>
		public string Code { get; set; }
		/// <summary>
		/// namespace for extension
		/// </summary>
		public string NameSpace { get; set; }
		/// <summary>
		/// name of extension type
		/// </summary>
		public string TypeName { get; set; }
		/// <summary>
		/// resolved extension type
		/// </summary>
		public Type Type { get; set; }
		/// <summary>
		/// created instance of extension
		/// </summary>
		public object Instance { get; set; }

		/// <summary>
		/// instantiates extension
		/// </summary>
		public void LoadExtension() {
			this.Type = Type.GetType(TypeName, true);
			this.Instance = Activator.CreateInstance(this.Type);
		}
	}
}