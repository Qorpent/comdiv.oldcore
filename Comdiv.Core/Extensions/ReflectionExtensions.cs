// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Comdiv.Logging;

namespace Comdiv.Extensions {
    //TODO: разобрать что лишнее
    ///<summary>
    ///</summary>
    public static class ReflectionExtensions {
        /// <summary>
        /// Позволяет обойти проблемы загрузки сборок не из дефолтной bin - директорией
        /// </summary>
        /// <param name="filename">путь к файлу</param>
        /// <returns></returns>
        public static Assembly LoadAssemblyFromFile(string filename, string root = null, string[] exclude = null) {
            string dir = root ?? Path.GetDirectoryName(filename);
            exclude = exclude ?? new string[] {};

            ResolveEventHandler loader = (s, a) =>
                                             {
                                                 string file = Directory
                                                     .EnumerateFiles(dir, a.Name + ".dll", SearchOption.AllDirectories)
                                                     .Select(x => x.ToLower().Replace("\\", "/"))
                                                     .Where(x => null == exclude.FirstOrDefault(e => x.like(e)))
                                                     .FirstOrDefault();
                                                 if (file.hasContent()) {
                                                     return Assembly.LoadFrom(file);
                                                 }
                                                 return null;
                                             };
            try {
                AppDomain.CurrentDomain.AssemblyResolve += loader;
                return Assembly.LoadFrom(filename);
            }
            finally {
                AppDomain.CurrentDomain.AssemblyResolve -= loader;
            }
        }


        public static PropertyInfo toBasePropertyInfo(this PropertyInfo p) {
            PropertyInfo result = p;
            var interfaces = new List<Type>();
            interfaces.AddRange(p.DeclaringType.GetInterfaces());
            foreach (Type i in interfaces) {
                PropertyInfo p_ = null;
                if ((p_ = i.GetProperty(p.Name)) != null) {
                    result = p_;
                    break;
                }
            }
            return result;
        }


        public static T bindfrom<T>(this T target, object from) {
            return bindfrom(target, from, false);
        }

        public static T bindfrom<T>(this T target, object from, bool primitivesOnly, params string[] excludes) {
            excludes = excludes ?? new string[] {};
            PropertyInfo[] props = from.GetType().GetProperties();
            foreach (PropertyInfo src in props) {
                if (excludes.yes() && src.Name.isIn(excludes)) continue;

                if (!primitivesOnly || src.PropertyType.Equals(typeof (string)) || src.PropertyType.IsValueType) {
                    object val = src.GetValue(from, null);
                    if (null != val) {
                        target.setPropertySafe(src.Name, val);
                    }
                }
            }
            return target;
        }

        public static Type ResolveTypeByWellKnownName(string name) {
            switch (name) {
                case "int":
                    return typeof (int);
                case "str":
                    return typeof (string);
                case "date":
                    return typeof (DateTime);
                case "bool":
                    return typeof (bool);
                case "decimal":
                    return typeof (decimal);
            }
            return null;
        }

        public static string ResolveWellKnownName(Type type) {
            switch (type.Name) {
                case "Int32":
                    return "int";
                case "String":
                    return "str";
                case "DateTime":
                    return "date";
                case "Boolean":
                    return "bool";
                case "Decimal":
                    return "decimal";
            }
            return null;
        }


        public static bool hasAttribute(this MemberInfo info, Type attributeType) {
            if (info == null) throw new ArgumentNullException("info");
            if (attributeType == null) throw new ArgumentNullException("attributeType");
            return info.GetCustomAttributes(attributeType, true).yes();
        }

