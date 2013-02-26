using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Comdiv.Extensions;

namespace Comdiv.Useful{
    public static class AdvancedReflectionExtensions{
        public static string toSygnatureString(this MethodInfo info)
        {
            //Contract.Requires(null!=info);
            var b = new StringBuilder();
            if (info.IsPublic) b.Append("public ");
            if (info.IsFamily) b.Append("protected ");
            if (info.IsAssembly) b.Append("internal ");
            if (info.IsStatic) b.Append("static ");
            if (info.IsVirtual) b.Append("virtual ");
            if (info.ReturnType == null || info.ReturnType.Equals(typeof(void))) b.Append("void ");
            else b.Append(info.ReturnType.toSygnatureString() + " ");
            b.Append(info.Name);
            if (info.IsGenericMethodDefinition) b.Append((string)_getGenericString(info.GetGenericArguments()));
            b.Append("(");
            if (info.hasAttribute<ExtensionAttribute>()) b.Append("this ");
            var parameters = new List<string>();
            foreach (var parameter in info.GetParameters())
            {
                parameters.Add("{2}{0} {1}"._format(
                                   parameter.ParameterType.toSygnatureString(),
                                   parameter.Name,
                                   parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).yes()
                                       ? "params "
                                       : ""
                                   ));
            }
            b.Append(String.Join(", ", parameters.ToArray()));
            b.Append(")");
            return b.ToString();
        }

        public static string toSygnatureString(this Type type)
        {

            // Contract.Assert(type!=null);
            var result = type.Name;
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                //Contract.Assume(args != null);
                result += _getGenericString(args);
            }
            return result;
        }

        private static string _getGenericString(IEnumerable<Type> args)
        {
            //Contract.Requires(args != null);
            var genstr = "<";
            foreach (var argument in args)
            {
                genstr += argument.toSygnatureString();
            }
            genstr += ">";
            return genstr;
        }
    }
}