//   Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//   Supported by Media Technology LTD 
//    
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//    
//        http://www.apache.org/licenses/LICENSE-2.0
//    
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//   
//   MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Comdiv.Application;
using Comdiv.Extensibility.ExtensionsCompiler;
using Comdiv.IO;
using NUnit.Framework;

namespace Comdiv.Extensibility.Test.ExtensionsCompiler {
    [TestFixture]
    [Category("INTEGRATION")]
    [Category("LONGEXEC")]
    public class ByExeTest {
        [SetUp]
        public void setup() {
            FilePathResolverExtensions.Write(myapp.files, "~/extensions/a.boo",
                                             @"class ByCompilerTest:
    pass
registry['x'] = ByCompilerTest");

            try {
            	Directory.CreateDirectory("tmp");
                File.Delete("tmp\\ByExeTest.dll");
            }
            catch {
            }
        }

        [Test]
        public void can_generate_assembly() {
            Assert.False(File.Exists("tmp\\ByExeTest.dll"));
            var p = new Process();
            p.StartInfo = new ProcessStartInfo("extensionscompiler.exe", "--root \"\" --web false --dllname tmp\\ByExeTest")
                          {
                              UseShellExecute =
                                  false,
                              RedirectStandardOutput
                                  = false,
                              RedirectStandardError = false,
                          };
            p.Start();
            bool result = p.WaitForExit(360000);
            
           // var results = p.StandardOutput.ReadToEnd();
          //  Console.WriteLine(results);
            Console.ForegroundColor = ConsoleColor.Red;
           // Console.WriteLine(p.StandardError.ReadToEnd());
            Console.ResetColor();
            Assert.True(result);
            
            var assembly = Assembly.LoadFrom("tmp\\ByExeTest.dll");
            Assert.True(File.Exists("tmp\\ByExeTest.dll"));
            var r = new ExtensionsLoader().GetRegistry(assembly);
            Assert.True(r.ContainsKey("x"));
        }
    }
}