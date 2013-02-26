#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Scaffolding;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Web;
using Comdiv.Xml;
using NHibernate.Criterion;
using Qorpent.Security;

#endregion

//using Ummc.Agro.Model;

namespace Comdiv.MVC.Scaffolding{
    public class MetadataHelper : IMetadataHelper{

      
        private static readonly XmlSerializer ser = XmlSerializer.FromTypes(new[]{typeof (ClassMetadata)})[0];

        private readonly IDictionary<string, ClassMetadata> Cache = new Dictionary<string, ClassMetadata>();

        public MetadataHelper(){
            myapp.OnReload += (s, a) => Reload();
            PrincipalSource = myapp.principals;
            PathResolver = myapp.files;
            this.storage = myapp.storage.GetDefault();
        }

        private static IInversionContainer _container;
        private StorageWrapper<object> storage;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(MetadataHelper)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public static IMetadataHelper Default{
            get { return Container.get<IMetadataHelper>(); }
        }

        public IPrincipalSource  PrincipalSource { get; set; }

        public IFilePathResolver PathResolver { get; set; }

        #region IMetadataHelper Members

        public ClassMetadata GetMeta(Type type){
            return GetMeta(type, "");
        }

        public ClassMetadata GetMeta(Type type, string viewAlias){
            lock (this){
                logger.get("comdiv.sys").debug(() => "helper called for " + type);
                var suffix = viewAlias.hasContent() ? "_" + viewAlias : "";
                var key = PrincipalSource.CurrentUser.Identity.Name + "/" + type.FullName + "/" + suffix;

                if (Cache.ContainsKey(key)){
                    return Cache[key];
                }
                var probes = new[]{
                                      "meta/" + type.FullName + suffix + ".xml", "meta/" + type.Name + suffix + ".xml",
                                      "meta/" + type.FullName + ".xml", "meta/" + type.Name + ".xml"
                                  };
                var resolved = "";
                foreach (var probe in probes){
                    resolved = PathResolver.Resolve(probe);
                    if (null == resolved){
                        continue;
                    }
                    //resolved = "~/" + PathResolver.ResolvePath(resolved).Path;
                    break;
                }
                ClassMetadata def = null;
                logger.get("comdiv.sys").debug(() => "helper file resolved " + (resolved ?? "NONE"));
                if (resolved.hasContent()){
                    using (var r = IncludeAwareXmlReader.Create(resolved.mapPath())){
                        def = (ClassMetadata) ser.Deserialize(r);
                        if (null == def.Properties || 0 == def.Properties.Length ||
                            def.NeedExpansionWithDefaultDescriptor){
                            var alt = BuildDefaultDescriptor(type, viewAlias);
                            def.Properties =
                                (def.Properties ?? new PropertyMetadata[]{}).Union(alt.Properties).ToArray();
                            def.Commands = (def.Commands ?? new CommandDesc[]{}).Union(alt.Commands).ToArray();
                        }
                    }
                }

                else{
                    def = BuildDefaultDescriptor(type, viewAlias);
                }

                MergeWithClsDefinition(def, type, viewAlias);
                Cache[key] = def;
                return def;
            }
        }

        #endregion

        public void Reload(){
            lock (this){
                Cache.Clear();
            }
        }

        private void MergeWithClsDefinition(ClassMetadata def, Type type, string alias){
            var cls = storage.First<ICls>(Restrictions.Eq("Comment", alias));
            if (null == cls){
                return;
            }
            if (def.Title.noContent()){
                def.Title = cls.Title;
            }
            foreach (var property in cls.Properties){
                switch (property.ClsPropertyType.Code){
                    case "allow.new":
                        def.AllowNew = bool.Parse(property.Name);
                        break;
                    case "allow.edit":
                        def.AllowEdit = bool.Parse(property.Name);
                        break;
                    case "allow.delete":
                        def.AllowDelete = bool.Parse(property.Name);
                        break;
                    case "visible":
                        def.Visible = bool.Parse(property.Name);
                        break;
                    case "sort":
                        def.Sort = def.Sort.hasContent() ? def.Sort : property.Name;
                        break;
                    case "group":
                        def.GroupBy = def.GroupBy.hasContent() ? def.GroupBy : property.Name;
                        break;
                    case "path":
                        def.Path = def.Path.hasContent() ? def.Path : property.Name;
                        break;
                    case "commands":
                        var ser = XmlSerializer.FromTypes(new[]{typeof (CommandDesc[])})[0];
                        var commands =
                            (CommandDesc[]) ser.Deserialize(XmlReader.Create(new StringReader(property.Name)));
                        def.Commands = (def.Commands ?? new CommandDesc[]{}).Union(commands).ToArray();
                        break;
                    case "properties":
                        var ser2 = XmlSerializer.FromTypes(new[]{typeof (PropertyMetadata[])})[0];
                        var props =
                            (PropertyMetadata[]) ser2.Deserialize(XmlReader.Create(new StringReader(property.Name)));
                        def.Properties = def.Properties.Union(props).ToArray();

                        break;


                    default:
                        if (!def.Parameters.ContainsKey(property.Code)){
                            def.Parameters[property.Code] = property.Name;
                        }
                        break;
                }
            }
        }

        private ClassMetadata BuildDefaultDescriptor(Type type, string alias){
            var attr = type.getFirstAttribute<MetadataProviderAttribute>();
            if (attr == null){
                var realtype = storage.Resolve(type, "");
                if (realtype != null){
                    attr = realtype.getFirstAttribute<MetadataProviderAttribute>();
                }
            }
            if (attr != null){
                return attr.GetMetadata(type, alias);
            }
            var providerFactory = Container.get<IMetadataProviderFactory>();
            var provider = providerFactory != null
                               ? providerFactory.CreateProvider(type, alias)
                               : new AutoMetadataProvider();
            logger.get("comdiv.sys").debug(() => "helper send metadatahelper to " + provider);
            return provider.GetMetadata(type, alias);
        }
    }
}