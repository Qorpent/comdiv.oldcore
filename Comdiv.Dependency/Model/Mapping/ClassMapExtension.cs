// // Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// // Supported by Media Technology LTD 
// //  
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //  
// //      http://www.apache.org/licenses/LICENSE-2.0
// //  
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// // 
// // MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.Model.Interfaces;
using FluentNHibernate.Mapping;


namespace Comdiv.Model.Mapping{



    public static class ClassMapExtension{
        private static IInversionContainer _container;
        public static object sync = new object();

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (sync){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public static void standard<T>(this ClassMap<T> map, params Type[] excludeModelTypes){
            var tablename = typeof (T).Name.ToLower();
            var tn = map.GetType().getFirstAttribute<TableNameAttribute>();
            if (null != tn){
                tablename = tn.Name;
            }
            foreach (var i in typeof(T).GetInterfaces()) {
                var atts = i.GetCustomAttributes(typeof (TableAttribute), false);
                if(atts.Length!=0) {
                    tablename = ((TableAttribute) atts[0]).Name;
                }

            }
            var rewrited =
                Container.all<IMapTableRewriter<T>>().Select(x => x.GetTableName()).FirstOrDefault(x => string.IsNullOrWhiteSpace(x));
            if (!string.IsNullOrWhiteSpace(rewrited)){
                tablename = rewrited;
            }
#if !LIB2
            map.WithTable(tablename);
#else
            map.Table(tablename);
#endif

            if (Is<T, IWithId>(excludeModelTypes)){
                var id = map.Id(i => ((IWithId) i).Id,"id");
                if (map is IDbSpecified && ((IDbSpecified) map).DbType == "postgres"){
                    id.GeneratedBy.Sequence(tablename + "_id_seq");
                }
            }
            if (Is<T, IWithCode>(excludeModelTypes)){
                map.Map(i => ((IWithCode) i).Code,"code")
                    .Not.Nullable().Unique();
            }
            
            if (Is<T, IWithName>(excludeModelTypes)){
#if !LIB2
                var map_ = map.Map(i => ((IWithName) i).Name,"name").WithLengthOf(20000); 
                if (!Is<T, IWithCode>()){
                    map_.Unique().Not.Nullable();
                }
#else
                var map_ = map.Map(i => ((IWithName)i).Name, "name").Length(20000);
                if (!Is<T, IWithCode>())
                {
                    map_.Unique().Not.Nullable();
                }
#endif
            }

            if (Is<T, IWithComment>(excludeModelTypes)){
                map.Map(i => ((IWithComment) i).Comment,"comment").Nullable();
            }

            if (Is<T, IWithVersion>(excludeModelTypes)){
#if !LIB2
                map.Version(i => ((IWithVersion) i).Version).ColumnName("version");
#else
                map.Version(i => ((IWithVersion)i).Version).Column("version");
#endif
            }

            if (map is ExtendedClassMap<T>) {
                var mymap = (ExtendedClassMap<T>) map;
                processMaps(mymap);
            }

            //TODO: обработка REF

        }


        
        private static void processMaps<T>(ExtendedClassMap<T> mymap) {
            if (Is<T, IWithRange>())
            {
                var _DateRange = mymap.Component(x =>(DateRange) ((IWithRange)x).Range, c => { });
                _DateRange.Map(c => c.Start);
                _DateRange.Map(c => c.Finish);
                //_DateRange. SetAttribute("class", typeof(DateRange).AssemblyQualifiedName);
                _DateRange.ReadOnly();
            }
            foreach (var property in typeof (T).GetProperties()){
                var mapproperty = property.GetCustomAttributes(typeof(MapAttribute), true).OfType<MapAttribute>().LastOrDefault();



                mapproperty = mapproperty ?? 
                    property.toBasePropertyInfo().GetCustomAttributes(typeof (MapAttribute), true).OfType<MapAttribute>().LastOrDefault();

                
#if !LIB2
                if (mymap.GetAllProperties().FirstOrDefault(x => x.Property.Name == property.Name) != null)
#else
                if (mymap.GetAllProperties().FirstOrDefault(x => x.Member.Name == property.Name) != null)
#endif
                    continue;//обходим ситуацию двойного маппинга после интерфейсов
                if(property.Name=="Version" || property.Name=="Id" || property.Name =="Code" || property.Name =="Name" || property.Name=="Uid") {
                    continue;//HACK: версия обрабатывается по другому
                }
                if(property.PropertyType == typeof(DateRange)) {
                    continue;
                    ;
                }
                if (null != mapproperty){
                    var column = mapproperty.ColumnName;
                    if (string.IsNullOrWhiteSpace(column)){
                        column = property.Name.ToLower();
                    }
                    var notnull = mapproperty.NotNull;
                    var pm = mymap.Map(property, column);
                    if (notnull){
                        pm.Not.Nullable();
                    }
                    if(null!=mapproperty.CustomType) {
#if LIB2
                        pm.CustomType(mapproperty.CustomType);
#else
                        pm.CustomTypeIs(mapproperty.CustomType);
#endif
                    }
                    if(mapproperty.Formula.hasContent()) {
#if !LIB2
                        pm.FormulaIs(mapproperty.Formula);
#else
                        pm.Formula(mapproperty.Formula);
#endif
                    }
					if(mapproperty.UseMaxLength) {
						pm.Length(2147483647);
					}
                }
            }
        }

        public static OneToManyPart<C> Standard<C>(this OneToManyPart<C> part, string columnName){
#if !LIB2
            part.KeyColumnNames.Add(columnName);
#else
            part.KeyColumn(columnName);
#endif

#if !LIB2
            part.LazyLoad().FetchType.Select();
#else
            part.LazyLoad().Fetch.Select();
#endif
            part.Cascade.None();
            return part;

        }

		public static ManyToOnePart<I> Standard<I, T>(this ManyToOnePart<I> part, string columnName) where T : I
        {
            return part.Standard<I, T>(columnName, true);
        }

		public static ManyToOnePart<I> Standard<I, T>(this ManyToOnePart<I> part)
        {
            return part.Standard<I, T>(null, true);
        }

		public static ManyToOnePart<I> Standard<I, T>(this ManyToOnePart<I> part, string columnName, bool notNullable)
        {
#if !LIB2
            part.WithForeignKey()
            .FetchType.Select()

#else
         part.ForeignKey()
             .Fetch.Select()
#endif
                
                .LazyLoad()
#if !LIB2
                .SetAttribute("class", typeof (T).AssemblyQualifiedName);
#else
.Class( typeof(T));
#endif
            if (notNullable){
                part.Not.Nullable();
            }
            if (!string.IsNullOrWhiteSpace(columnName)){
#if !LIB2
                part.ColumnName(columnName);
#else
                part.Column(columnName);
#endif
            }
            return part;
        }


        public static bool Is<T, I>(params Type[] excludeModelTypes){
            if (excludeModelTypes.Contains(typeof (I))){
                return false;
            }
            return typeof (I).IsAssignableFrom(typeof (T));
        }
    }

    public class TableNameAttribute : Attribute{
        public TableNameAttribute(string name){
            Name = name;
        }

        public string Name { get; protected set; }
    }
}