using System;
using Comdiv;
using Comdiv.Extensions;
using Comdiv.Rules;


namespace Comdiv.Rules.Config
{
	public class ServiceDescriptor:IServiceDescriptor
	{
		private Type serviceType;
		public string Name { get; set; }

		public string RegisterTypeName{
			get{
				if (null == RegisterType) return null;
				return RegisterType.AssemblyQualifiedName;
			}
			set{
				if (string.IsNullOrEmpty(value)) RegisterType = null;
				RegisterType = Type.GetType(value, true);
			}
		}

		public string ServiceTypeName{
			get{
				if (null == ServiceType) return null;
				return ServiceType.AssemblyQualifiedName;
			}
			set{
				if (string.IsNullOrEmpty(value)) ServiceType = null;
				ServiceType = Type.GetType(value, true);
			}
		}


		public Type RegisterType { get; set; }

		public Type ServiceType{
			get { return serviceType; }
			set{
			//	value.contract_HasDefaultCtor();
				serviceType = value;
			}
		}


		public object Service { get; set; }

		public IServiceCreator Creator { get; set; }

		public void RegisterService(IServicesContainer container){
		//	@"container".contract_NotNull(container);

			object s = null;
			if (null != Service) s = Service;
			else if (null != Creator) s = Creator.CreateService(this);
			else if (null != ServiceType) s = ServiceType.create<object>();

		//	@"s".contract_NotNull(s, "Не удалось инстанцировать сервис");

			if (!string.IsNullOrEmpty(Name))
				container.RegisterService(Name, s);
			if (null != RegisterType){
			//	s.contract_Is(RegisterType);

				container.RegisterService(RegisterType, s);
			}
			if (string.IsNullOrEmpty(Name) && null == RegisterType)
				container.RegisterService(s);
		}
	}
}