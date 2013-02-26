using System;
using System.Collections.Generic;
using Comdiv.Rules;

namespace Comdiv.Rules.Support
{
	public class ServicesContainer : IServicesContainer
	{
		private readonly IDictionary<string, object> byName = new Dictionary<string, object>();
		private readonly IDictionary<Type, object> byType = new Dictionary<Type, object>();
		private IServicesContainer parentContainer;

		#region IServicesContainer Members

		public virtual IServicesContainer ParentContainer{
			get { return parentContainer ?? (parentContainer = findParentContainer()); }
			set { parentContainer = value; }
		}

		public void RegisterService(string name, object service){
			//CheckNameAndServiceContract(name, service);

			byName[name] = service;
		}

		public void RegisterService<S>(string name, S service)
			where S : class{
			//CheckNameAndServiceContract(name, service);
			RegisterService(service);
			RegisterService(name, (object) service);
		}

		public void RegisterService(object servive){
			byType[servive.GetType()] = servive;
		}

		public void RegisterService(Type type, object service){
			//@"type".contract_NotNull(type);
			//@"service".contract_NotNull(service);

			byType[type] = service;
		}

		public void RegisterService<S>(S service)
			where S : class{
			//@"service".contract_NotNull(service);

			byType[typeof (S)] = service;
		}

		public void UnRegisterService(object service){
			//@"service".contract_NotNull(service);
			var namekeys = new List<string>();
			foreach (var pair in byName){
				if (pair.Value == service)
					namekeys.Add(pair.Key);
			}

			var typekeys = new List<Type>();
			foreach (var pair in byType){
				if (pair.Value == service)
					typekeys.Add(pair.Key);
			}
			foreach (var type in typekeys)
				byType.Remove(type);
			foreach (var s in namekeys)
				byName.Remove(s);
		}

		public Z Get<Z>(){
			return (Z) this[typeof (Z)];
		}

		public virtual object this[string name]{
			get{
				//@"name".contract_HasContent(name);

				return byName.ContainsKey(name)
				       	? byName[name]
				       	:
				       		null == ParentContainer
				       			? null
				       			:
				       				parentContainer[name];
			}
		}

		public virtual object this[Type type]{
			get{
				//@"type".contract_NotNull(type);
				return byType.ContainsKey(type)
				       	? byType[type]
				       	:
				       		null == ParentContainer
				       			? null
				       			:
				       				parentContainer[type];
			}
		}

		#endregion

		protected virtual IServicesContainer findParentContainer(){
			return null;
		}

		//private void CheckNameAndServiceContract(string name, object service){
			//@"name".contract_HasContent(name);
			//@"service".contract_NotNull(service);
		//}
	}
}