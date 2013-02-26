using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Comdiv.Extensions;

namespace Comdiv.IO.FileSystemScript {
    public class FileSystemCommandLocator {
        static IDictionary<string , Type> cache = new Dictionary<string, Type>();

        public IFileSystemCommand Get(XElement xmldata) {
            return Get(xmldata.Name.LocalName, xmldata);
        }

        public IFileSystemCommand Get(string code,XElement xmldata = null) {
            var type =  cache.get(code, () =>
                                            {
                                                var assemblies =  AppDomain.CurrentDomain.GetAssemblies().ToList();
                                                assemblies.Remove(Assembly.GetExecutingAssembly());
                                                assemblies.Insert(0, Assembly.GetExecutingAssembly()); //moves comdiv core to first queue due to that fact that most of commands will be created here
                                                foreach (var assembly in assemblies) {
                                                    if (assembly.GetName().Name.StartsWith("mscorlib")) continue;
                                                    if (assembly.GetName().Name.StartsWith("System.")) continue;
                                                    if (assembly.GetName().Name.StartsWith("Microsoft.")) continue;
                                                    foreach (var t in assembly.GetTypes()) {
                                                        if(typeof(IFileSystemCommand).IsAssignableFrom(t)) {
                                                            var a = t.getFirstAttribute<FileSystemCommandAttribute>();
                                                            if(null!=a) {
                                                                if(a.CommandCode==code) {
                                                                    return t;
                                                                }
                                                            }
                                                        }
                                                    }
                                               
                                                }
                                                return null;
                                            });
            if(null==type) {
                throw new Exception("cannot locate class to implement "+code+" command");
            }
            var result = type.create<IFileSystemCommand>();
            result.Code = code;
            if(xmldata!=null) {
                xmldata.applyTo(result);
                foreach (var attribute in xmldata.Attributes()) {
                    result.Args[attribute.Name.LocalName] = attribute.Value;
                }
            }
            return result;
        }
    }
}