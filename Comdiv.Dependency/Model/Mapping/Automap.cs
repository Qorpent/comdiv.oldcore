using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Comdiv.Model.Interfaces;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using Comdiv.Extensions;
using FluentNHibernate.Mapping.Providers;

namespace Comdiv.Model.Mapping
{

    public static class AutomapExtension {
        public static Automap<T> getAutoMap<T>(this T target) {
            return new Automap<T>();
        }
    }

    public sealed class Automap<T>:ClassMap<T> 
    {
        public Automap() {
#if !LIB2
            SchemaIs(typeof (T).getFirstAttribute<SchemaAttribute>().Name("dbo"));
#else
            Schema(typeof(T).getFirstAttribute<SchemaAttribute>().Name("dbo"));
#endif
            processDefaultInterfaces();
            var properties = typeof (T).GetProperties();
            foreach (var property in properties) {
                var map = property.getFirstAttribute<MapAttribute>();
                if(null!=map) {
                    
#if !LIB2
                    if(null!=this.Properties.FirstOrDefault(x=>x.Property.Equals(property)))continue;
#else
                    if (null != this.Properties.FirstOrDefault(x => x.GetPropertyMapping().Member.MemberInfo.Equals(property))) continue;
#endif
					
                    if(property.PropertyType.IsValueType || property.PropertyType==typeof(string)) {
                        proceedWithSimpleProperty(property,map);
                    }else if(typeof(IEnumerable).IsAssignableFrom(property.PropertyType)) {
                        proceedWithMany(property, map);
                    }else {
                        proceedWithRef(property, map);
                    }
                }
            }

        }

        private void proceedWithRef(PropertyInfo property, MapAttribute map) {
            var columnname = map.ColumnName.hasContent() ? map.ColumnName : property.Name;
#if !LIB2
            var method = this.GetType().GetMethod("References",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.InvokeMethod, null, CallingConventions.Standard, new[]{typeof(PropertyInfo),typeof(string)}, null);
#else
            var method = this.GetType().GetMethod("References", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, CallingConventions.Standard, new[] { typeof(Member), typeof(string) }, null);
#endif
            method = method.MakeGenericMethod(property.PropertyType);
            
#if !LIB2

            var manytoone =(IManyToOnePart) method.Invoke(this, new object[] { property, columnname });
            manytoone.SetAttribute("fetch", "select");
            manytoone.SetAttribute("lazy","proxy");
            if(null!=map.CustomType) {
                manytoone.SetAttribute("class",map.CustomType.AssemblyQualifiedName);
            }
            if(map.NotNull) {
                manytoone.SetAttribute("not-null","true");
            }else {
                manytoone.SetAttribute("not-null", "false");
            }
#else
            var manytoone = (IExtendedManyToOnePart)method.Invoke(this, new object[] { new PropertyMember(property), columnname });

            manytoone.Setup(true,map.CustomType,map.NotNull);
#endif
        }

        private void proceedWithMany(PropertyInfo property, MapAttribute map) {
            var columnname = map.ColumnName.hasContent() ? map.ColumnName : property.Name;
#if !LIB2
            var method = this.GetType().GetMethod("HasMany", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, CallingConventions.Standard, new[] { typeof(PropertyInfo) }, null);
#else
            var method = this.GetType().GetMethod("HasMany", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, CallingConventions.Standard, new[] { typeof(Member) }, null);
#endif

            var type = property.PropertyType.GetGenericArguments()[0];
            method = method.MakeGenericMethod(type);
            
#if !LIB2
            var onetomany = (IOneToManyPart) method.Invoke(this, new object[] {property});
            onetomany.KeyColumnNames.Add(columnname);
            onetomany.Cascade.None();
            onetomany.LazyLoad();
            onetomany.SetAttribute("fetch", "select");
            if (map.Cascade) {
                onetomany.Cascade.Delete();
            }
#else
            var onetomany = (IExtendedOneToManyPart)method.Invoke(this, new object[] {new PropertyMember( property) });
            onetomany.Setup(columnname,map.Cascade);
#endif
        }

        private void proceedWithSimpleProperty(PropertyInfo property,MapAttribute map) {
			if(null!=property.GetCustomAttribute(typeof(NoMapAttribute),true)) return;
            var columnname = map.ColumnName.hasContent() ? map.ColumnName : property.Name;
#if !LIB2
            var mapped = Map(property, columnname);
            if(map.CustomType!=null) {
                mapped.CustomTypeIs(map.CustomType);
            }
            if(map.NotNull) {
                mapped.Not.Nullable();
            }
            if(map.Formula.hasContent()) {
                mapped.FormulaIs(map.Formula);
            }
            if(map.ReadOnly) {
                mapped.ReadOnly();
            }
#else
            var mapped = Map(new PropertyMember(property), columnname);
            if (map.CustomType != null)
            {
                mapped.CustomType(map.CustomType);
            }
            if (map.NotNull)
            {
                mapped.Not.Nullable();
            }
            if (map.Formula.hasContent())
            {
                mapped.Formula(map.Formula);
            }
            if (map.ReadOnly)
            {
                mapped.ReadOnly();
            }
			if(map.UseMaxLength) {
				mapped.Length(2147483647);
			}
			if(map.NoLazy) {
				mapped.Not.LazyLoad();
			}
#endif
        }

        private void processDefaultInterfaces() {
            if(typeof(IWithId).IsAssignableFrom(typeof(T))) {
                Id(x => ((IWithId) x).Id).GeneratedBy.Native();
            }
            if (typeof(IWithVersion).IsAssignableFrom(typeof(T))) {
                Version(x => ((IWithVersion) x).Version);
            }
            if (typeof(IWithCode).IsAssignableFrom(typeof(T)))
            {
                Map(x => ((IWithCode)x).Code).Unique();
            }
            if (typeof(IWithName).IsAssignableFrom(typeof(T)))
            {
                Map(x => ((IWithName)x).Name);
            }
        }
    }
}