        public static PropertyInfo resolveProperty(this Type type, string name) {
            PropertyInfo result = type.GetProperty(name,
                                                   BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

            if (null == result && type.IsInterface) {
                foreach (Type intType in type.GetInterfaces()) {
                    result = intType.resolveProperty(name);
                    if (null != result) break;
                }
            }

            return result;
        }

        public static Type toType(this string str)
        {
            return toType(str, null);
        }

        public static Type toType(this string str, IDictionary<string ,Type> map = null) {
            if (string.IsNullOrWhiteSpace(str)) return null;
            
            if (str == "string") return typeof (string);
            if (str == "int") return typeof(int);
            if (str == "decimal") return typeof(decimal);
            if (str == "date") return typeof(DateTime);
            if (str == "bool") return typeof(bool);
            
            if (str.EndsWith(".dll")) str = str.Substring(0, str.Length - 4);
            if(null!=map && map.ContainsKey(str))
            {
                return map[str];
            }
            Type result = Type.GetType(str);
            if (null == result) throw new NullReferenceException(str + " maps to non-correct or unavailable type");
            return result;
        }

        public static T getFirstAttribute<T>(this Type type)
            where T : Attribute {
            if (null == type) {
                return null;
            }
            return (T) type.GetCustomAttributes(typeof (T), true).FirstOrDefault();
        }

        public static T getFirstAttribute<T>(this MemberInfo member)
            where T : Attribute {
            if (null == member) {
                return null;
            }
            return (T) member.GetCustomAttributes(typeof (T), true).FirstOrDefault();
        }

        public static object create(this Type type) {
            return type.create<object>();
        }

        public static T create<T>(this Type type) {
            //var x = new Int32();
            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            if (null == ctor) {
                return (T) String.Empty.to(type);
            }
            return ctor.Invoke(null).to<T>();
        }

        public static T create<T>(this Type t, params object[] parameters) {
            Type[] types = parameters.Select(o => null == o
                                                      ? typeof (object)
                                                      : o.GetType()
                ).ToArray();

            return (T) t.GetConstructor(types).Invoke(parameters);
        }

        public static T create<T>(this Type t, Type[] types, params object[] parameters) {
            return (T) t.GetConstructor(types).Invoke(parameters);
        }

        public static bool hasAttribute<T>(this MemberInfo info) where T : Attribute {
            return info.hasAttribute<T>(t => true);
        }

        public static bool hasAttribute<T>(this MemberInfo info, Func<T, bool> predicate) where T : Attribute {
            predicate = predicate ?? (a => true);
            return info.hasAttribute(typeof (T), t => predicate((T) t));
        }

        public static bool hasAttribute(this MemberInfo info, Type attributeType, Func<Attribute, bool> predicate) {
            if (info == null) return false;
            if (attributeType == null) return false;
            predicate = predicate ?? (a => true);
            return info.GetCustomAttributes(attributeType, true).Cast<Attribute>().Where(t => predicate(t)).yes();
        }


        public static bool hasProperty(this Type type, string name) {
            return null != type.resolveProperty(name);
        }


        public static T getPropertySafe<T>(this object target, string name) {
            return target.getPropertySafe(name, default(T));
        }

        public static T getPropertySafe<T>(this object target, string name, T def) {
            if (null == target) return def;
            PropertyInfo prop = target.GetType().resolveProperty(name);
            if (null == prop) return def;
            return prop.GetValue(target, null).to(true, def);
        }

        public static T setPropertySafe<T>(this T target, string name, object value) {
            if (null == target) return target;

            PropertyInfo pi = target.GetType().resolveProperty(name);
            if (null == pi) return target;
            try {
                Type ttype = pi.PropertyType;
                if (null != value) {
                    value = value.to(ttype);
                }
                if (pi.GetSetMethod() != null) {
                    pi.SetValue(target, value, null);
                }
            }
            catch (Exception ex) {
            }
            return target;
        }

        public static T getProperty<T>(this object target, string name) {
            return getProperty(target, name).to<T>();
        }

        public static object getProperty(this object target, string name) {
            if (null == target)
                throw new InvalidOperationException(String.Format("cannot retrieve property {0} of NULL", name));
            PropertyInfo prop = target.GetType().GetProperty(name);
            if (null == prop)
                throw new InvalidOperationException(String.Format("No property {0} on class {1}", name, target.GetType()));
            return target.GetType().GetProperty(name).GetValue(target, null);
        }

        public static T setProperty<T>(this T target, string name, object value) {
            if (null == target) throw new InvalidOperationException();
            PropertyInfo pi = target.GetType().GetProperty(name);
            if (null == pi)
                throw new InvalidOperationException(String.Format("No property {0} on class {1}", name, target.GetType()));
            object newValue = value;
            Type propertyType = pi.PropertyType;
            if (null == newValue || !propertyType.IsAssignableFrom(newValue.GetType())) {
                newValue = newValue.to(propertyType);
            }

            target.GetType().GetProperty(name).SetValue(target, newValue, null);
            return target;
        }


        public static IEnumerable<Type> getTypesWithAttribute<TAttribute>(this Assembly assembly)
            where TAttribute : Attribute {
            return getTypesWithAttribute<TAttribute>(assembly, null);
        }

        public static IEnumerable<Type> getTypesWithAttribute<TAttribute>(this Assembly assembly,
                                                                          Func<TAttribute, Type, bool> predicate)
            where TAttribute : Attribute {
            predicate = predicate ?? ((a, t) => true);
            return new[] {assembly}.getTypesWithAttribute(typeof (TAttribute), (a, t) => predicate((TAttribute) a, t));
        }

        public static IEnumerable<Type> getTypesWithAttribute<TAttribute>(this IEnumerable<Assembly> assemblies)
            where TAttribute : Attribute {
            return getTypesWithAttribute<TAttribute>(assemblies, null);
        }

        public static IEnumerable<Type> getTypesWithAttribute<TAttribute>(this IEnumerable<Assembly> assemblies,
                                                                          Func<TAttribute, Type, bool> predicate)
            where TAttribute : Attribute {
            predicate = predicate ?? ((a, t) => true);
            return assemblies.getTypesWithAttribute(typeof (TAttribute), (a, t) => predicate((TAttribute) a, t));
        }

        public static IEnumerable<Type> getTypesWithAttribute(this IEnumerable<Assembly> assemblies, Type attributeType,
                                                              Func<Attribute, Type, bool> predicate) {
            IEnumerable<Assembly> assemblyset = assemblies.yes() ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
            predicate = predicate ?? ((a, t) => true);

            foreach (Assembly assembly in assemblyset) {
                Type[] types = null;
                try {
                    types = assembly.GetTypes();
                }
                catch (Exception ex) {
                    logger.get("comdiv.core.reflection").Error("dll cannot be tested for attributes", ex);
                    continue;
                }
                if (types.yes()) {
                    foreach (Type type in types) {
                        object[] attributes = type.GetCustomAttributes(attributeType, true);
                        if (attributes.no()) continue;
                        bool filtered = false;
                        foreach (Attribute attr in attributes) {
                            if (predicate(attr, type)) {
                                filtered = true;
                                break;
                            }
                        }
                        if (filtered)
                            yield return type;
                    }
                }
            }
        }

        public static IEnumerable<Type> getTypesWithAttribute(this Assembly assembly, Type attributeType) {
            foreach (Type type in assembly.GetTypes())
                if (type.GetCustomAttributes(attributeType, true).yes()) yield return type;
        }


        public static IEnumerable<MemberInfo> getMembersWithAttribute<T>(this T attribute, Type targetType,
                                                                         Func<T, MemberInfo, bool> filter)
            where T : Attribute {
            return getMembersWithAttribute(attribute, targetType, BindingFlags.Default, filter);
        }

        public static IEnumerable<MemberInfo> getMembersWithAttribute<T>(this T attribute, Type targetType,
                                                                         BindingFlags binding)
            where T : Attribute {
            return getMembersWithAttribute(attribute, targetType, binding, null);
        }


        public static IEnumerable<MemberInfo> getMembersWithAttribute<T>(this T attribute, Type targetType)
            where T : Attribute {
            return getMembersWithAttribute(attribute, targetType, BindingFlags.Default, null);
        }

        public static IEnumerable<MemberInfo> getMembersWithAttribute<T>(this T attribute, Type targetType,
                                                                         BindingFlags binding,
                                                                         Func<T, MemberInfo, bool> filter)
            where T : Attribute {
            if (null == targetType) throw new ArgumentNullException("targetType");

            Type attributeType = typeof (T);
            Func<T, MemberInfo, bool> filterFunc = filter ?? ((a, t) => true);

            MemberInfo[] members = binding == BindingFlags.Default
                                       ? targetType.GetMembers()
                                       : targetType.GetMembers(binding);

            foreach (MemberInfo member in members) {
                object[] attributes = member.GetCustomAttributes(attributeType, true);
                if (attributes.no()) continue;
                bool filtered = false;
                foreach (T attr in attributes) {
                    if (filterFunc(attr, member)) {
                        filtered = true;
                        break;
                    }
                }
                if (filtered)
                    yield return member;
            }
        }

        public static Type[] toTypes(this object[] parameters) {
            return parameters.Select(p => null == p ? typeof (object) : p.GetType()).ToArray();
        }
    }
}