using System;
using Comdiv.Rules;


namespace Comdiv.Rules.Config{
    public interface IServiceDescriptor {
        string Name { get; set; }
        Type RegisterType { get; set; }
        Type ServiceType { get; set; }
        object Service { get; set; }
        IServiceCreator Creator { get; set; }
        void RegisterService(IServicesContainer container);
    }
}