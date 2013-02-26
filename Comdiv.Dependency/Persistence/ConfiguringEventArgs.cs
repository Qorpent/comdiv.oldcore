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
using NHibernate.Cfg;

namespace Comdiv.Persistence{
    /// <summary>
    /// Provides data for the <see cref="IConfigurationProvider.BeforeConfigure"/> event.
    /// </summary>
    public class ConfiguringEventArgs : EventArgs{
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguringEventArgs"/> class.
        /// </summary>
        /// <param name="configuration">An istance of <see cref="NHibernate.Cfg.Configuration"/> not configured.</param>
        /// <remarks>
        /// At this point the method <see cref="NHibernate.Cfg.Configuration.Configure()"/> 
        /// should not be called.
        /// </remarks>
        public ConfiguringEventArgs(Configuration configuration){
            Configuration = configuration;
            Configured = false;
        }

        /// <summary>
        /// The not-configured <see cref="NHibernate.Cfg.Configuration"/>
        /// </summary>
        public Configuration Configuration { get; private set; }

        /// <summary>
        /// Set to <see langword="true"/> if your event handler are managing the whole NHibernate 
        /// configuration process (it call <see cref="NHibernate.Cfg.Configuration.Configure()"/> or one of
        /// its overloads).
        /// </summary>
        public bool Configured { get; set; }
    }
}