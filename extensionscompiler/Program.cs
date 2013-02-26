using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Comdiv.Extensibility.ExtensionsCompiler;
using Comdiv.Extensions;
using Comdiv.MAS;

namespace extensionscompiler {
    class Program:MasConsoleApplication {
        public Program() {
            this.CanIgnoreMas = true;
            this.IgnoreMasByDefault = true;
        }

        private static string outfile;
        static string root ="";
        static void Main(string[] args) {
           
           // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
           new Program().Run(args);

        }

        //static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    var dll =
        //        Directory.EnumerateFiles(root, args.Name.Split(',')[0].Trim() + ".dll",
        //                                 SearchOption.AllDirectories)
        //            .Where(x => !x.ToLower().Contains("\\extensionslib\\")).FirstOrDefault();
        //    if (dll.hasContent())
        //    {
        //        return Assembly.LoadFrom(dll);
        //    }
        //    write("no loaded "+args.Name);
        //    return null;
        //}

        protected override void execute() {
            IList<string> log = new List<string>();
            var a = new Comdiv.ConsoleUtils.ConsoleArgumentParser().Parse(CommandArgs);
            root = a.get("root","../");
            outfile = a.get("outfile");
            var isdebug = a.get("debug").toBool();
            if (isdebug) Debugger.Break();
            var exec = new ExtensionsCompilerConsoleProgram();
            exec.WriteLog = new Action<string>(s => log.Add(s));
            exec.Execute(CommandArgs);
            foreach (var l in log)
            {
                write(l);
            }
        }

        private void write(string l) {

            loginfo(l);
            if (outfile.hasContent())
            {
                File.AppendAllText(outfile, l + Environment.NewLine);
            }
            //else
            //{
            //    Console.WriteLine(l);
            //}
        }
    }
}
