using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Xsl;
using Comdiv.Extensions;
using Qorpent.Utils.Extensions;

namespace Comdiv.Xml {
	/// <summary>
	/// definition for xml transform in XmlDsl infrastructure
	/// </summary>
	public class XmlDslDefinition {

		/// <summary>
		/// creates new instance of definition
		/// </summary>
		public XmlDslDefinition() {
			Parameters = new Dictionary<string, string>();
			Imports = new List<string>();
			Extensions = new List<XmlDslExtensionDefinition>();
		}
		/// <summary>
		/// name of XSLT (lang) name
		/// </summary>
		public string LangName { get; set; }
		/// <summary>
		/// list of custom imports to be added in XSLT
		/// </summary>
		public IList<string> Imports { get; private set; }
		/// <summary>
		/// list of custom transform parameters
		/// </summary>
		public IDictionary<string, string> Parameters { get; private set; }
		/// <summary>
		/// list of extensions objects
		/// </summary>
		public IList<XmlDslExtensionDefinition> Extensions { get; private set; }

		public bool NeedPrepareXslt {
			get { return Imports.Any() || Extensions.Any() || Parameters.Any(); }
		}

		private const string XSLTNS = "http://www.w3.org/1999/XSL/Transform";

		/// <summary>
		/// prepares xslt file to match dsl requirements
		/// </summary>
		/// <param name="xsltelement"></param>
		/// <returns></returns>
		public XElement PrepareXslt (XElement xsltelement) {
			if (!NeedPrepareXslt) return xsltelement;
			var root = xsltelement;
			//check extensions namespace definition
			foreach (var extensionDefinition in Extensions) {
				var existedns = root.GetNamespaceOfPrefix(extensionDefinition.Code);
				if(existedns==null) {
					root.Add(new XAttribute(XNamespace.Xmlns + extensionDefinition.Code, extensionDefinition.NameSpace));
				}
				else if(existedns.NamespaceName!=extensionDefinition.NameSpace) {
					throw new XmlDslLoaderException("transform already contains NS for prefix "+extensionDefinition.Code+" ("+existedns.NamespaceName+"), that not match needed "+extensionDefinition.NameSpace);
				}
			}

			//imports must be at top, so we have to know where import block ends
			var lastimport = root.Elements("{" + XSLTNS + "}import").LastOrDefault();
			var lastparam = root.Elements("{" + XSLTNS + "}param").LastOrDefault();


			//prepare parameters 
			foreach (var parameter in Parameters) {
				var existed = root.Elements("{" + XSLTNS + "}param").FirstOrDefault(x => x.attr("name") == parameter.Key);
				var paramelement = new XElement("{" + XSLTNS + "}param", new XAttribute("name", parameter.Key));
				if(null!=existed) {
					existed.ReplaceWith(paramelement);
					continue;
				}
				var val = parameter.Value;
				if(val.StartsWith("select:")) {
					paramelement.Add(new XAttribute("select",val.Substring(7)));
				}else {
					paramelement.Value = val;
				}
				if (null == lastparam) {
					if (null == lastimport) {
						root.AddFirst(paramelement);
					}
					else {
						lastimport.AddAfterSelf(paramelement);
					}
				}else {
					lastparam.AddAfterSelf(paramelement);
				}
				lastparam = paramelement;
				
			}

			// prepare imports (must be applyed last, after parameters)
			foreach (var import in Imports) {
				var importelement = new XElement("{" + XSLTNS + "}import", new XAttribute("href", import));
				if (null == lastimport) {
					root.AddFirst(importelement);
				}else {
					lastimport.AddAfterSelf(importelement);
				}
				lastimport = importelement;
			}

			return root;
		}


		/// <summary>
		/// creates argument list from DSL definition
		/// </summary>
		/// <returns></returns>
		public XsltArgumentList CreateArguments () {
			var result = new XsltArgumentList();
			foreach (var parameter in Parameters) {
				if(parameter.Value.StartsWith("select:"))continue; //it means that it's not parameter for args it's just like variable
				result.AddParam(parameter.Key,"",parameter.Value);
			}
			foreach (var extension in Extensions) {
				result.AddExtensionObject(extension.NameSpace,extension.Instance);
			}
			return result;
		}


		/// <summary>
		/// extracts XmlDsl definition from given source file
		/// </summary>
		/// <param name="sourceelement"></param>
		/// <returns></returns>
		/// <exception cref="XmlDslLoaderException"></exception>
		public XmlDslDefinition Extract(XElement sourceelement) {
			var lang = sourceelement.Element("transform");
			if(null==lang)throw new XmlDslLoaderException("no transfrom element in source");
			var imports = sourceelement.Elements("transform_import").ToArray();
			var parameters = sourceelement.Elements("transform_param").ToArray();
			var extensions = sourceelement.Elements("transform_extension").ToArray();
			LangName = lang.Describe().Id;
			if(LangName.IsEmpty()) throw new XmlDslLoaderException("lang name not defined "+sourceelement.Describe().ToWhereString());
			lang.Remove();
			foreach (var e in imports) {
				var import = e.Describe().Id;
				if (import.IsEmpty()) throw new XmlDslLoaderException("import name not defined " + e.Describe().ToWhereString());
				Imports.Add(import);
				e.Remove();
			}
			foreach (var e in parameters) {
				var code = e.Describe().Id;
				var val = e.Describe().Name;
				if (code.noContent()) throw new XmlDslLoaderException("parameter name not defined " + e.Describe().ToWhereString());
				Parameters[code] = val;
			}
			foreach (var e in extensions) {
				var code = e.Describe().Id;
				var ns = e.Describe().Name;
				var typename = e.Value;
				if (code.noContent()) throw new XmlDslLoaderException("prefix for extension not defined " + e.Describe().ToWhereString());
				if (ns.noContent()) throw new XmlDslLoaderException("namespace for extension not defined " + e.Describe().ToWhereString());
				if (typename.noContent()) throw new XmlDslLoaderException("typename for extension not defined " + e.Describe().ToWhereString());
				var extdef = new XmlDslExtensionDefinition {Code = code, NameSpace = ns, TypeName = typename};
				extdef.LoadExtension();
				Extensions.Add(extdef);
			}
			return this;
		}
	}
}