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
using System.Configuration;
using System.IO;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Comdiv.Extensions;
using Comdiv.Logging;

namespace Comdiv.Inversion{
    public static class WindsoreContainerExtensions{
        public static ILog log = logger.get("comdiv.inversion.windsore");
        public static void RegisterXml(this IWindsorContainer container)
        {
            log.Info("start load default xml");
            try{
                var i = new XmlInterpreter();
                container.RegisterXml(i);
            }
            catch (ConfigurationErrorsException ex)
            {
                log.Warn("some problems with configuration, in most cases - it was specified",ex);
                return;
            }
            
            log.Info("end load default xml");
        }


        public static void LoadDefaultXml(this IWindsorContainer container)
        {
            container.RegisterXml();
            container.RegisterXml("~/ioc.config");
            container.RegisterXml("~/sys/ioc.config");
            container.RegisterXml("~/mod/ioc.config");
            container.RegisterXml("~/usr/ioc.config");
            container.EndProcessXml();
        }

        public static void RegisterXml(this IWindsorContainer container,string xmlFile)
        {
            xmlFile = xmlFile.mapPath();
            if(!File.Exists(xmlFile)){
                log.Warn("file "+xmlFile+" not exists");
                return;
            }
            log.Info("start load container from "+xmlFile);
            container.RegisterXml(new XmlInterpreter(xmlFile));
            log.Info("end load container from " + xmlFile);
        }

        public static void RegisterXml(this IWindsorContainer container,XmlInterpreter interpreter)
        {
            
            if(null==container){
                return;
            }
            if(null==interpreter){
                return;
            }
            log.Info("start process xml");
            interpreter.ProcessResource(interpreter.Source, container.Kernel.ConfigurationStore);
            
            log.Info("end process xml");
        }

        public static void EndProcessXml(this IWindsorContainer container){
            var c = container as WindsorContainer;
            if(null!=c){
                c.Installer.SetUp(container,container.Kernel.ConfigurationStore);
            }
        }

        private static void register(this IWindsorContainer container, string name, Type type, IRegistration registration)
        {
            name = name.hasContent() ? name : defaultName(type);
            if (null != name){
                container.Remove(name);
            }
            if(null==name){
                name = Guid.NewGuid().ToString();
            }
            if (registration is ComponentRegistration<object> && name!=null)
            {
                ((ComponentRegistration<object>)registration).Named(name);

            }
            container.Register(registration);
        }

        public static void  Remove(this IWindsorContainer container, string name)
        {
            if (container.IsRegistered(name))
            {
                bool removed = container.Kernel.RemoveComponent(name);

                if (!removed)
                {
                    throw new InversionException("Cannot remove component with name " + name);
                }
            }
        }
        public static void Remove(this IWindsorContainer container, Type type)
        {
            //windsor has no ability to remove by types so we do hack with naming everything
            var name = defaultName(type);
            container.Remove(name);
        }


        public static  void RegisterTransient(this IWindsorContainer container, string name, Type serviveType, Type implementatinType, IDictionary<string, string> parameters)
        {
            //base.Service(name, serviveType, implementatinType, parameters);
            serviveType = serviveType ?? implementatinType;
            var c =
                Component.For(serviveType).ImplementedBy(implementatinType).LifeStyle.Transient.
                    OverWrite();
            if (parameters != null)
            {
                var _p = parameters.Select(p => Parameter.ForKey(p.Key).Eq(p.Value)).ToArray();
                c.Parameters(_p);
            }
            container.register(name, serviveType, c);
        }

        public static void RegisterService(this IWindsorContainer container,string name, Type type, object instance,IDictionary<string,string> parameters ){
            
            if (null == type) type = instance is Type ? (Type)instance : instance.GetType();
            
            if (instance is IRegistration){
                container.register(name, type, (IRegistration) instance);
                return;
                
            }
            var c = Component.For(type).LifeStyle.Singleton.OverWrite();
            if(parameters!=null){
                var _p = parameters.Select(p => Parameter.ForKey(p.Key).Eq((string) p.Value)).ToArray();
                c.Parameters(_p);
                
            }
            
            if(instance!=null){
                if(instance is Type){
                    var t = instance as Type;
                    c.ImplementedBy(t);
                }
                else{
                    c.Instance(instance);
                }
            }
            container.register(name,type,c);
            return;
            ;
        
        }

        private static string defaultName(Type type)
        {
            if(type==typeof(object)) return null;
            return type.FullName + ".__default__name";
        }

        public static bool IsRegistered(this IWindsorContainer container, string name)
        {
            return container.Kernel.HasComponent(name);
        }
        public static bool IsRegistered(this IWindsorContainer container, Type type)
        {
            return container.Kernel.HasComponent(type);
        }

        
    }
}