using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Model.Mapping;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Comdiv.Web;
using FluentNHibernate;
using NHibernate.Criterion;

namespace Comdiv.Model.Scaffolding.Model{
    public class ClsListItem{
        public string Code { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class ClsFindHelper{
        private  StorageWrapper<object> storage;

        public ClsFindHelper(){
            this.storage = myapp.storage.GetDefault();
        }
        public static IList<ClsListItem> Get(string comment){
            using (new TemporarySimpleSession()){
                var res = myapp.storage.Get<ICls>().Query(Restrictions.Eq("Comment", comment), Order.Asc("Idx"));

                var result = new List<ClsListItem>();

                foreach (var a in res){
                    var cli = new ClsListItem();
                    cli.Code = a.Code;
                    cli.Name = a.Title;
                    cli.Type = Type.GetType(a.Name);
                    result.Add(cli);
                }
                return result;
            }
        }
    }

    public class ClsModel : PersistenceModel{
        private string schema = "cwcvm";

        public ClsModel(){
            Add(new ClsMap(Schema));
            Add(new ClsPropertyTypeMap(Schema));
            Add(new ClsPropertyMap(Schema));
        }

        public string Schema{
            get { return schema; }
            set { schema = value; }
        }
    }

    [MetadataProvider(typeof (ClsMetadataProvider))]
    public class Cls : ICls{
        public virtual Guid Uid { get; set; }
        public virtual string Tag { get; set; }
        #region ICls Members

        public virtual string Title { get; set; }


        public virtual string Group{
            get { return Comment; }
            set { Comment = value; }
        }

        public virtual int GroupOrder{
            get { return Idx; }
            set { Idx = value; }
        }

        public virtual IList<IClsProperty> Properties { get; set; }

        public virtual string Code { get; set; }

        public virtual string Comment { get; set; }

        public virtual int Id { get; set; }

        public virtual int Idx { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime Version { get; set; }

        #endregion
    }

    internal class ClsMetadataProvider : AutoMetadataProvider{
        protected override void internalGetProperties(Type type, IList<PropertyMetadata> properties, string alias){
            properties.Add(new PropertyMetadata("Id", typeof (int), true).SetTitle("Ид"));
            properties.Add(new PropertyMetadata("Code").SetTitle("Псевдоним"));
            properties.Add(new PropertyMetadata("Title").SetTitle("Заголовок"));
            properties.Add(new PropertyMetadata("Name").SetTitle("Класс"));
            properties.Add(new PropertyMetadata("Comment").SetTitle("Группа"));
        }

        protected override void internalGetCommands(Type type, string alias, IList<CommandDesc> commands){
            var c = new CommandDesc{
                                       Name = "Свойства",
                                       OnClick = "Globals.Behaviour.LoadContent(event);return false;",
                                       Command = "/database/object/list.rails?type=clsp&amp;hql=x.Cls.Id={0}",
                                   };
            commands.Add(c);
        }
    }

    public class ClsProperty : IClsProperty{
        public virtual Guid Uid { get; set; }
        public virtual string Tag { get; set; }
        #region IClsProperty Members

        public virtual ICls Cls { get; set; }

        public virtual IClsPropertyType ClsPropertyType { get; set; }

        public virtual string Code { get; set; }

        public virtual string Comment { get; set; }

        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime Version { get; set; }

        #endregion
    }

    public class ClsPropertyType : IClsPropertyType{
        public virtual Guid Uid { get; set; }
        public virtual string Tag { get; set; }
        #region IClsPropertyType Members

        public virtual IList<IClsProperty> Properties { get; set; }

        public virtual string Code { get; set; }

        public virtual string Comment { get; set; }

        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime Version { get; set; }

        #endregion
    }

    public class ClsMap : ExtendedClassMap<Cls>{
        public ClsMap() : this("cwcvm") {}

        public ClsMap(string schema) : base(schema){
            this.standard();
            Map(i => i.Title).Not.Nullable();
            HasMany<ClsProperty>(i => i.Properties).Standard("Cls");
        }
    }

    public class ClsPropertyTypeMap : ExtendedClassMap<ClsPropertyType>{
		public ClsPropertyTypeMap() : this("cwcvm") { }

        public ClsPropertyTypeMap(string schema)
            : base(schema){
            this.standard();
            HasMany<ClsProperty>(i => i.Properties).Standard("ClsPropertyType");
        }
    }

    public class ClsPropertyMap : ExtendedClassMap<ClsProperty>{
		public ClsPropertyMap() : this("cwcvm") { }

        public ClsPropertyMap(string schema)
            : base(schema){
            this.standard();
            References(x => x.Cls, "Cls").Standard<ICls, Cls>();
            References(x => x.ClsPropertyType, "ClsPropertyType").Standard<IClsPropertyType, ClsPropertyType>();
        }
    }
}