using System;

namespace Comdiv.Useful{
    public static class ConsoleExtensions{
        /// <summary>
        /// reads argument from console parameters, using notation --argname [argvalue]
        /// </summary>
        /// <param name="args">arg list</param>
        /// <param name="argName">argument name to find</param>
        /// <returns>
        /// "" - is no -argname exists or if argvalue not exists, or argvalue is next param in form --argvalue
        /// argvalue if exists
        /// </returns>
        public static string getArg(this string[] args, string argName){
            return getArg(args, argName, "");
        }

        /// <summary>
        /// reads argument from console parameters, using notation --argname [argvalue]
        /// </summary>
        /// <param name="args">arg list</param>
        /// <param name="argName">argument name to find</param>
        /// <param name="defaultValue">default value to be returned</param>
        /// <returns>
        /// defaultValue - is no -argname exists or if argvalue not exists, or argvalue is next param in form --argvalue
        /// argvalue if exists
        /// </returns>
        public static string getArg(this string[] args, string argName, string defaultValue){
            int idx = Array.IndexOf(args, "--" + argName);
            if (-1 == idx){
                return defaultValue;
            }
            if (idx == args.Length - 1){
                return defaultValue;
            }
            string value = args[idx + 1];
            if (value.StartsWith("--")){
                return defaultValue;
            }
            return value;
        }

        public static void print(this string str, params object[] args){
            Console.WriteLine(str, args);
        }
    }
}