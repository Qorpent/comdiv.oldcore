using System;
using System.Collections;
using System.Text;
using Comdiv.HbmCompiler;

namespace hbmcompiler {
    class Program {
        static int Main(string[] args) {
            try {
                var exec = new HbmCompilerConsoleProgram();
                exec.Execute(args, Console.WriteLine);
                return 0;
            }catch(Exception ex) {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}
