// //   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// //   Supported by Media Technology LTD 
// //    
// //   Licensed under the Apache License, Version 2.0 (the "License");
// //   you may not use this file except in compliance with the License.
// //   You may obtain a copy of the License at
// //    
// //        http://www.apache.org/licenses/LICENSE-2.0
// //    
// //   Unless required by applicable law or agreed to in writing, software
// //   distributed under the License is distributed on an "AS IS" BASIS,
// //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //   See the License for the specific language governing permissions and
// //   limitations under the License.
// //   
// //   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.Persistence {
	public class HqlColumn {
		private IList<IEntityDataPattern> _dictionary;
		public HqlColumn() {
			this.Editable = true;
		}
		public bool Editable { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public int Idx { get; set; }
		public string Comment { get; set; }
		public PropertyInfo Property { get; set; }
		public string Views { get; set; }
		public string Types { get; set; }
		public bool Hidden { get; set; }
		public bool IsExtension { get; set; }
		public string CustomValueProvider { get; set; }
		public string View { get; set; }
		public bool UseCodes { get; set; }
		
		public string IsRef { get; set; }
		public string Tag { get; set; }

		public string LookupType { get; set; }

		public string CssClass { get; set; }
		public string CssStyle { get; set; }

		public string Dictionary { get; set; }
		public string FixedDictionary { get; set; }

		public string System { get; set; }


		public IList<IEntityDataPattern> GetDictionary() {
			return _dictionary ?? (_dictionary = internalGetDict());
		}

		private IList<IEntityDataPattern> internalGetDict() {
			if (FixedDictionary.hasContent()) {
				var result = new List<IEntityDataPattern>();
				var items = FixedDictionary.split(false, true, '|',';');
				foreach (var item in items) {
					var pair = item.split(false, true, ':', '=');
					var code = pair[0];
					var name = pair[0];
					if (pair.Count > 1) name = pair[1];
					result.Add(new Entity{Code=code,Name=name});
				}
				return result;
			}
			else {
				return myapp.storage.GetDefault().WithSystem(System).QueryEntities(Dictionary);
			}
		}

		public string GetCurrentDictList(string txt, string delimiter) {
			var dict = this.GetDictionary();
			var split = SlashListHelper.ReadList(txt);
			var result = new List<string>();
			foreach (var s in split) {
				var existed = dict.FirstOrDefault(x => x.Code == s);
				if (null == existed) {
					result.Add(s);
				}
				else {
					result.Add(string.Format("<span title='{1}'>{0}</span>", existed.Name, existed.Code));
				}
			}
			return result.concat(delimiter);
		}

		public object GetValue(object target) {
			if (CustomValueProvider.hasContent()) {
				var provider = myapp.ioc.get(CustomValueProvider).to<IHqlColumnValueProvider>();
				if (null == provider) {
					throw new Exception("Нет провайдера с именем " + CustomValueProvider);
				}
				return provider.GetValue(this, target);
			}
			return Property.GetValue(target, null);
		}

		public string GetParam(string name) {
			return TagHelper.Value(this.Tag, name);
		}
	}
}