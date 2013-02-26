using System;
using System.Linq;
using System.Xml.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;
using Qorpent.Bxl;

namespace Comdiv.Security
{
    public class RoleOperator
    {
        static object sync = new object();
        private const string filename = "~/usr/security.map.bxl";

        public void AssignToRole(string username, string role) {
            lock(sync) {
                if (!myapp.roles.IsInRole(username.toPrincipal(), role)) {
                    appendToFile(new XElement("map", new XAttribute("id", username.ToLower().Replace("\\","/")),
                                              new XAttribute("name", role.ToUpper())));
                    ((RoleResolver)myapp.roles).Reload();
                }
            }
        }

        public void RemoveFromRole(string  username, string  role) {
            lock (sync)
            {
                if (myapp.roles.IsInRole(username.toPrincipal(), role))
                {
                    removeFromFile("map",username.ToLower().Replace("\\","/"), role.ToUpper());
                    ((RoleResolver)myapp.roles).Reload();
                }
            }
        }


        private void removeFromFile(string map, string code, string name) {
        	if (code == null) throw new ArgumentNullException("code");
        	var generator = new Booxml.BooxmlGenerator();
            var file = myapp.files.Resolve(filename, true);
            
            if (null == file) return;
            XElement x = new BxlParser().Parse(myapp.files.Read(filename));
            foreach (var e in x.Elements(map).ToArray()) {
                if(e.attr("code")==code && e.attr("name")==name) {
                    e.Remove();
                }
            }
            myapp.files.Write(filename, generator.Generate(x));
        }

        private void appendToFile(XElement e) {
            
            var generator = new Booxml.BooxmlGenerator();
            var file = myapp.files.Resolve(filename,true);
            XElement x = null;
            if (null == file) {
                x = new XElement("root");
            }else {
                var parser = new BxlParser();
                x = parser.Parse(myapp.files.Read(filename));
            }
            x.Add(e);
            myapp.files.Write(filename, generator.Generate(x));

        }
    }
}
