#region LICENSE

// Copyright 2007-2012 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// Solution: Qorpent
// Original file : ReportDefinitionBase.cs
// Project: Comdiv.Reporting
// 
// ALL MODIFICATIONS MADE TO FILE MUST BE DOCUMENTED IN SVN

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Extensibility;
using Comdiv.Extensions;
using Comdiv.MVC;
using Comdiv.Model.Interfaces;

namespace Comdiv.Reporting {
	public class
		ReportDefinitionBase : IReportDefinitionDynamicView {
		public ReportDefinitionBase() {
			_widgets.Definition = this;
			templateParameters = new ParametersCollection();
			templateParameters.Report = this;
		}

		public IList<string> Conditions {
			get {
				return cache.get("conditions", () =>
					{
						if (!Parameters.ContainsKey("condition")) {
							return new string[] {};
						}
						return Parameters["condition"].ToString().split();
					});
			}
		}

		public IList<string> Hides {
			get { return hides; }
		}

		public IList<IReportDefinition> Sources {
			get { return sources; }
		}

		public object Thema { get; set; }
		public string Role { get; set; }


		public bool PreviewMode { get; set; }

		public object ControllerInstance { get; set; }


		public void CheckReportLive() {
			if (Task.HaveToTerminate || (null != Task.Context && !Task.Context.Response.IsClientConnected)) {
				Task.Terminate();
				throw new Exception("Выполнение отчета было отменено");
			}
		}


		public string PrepareViewGenerator { get; set; }
		public string RenderViewGenerator { get; set; }

		public string PrepareView {
			get {
				var p = TemplateParameters.FirstOrDefault(x => x.Code == "generatorname");
				if (null == p) {
					return "";
				}
				return p.Value.toStr();
			}
			set {
				var p = TemplateParameters.FirstOrDefault(x => x.Code == "generatorname");
				if (null == p) {
					p = new Parameter {Code = "generatorname", Target = "generatorname", Static = true};
					TemplateParameters.Add(p);
				}
				p.setdef(value);
			}
		}


		public ReportWidgetCollection Widgets {
			get { return _widgets; }
		}

		public string RenderView {
			get {
				var p = TemplateParameters.FirstOrDefault(x => x.Code == "viewname");
				if (null == p) {
					return "";
				}
				return p.Value.toStr();
			}
			set {
				var p = TemplateParameters.FirstOrDefault(x => x.Code == "viewname");
				if (null == p) {
					p = new Parameter {Code = "viewname", Target = "viewname", Static = true};
					TemplateParameters.Add(p);
				}
				p.setdef(value);
			}
		}

		public LongTask Task { get; set; }

		public IReportDefinition LoadParameters() {
			var c = new NameValueCollection();
			if (null != HttpContext.Current) {
				c = HttpContext.Current.Request.Params;
				if (HttpContext.Current.Request.Params.AllKeys.Contains("srcode")) {
					TemplateParameters.SavedReport =
						myapp.storage.Get<ISavedReport>().Load(HttpContext.Current.Request.Params["srcode"]);
					if (!TemplateParameters.SavedReport.Authorize(myapp.usr)) {
						throw new SecurityException("попытка вывода недоступного хранимого отчета с кодом " +
						                            TemplateParameters.SavedReport.Code);
					}
				}
				else {
					TemplateParameters.SavedReport =
						myapp.storage.Get<ISavedReport>().Load(Code + "_default");
				}
			}
			return LoadParameters(c);
		}

		public IReportDefinition LoadParameters(NameValueCollection collection) {
			collection = collection ?? new NameValueCollection();
			parameters = Parameters ?? new Dictionary<string, object>();
			parameters.Clear();
			var ps = TemplateParameters.Eval(collection);
			foreach (var o in ps) {
				parameters[o.Key] = o.Value;
			}
			return this;
		}

		public string Controller { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public string Area { get; set; }
		public string AdvancedParameters { get; set; }

		public virtual IReportDefinition Clone() {
			var result = (ReportDefinitionBase) GetType().create<IReportDefinition>();
			result.Task = Task;
			result.bindfrom(this, true);
			result.Hides.Clear();
			result.Thema = Thema;
			foreach (var hide in Hides) {
				result.Hides.Add(hide);
			}
			foreach (var parameter in Parameters.ToArray()) {
				if (parameter.Key == null) {
					continue;
				}
				result.Parameters[parameter.Key] = parameter.Value;
			}
			foreach (var parameter in TemplateParameters) {
				var existed = result.TemplateParameters.FirstOrDefault(x => x.Code == parameter.Code);
				if (existed != null) {
					result.TemplateParameters.Remove(existed);
				}
				result.TemplateParameters.Add(parameter.Clone());
			}
			foreach (var widget in Widgets) {
				result.Widgets.Add(widget.Key, widget.Value);
			}
			foreach (var extension in Extensions) {
				result.Extensions[extension.Key] = extension.Value;
			}
			foreach (var s in Sources) {
				result.Sources.Add(s);
			}
			return result;
		}

		public void CleanupParameters(IPrincipal usr) {
			foreach (var param in TemplateParameters.ToArray()) {
				if (!param.Authorize(myapp.usr)) {
					TemplateParameters.Remove(param);
				}
			}
		}


		public ParametersCollection TemplateParameters {
			get { return templateParameters; }
		}

		public Dictionary<string, object> Parameters {
			get { return parameters ?? (parameters = new Dictionary<string, object>()); }
		}

		public string Comment { get; set; }

		public virtual string PageTitle {
			get { return Name; }
		}

		public void ReadFromXml(XElement xml) {
			loadIdentity(xml);


			loadAdvancedParameters(xml);
			var templates =
				xml.read<Parameter>("./template/param")
					.Union(xml.read<Parameter>("./var"))
					.ToList()
				;

//                int idx = 10;
			foreach (var t in templates) {
				//                  t.Idx = idx;
				//                idx += 10;
				t.settemplate(true);
				TemplateParameters.Add(t);
			}

			loadBaseParameters(xml);
			//удаляет из набора параметров ненужные для данного отчета или прячет
			removeParameters(xml);

			Widgets.ReadFromXml(xml);

			foreach (var element in xml.Elements("extension")) {
				var code = element.attr("code");
				extensions[code] = element.Value;
			}

			internalReadXml(xml);
		}

		public IDictionary<string, string> Extensions {
			get { return extensions; }
		}

		public void WriteToXml(XmlWriter writer) {
			writer.WriteStartElement("report");
			writer.WriteAttributeString("controller", "", Controller);
			writer.WriteAttributeString("name", "", Name);
			writer.WriteAttributeString("area", "", Area);
			writer.WriteAttributeString("code", "", Code);
			writer.WriteAttributeString("type", "", GetType().AssemblyQualifiedName);
			writer.WriteElementString("advancedParameters", "", AdvancedParameters);
			writer.WriteElementString("comment", "", Comment);
			foreach (var pair in parameters) {
				writer.WriteStartElement("param");
				writer.WriteAttributeString("name", "", pair.Key);
				if (null != pair.Value) {
					if (pair.Value is int) {
						writer.WriteAttributeString("type", "", "int");
					}
					else if (pair.Value is bool) {
						writer.WriteAttributeString("type", "", "bool");
					}
					writer.WriteString(pair.Value.ToString());
				}
				writer.WriteEndElement();
			}
			writer.WriteStartElement("template");
			writer.write("param", TemplateParameters);
			writer.WriteEndElement();
			internalWriteXml(writer);
			writer.WriteEndElement();
		}

		public void SetCache(string name, object value) {
			cache[name] = value;
		}

		public IList<ReportWidget> widgets(string zone) {
			return Widgets.ForZone(zone);
		}

		public object get(string name, object def) {
			var result = Parameters.get(name, null);
			if(null==result||""==result) {
				if (Thema != null) return ((IPseudoThema)Thema).GetParameter(name) ?? def;	
			}
			return Parameters.get(name, def);
		}

		private void removeParameters(XElement element) {
			var hides = element.Elements("hide");
			foreach (var hide in hides) {
				var code = hide.attr("code");
				Hides.Add(code);
				var p = TemplateParameters.FirstOrDefault(x => x.Code == code);
				if (p != null) {
					p.Static = true;
				}
			}
		}


		protected virtual void loadIdentity(XElement xml) {
			Area = xml.chooseAttr("area", "zone");
			Controller = xml.chooseAttr("controller", "class");
			Code = xml.attr("code");
			Name = xml.attr("name");
			Comment = xml.getText("comment");
		}

		private void loadBaseParameters(XElement xml) {
			var props = xml.chooseFirstNotEmpty("./param", "./prop");
			foreach (var prop in props) {
				var p = new Parameter();
				p.settemplate(false);
				p.Code = prop.attr("code");
				if (p.Code.noContent()) {
					p.Code = prop.attr("name");
				}
				p.Target = prop.attr("target");
				if (p.Target.noContent()) {
					p.Target = p.Code;
				}
				p.settype(prop.attr("type"));
				p.setdef(prop.Value);
				if (prop.Value.noContent()) {
					var val = prop.chooseAttr("value", "defaultvalue");
					if (val.noContent()) {
						val = prop.Value;
					}
					p.setdef(val);
				}
				TemplateParameters.Add(p);
			}
		}

		private void loadAdvancedParameters(XElement xml) {
			AdvancedParameters = xml.getText("advancedParameters");
			if (AdvancedParameters.noContent()) {
				AdvancedParameters = xml.getText("advancedParamsStub");
			}

			if (AdvancedParameters.hasContent()) {
				foreach (var param in AdvancedParameters.split()) {
					var p = param.Split(':');
					Parameters[p[0].Trim()] = p[1].Trim();
				}
			}
		}

		protected virtual void internalReadXml(XElement current) {}

		protected virtual void internalWriteXml(XmlWriter writer) {}

		public bool IsConditionMatch(string condition) {
			if (condition.noContent()) {
				return true;
			}
			if (condition.Contains(",") || condition.Contains("|")) {
				var condsets = condition.split(false, true, '|');
				foreach (var condset in condsets) {
					var match = Conditions.containsAll(condset.split().ToArray());
					if (match) {
						return true;
					}
				}
				return false;
			}
			var e = PythonPool.Get();
			var cond = condition;
			try {
				cond = condition.replace(@"[\w\d_]+", m =>
					{
						if (m.Value.isIn("or", "and", "not")) {
							return m.Value;
						}
						var c = m.Value;
						if (Conditions.Contains(c)) {
							return " True ";
						}
						return " False ";
					}).Trim();
				var result = e.Execute<bool>(cond);
				return result;
			}
			catch (Exception ex) {
				throw new Exception("Ошибка в " + cond);
			}
			finally {
				PythonPool.Release(e);
			}
		}

		private readonly ReportWidgetCollection _widgets = new ReportWidgetCollection();
		private readonly IDictionary<string, string> extensions = new Dictionary<string, string>();
		private readonly IList<string> hides = new List<string>();
		private readonly IList<IReportDefinition> sources = new List<IReportDefinition>();
		private readonly ParametersCollection templateParameters;
		protected IDictionary<string, object> cache = new Dictionary<string, object>();
		protected Dictionary<string, object> parameters;
		}
}