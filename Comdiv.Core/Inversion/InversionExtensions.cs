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
using System.Linq;
using Comdiv.Application;
using Comdiv.Design;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;
using Qorpent.Security;

namespace Comdiv.Inversion{
    ///<summary>
    ///</summary>
    public static class InversionExtensions{
        public static IInversionContainer ensureService<TService>(this IInversionContainer container){
            return ensureService<TService>(container, null);
        }


        public static T typenameOrIoc<T>(string name){
            if (name.noContent()) return default(T);
            var type = Type.GetType(name);
            if(null!=type){
                return type.create<T>();
            }
            return (T)myapp.Container.Resolve(name);
        }

        /// <summary>
        /// Возвращает набор целевых расширений
        /// </summary>
        /// <typeparam name="TMatch"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static MatchedExtensionsSet<TMatch> _matched<TMatch>(this TMatch target)
        {
            return new MatchedExtensionsSet<TMatch>(target);
        }

        /// <summary>
        /// extension-like call to fill target list with typed items from IOC
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        [Sugar]
        public static IList<I> fillFromLocator<I>(this IList<I> target)
        {
            foreach (var i in Container.all<I>())
            {
                target.Add(i);
            }
            return target;
        }

        private static IInversionContainer _container;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(InversionExtensions)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        
        public static T first<T>(this IInversionContainer container){
            return (T)container.ResolveAll(typeof (T)).FirstOrDefault();
        }

        public static T get<T>(this string nameortype)
        {
            T t = default(T);
            if (!nameortype.Contains(",")){
             t =   ioc.get<T>(nameortype);
            }else{
            
                t = nameortype.toType().create<T>();
            }
            return t;
        }


        public static bool invalid(this IInversionContainer container){
            if(null==container){
                return true;
            }
            if(container.Disposed){
                return true;
            }
            return false;
        }

        public static IInversionContainer ensureService<TService>(this IInversionContainer container,string name){
            return ensureService<TService,TService>(container,name);
        }

        public static IInversionContainer ensureService<TService>(this IInversionContainer container, TService impl){
            return ensureService<TService>(container, impl, null);
        }

        public static IInversionContainer ensureService<TService>(this IInversionContainer container, TService impl,string name)
        {
            if (null == container) return null;
            if (name.noContent() && container.Exists(typeof(TService))) return container;
            if (name.hasContent() && container.Exists(name)) return container;
            container.AddSingleton( name, typeof(TService),impl);
            return container;
        }

        public static IInversionContainer ensureService<TService>(this IInversionContainer container, Type impl, string name)
        {
            if (null == container) return null;
            if (name.noContent() && container.Exists(typeof(TService))) return container;
            if (name.hasContent() && container.Exists(name)) return container;
            container.AddSingleton(name, typeof(TService), impl);
            return container;
        }


        public static IInversionContainer ensureService<TService,TImplementation>(this IInversionContainer container){
            return ensureService<TService, TImplementation>(container, null);
        }

        public static IInversionContainer ensureService<TService,TImplementation>(this IInversionContainer container, string name){
            if (null == container) return null;
            if(container.Exists(typeof(TService))) return container;
            container.AddSingleton(name, typeof (TService), typeof (TImplementation));
            return container;
        }

        public static T get<T>(this IInversionContainer container){
            return (T) container.Resolve(typeof (T));
        }

        public static T get<T>(this IInversionContainer container, string name)
        {
            if(Type.GetType(name)!=null){
                return (T)container.get(Type.GetType(name));
            }
            return (T)container.Resolve(name);
        }

        public static object get(this IInversionContainer container, string name)
        {
            if (Type.GetType(name) != null)
            {
                return container.get(Type.GetType(name));
            }
            return container.Resolve(name);
        }
        public static object get(this IInversionContainer container, Type type)
        {
            var result = container.Resolve(type);
            if(result==null && type!=null && type.IsInterface){
				try {
					return type.create();
				}catch(FormatException ex) {
					return null;
				}
            }
            return result;
        }

        public static IInversionContainer set(this IInversionContainer container, string name, object value)
        {
           
            if(value is Type){
                container.AddTransient(name, (Type) value);
            }else{
                container.AddSingleton(name, value.GetType(), value);
            }
            return container;
            ///return (T)container.Resolve(typeof(T));
        }

        public static IEnumerable<T> all<T>(this IInversionContainer container){
            return container.ResolveAll(typeof (T)).Cast<T>().ToArray();
        }

        public static IInversionContainer setupSecurity(this IInversionContainer container, params Type[] extensions)
        {
            if (null == container)
            {
                return null;
            }
            if (extensions == null || extensions.Length == 0)
            {
                extensions = new[] { typeof(XmlRoleProvider<ApplicationXmlReader>) };
            }
            foreach (var extension in extensions)
            {
                container.AddTransient(extension.Name, extension);
            }
            return
                container
                    .ensureService<IPrincipalSource, PrincipalSource>()
                    .ensureService<IImpersonator, Impersonator>()
                    .ensureService<IRoleResolver, RoleResolver>()
                    .ensureService<IAclRepository, DefaultAclRepository>()
                    .ensureService<IAclProviderService, DefaultAclProvider>()
                    .ensureService<IAclTokenResolver, DefaultAclTokenResolver>()
                    .ensureService<IAclInMemoryRuleProvider, DefaultAclInMemoryRuleProvider>()
                    .ensureService<IAclApplicationRuleProvider, DefaultAclApplicationRuleProvider>()
                    .ensureService<IAclProfileManager, DefaultAclProfileManager>();

        }
        public static IInversionContainer setupFilesystem(this IInversionContainer container)
        {
            if (null == container)
            {
                return null;
            }
            return
                container
                    .ensureService<IFilePathResolver, DefaultFilePathResolver>()
                    .ensureService<IFileTemplateRepository,FileTemplateRepository>("file.template.repository.transient")
                ;

        }

        //public static IInversionContainer setupDefaultStorageResolver(this IInversionContainer container)
        //{
        //    if (null == container)
        //    {
        //        return null;
        //    }
        //    return
        //        container
        //            .ensureService<IStorageResolver, DefaultStorageResolver>()
        //        ;

        //}
    }
}