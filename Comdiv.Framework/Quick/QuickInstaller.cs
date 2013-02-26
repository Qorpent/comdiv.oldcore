using System.IO;
using System.Reflection;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Qorpent.IoC;
using Qorpent.Mvc;
using Qorpent.Security;

namespace Comdiv.Framework.Quick {
	public class QuickInstaller {
		public void InstallFrom(Assembly assembly,IInversionContainer c) {
			var types = assembly.getTypesWithAttribute<ActionAttribute>();
			foreach (var type in types) {
				var qatr = type.getFirstAttribute<ActionAttribute>();
				c.set(qatr.Name + ".qweb.action", type);
			}
			types = assembly.getTypesWithAttribute<RenderAttribute>();
			foreach (var type in types)
			{
				var qatr = type.getFirstAttribute<RenderAttribute>();
				c.set(qatr.Name + ".qweb.render", type);
			}
		}

		public static void PrepareQWeb(IInversionContainer container) {
			container.set("wiki.qweb.render", typeof (WikiRender));
			Qorpent.Applications.Application.Current.Container.RegisterSubResolver(new OldAssoiTypeLocator{Idx=-1}); //регистриуем контейнер АССОИ в качестве приоритетного
			Qorpent.Applications.Application.Current.Container.Register(
				Qorpent.Applications.Application.Current.Container.NewComponent<IViewEngine, BooViewEngineAdapterForQWeb>(Lifestyle.Singleton)
			);
			var f = Qorpent.Applications.Application.Current.MvcFactory;
			foreach (var file in Directory.GetFiles (Qorpent.Applications.Application.Current.BinDirectory,"*.dll")) {
				var fname = Path.GetFileNameWithoutExtension(file);
				if(fname.StartsWith("Comdiv.")||fname.StartsWith("Qorpent.")||fname.StartsWith("mia.")||fname.StartsWith("Zeta")) {
					var assembly = Assembly.Load(fname);
					f.Register(assembly);
				}
			}

		}
	}
}