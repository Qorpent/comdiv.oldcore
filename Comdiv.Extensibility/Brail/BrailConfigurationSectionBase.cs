using System;
using System.Configuration;
using System.Reflection;
using System.Xml;
using Comdiv.Extensibility.Brail;

namespace Castle.MonoRail.Views.Brail {
    public abstract class BrailConfigurationSectionBase : IConfigurationSectionHandler {
        public abstract ViewEngineOptions CreateNew();

        public object Create(object parent, object configContext, XmlNode section) {
            var options = CreateNew();
            if (section.Attributes["baseType"]!=null){
                options.BaseType = section.Attributes["baseType"].Value;
            }
            if (section.Attributes["batch"] != null)
                options.BatchCompile = bool.Parse(section.Attributes["batch"].Value);
            if (section.Attributes["saveToDisk"] != null)
                options.SaveToDisk = bool.Parse(section.Attributes["saveToDisk"].Value);
            if (section.Attributes["debug"] != null)
                options.Debug = bool.Parse(section.Attributes["debug"].Value);
            if (section.Attributes["saveDirectory"] != null)
                options.SaveDirectory = section.Attributes["saveDirectory"].Value;
            if (section.Attributes["collectstatistics"] != null)
                options.CollectStatistics = bool.Parse(section.Attributes["collectstatistics"].Value);
            if (section.Attributes["savePreprocessDirectory"] != null)
                options.SavePreprocessedDirectory = section.Attributes["savePreprocessDirectory"].Value;
            if (section.Attributes["commonScriptsDirectory"] != null)
                options.CommonScriptsDirectory = section.Attributes["commonScriptsDirectory"].Value;
            foreach(XmlNode refence in section.SelectNodes("reference"))
            {
                XmlAttribute attribute = refence.Attributes["assembly"];
                if (attribute == null)
                    throw GetConfigurationException("Attribute 'assembly' is mandatory for <reference/> tags");
                Assembly asm = Assembly.Load(attribute.Value);
                options.AssembliesToReference.Add(asm);
            }

            foreach (XmlNode import in section.SelectNodes("import")){
                XmlAttribute attribute = import.Attributes["namespace"];
                if (attribute == null)
                    throw GetConfigurationException("Attribute 'namespace' is mandatory for <import/> tags");
                string name = attribute.Value;
                options.NamespacesToImport.Add(name);
            }
            //configures preprocessors in factory mode, by design it means that preprocessorFactoryType is
            //container-impl independent way too access used IOC by straightfully defined type
           /* if (section.Attributes["preprocessorFactoryType"] != null){
                var preprocessorFactoryTypeName = section.Attributes["preprocessorFactoryType"].Value;
                var preprocessorFactory =
                    (IBrailPreprocessorFactory)
                    Type.GetType(preprocessorFactoryTypeName).GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (var preprocessor in preprocessorFactory.GetPreprocessors()){
                    options.Preprocessors.Add(preprocessor);
                }
            }
             */  
            return options;
        }

        private static Exception GetConfigurationException(string error)
        {
            return new ConfigurationErrorsException(error);
        }
    }
}