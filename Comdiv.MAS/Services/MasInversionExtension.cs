using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Persistence;
using Comdiv.QueryEngine;

namespace Comdiv.MAS
{
    public static class MasInversionExtension
    {
        public static void setupMas(MasConfiguration config = null) {
            setupMas(myapp.ioc, config);
        }

        public static IInversionContainer setupMas(this IInversionContainer container, MasConfiguration config = null) {
            config = config ?? new MasConfiguration();
            var processrepository = config.ProcessRepositoryType ?? typeof (DefaultMasProcessRepository);
            container.AddTransient("mas.model", typeof (MasModel));
            container.AddSingleton("mas.config",typeof(MasConfiguration), config);
            container.ensureService<IMasProcessRepository>( processrepository, "mas.repository.transient");
            container.ensureService<IMasProcessStarter,DefaultMasProcessStarter>("mas.process.starter.transient");
            container.AddTransient("mas.main-controller", typeof (MasController));
            var repositorysetupprovider =
                processrepository.getFirstAttribute<SetupProviderAttribute>().Type.create<IMasSetupProvider>();
            if(null!=repositorysetupprovider) {
                repositorysetupprovider.Execute(container,config);
            }
            return container;
        }
    }
}
