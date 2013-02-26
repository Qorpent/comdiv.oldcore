using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comdiv.Extensions{
    public static class AdvancedIoExtensions{
        public static Process toCmdProcess(this string fileName)
        {
            var pi = new ProcessStartInfo
                     {
                         FileName = fileName,
                         RedirectStandardOutput = true,
                         RedirectStandardError = true,
                         UseShellExecute = false,
                         CreateNoWindow = true
                     };
            var p = new Process {StartInfo = pi};
            return p;
        }

        public static string run(this Process process)
        {
            return run(process, 0);
        }

        public static string run(this Process process, int miliseconds)
        {
            process.Start();
            if (miliseconds != 0)
            {
                process.WaitForExit(miliseconds);
            }
            else
            {
                process.WaitForExit();
            }
            return process.StandardOutput.ReadToEnd() + "\r\n" + process.StandardError.ReadToEnd();
        }
    }
}