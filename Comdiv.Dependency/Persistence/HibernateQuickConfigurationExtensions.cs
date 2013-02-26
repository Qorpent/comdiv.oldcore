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
using System.IO;
using Comdiv.Inversion;
using FluentNHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;

namespace Comdiv.Persistence
{
    public static class HibernateQuickConfigurationExtensions
    {
        public static string[] getCreationScript(this Configuration cfg)
        {
            return cfg.GenerateSchemaCreationScript(
                Dialect.GetDialect(cfg.Properties)
                );
        }

        public static string[] getDropScript(this Configuration cfg)
        {
            return cfg.GenerateDropSchemaScript(
                Dialect.GetDialect(cfg.Properties)
                );
        }

        public static IInversionContainer setupHibernate(this IInversionContainer container)
        {
           // if (checkexistence && !hasnhib()) return container;
            return setupHibernate(container, (IConnectionsSource)null);
        }

        public static IInversionContainer setupHibernate(this IInversionContainer container, IConnectionsSource connectionsSource)
        {

            //if (checkexistence && !hasnhib()) return container;
            return setupHibernate(container, connectionsSource, null);
        }

        private static bool hasnhib()
        {
            var d = AppDomain.CurrentDomain;
            foreach (var assembly in d.GetAssemblies())
            {
                if (assembly.GetName().Name.StartsWith("NHibernate"))
                {
                    return true;
                }
            }
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            return File.Exists(dir + "\\NHibernate.dll");
        }


        public static IInversionContainer setupHibernate(this IInversionContainer container,
                                                         params PersistenceModel[] models)
        {
        //    if (!hasnhib()) return container;
            return setupHibernate(container, null, models);
        }

        public static IInversionContainer	setupHibernate(this IInversionContainer container,
                                                         IConnectionsSource connectionsSource,
                                                         params PersistenceModel[] models)
        {

            try
            {
                var nhib = typeof(NHibernate.ISession).Assembly;
            }
            catch (Exception ex)
            {
                //NOTE:assume that NHibernate not supplied at all!!!! it's not mistake - people just don't want use ORM but app call this method
                return container;
            }

            if (connectionsSource != null)
            {
                container.ensureService(connectionsSource, "default.connectionssource");
            }
            else
            {
                container.ensureService<IConnectionsSource, DefaultConnectionsSource>("default.connectionssource");
            }
            if (null != models)
            {
                var i = 0;
                foreach (var model in models)
                {
                    i++;
                    container.ensureService(model, "model." + model.GetType().Name + "." + i);
                }
            }
            container
                .ensureService<IConfigurationProvider, DefaultConfigurationProvider>
                ("default.hibernate.configuration.provider")
                .ensureService<ISessionFactoryProvider, DefaultSessionFactoryProvider>
                ("default.hibernate.factory.provider")
                .ensureService<IStorage, HibernateStorage>("default.hibernate.storage")
                .AddTransient("default.temporary.current.session", typeof(TemporaryTransactionSession))
                .AddTransient("http.session.clear", typeof(HttpContextSessionCleaner))
                .AddTransient("default.hibernate.applyer", typeof(HibernatePropertyApplyer))
                .AddTransient("default.hibernate.applyer.impl", typeof(DefaultHibernatePropertyApplyerImpl))
                ;

            return container;
        }

    }
}