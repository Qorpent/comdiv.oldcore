using System.Linq;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility.Boo.Dsl{
    public static class MethodUtils{

       

        public static T Static<T>(this T method)where T:TypeMember{
            method.Modifiers = method.Modifiers | TypeMemberModifiers.Static;
            return method;
        }
        public static ClassDefinition Static(this ClassDefinition method) 
        {
            method.Modifiers = method.Modifiers | TypeMemberModifiers.Static | TypeMemberModifiers.Final | TypeMemberModifiers.Abstract;
            return method;
        }
        public static T Private<T>(this T method) where T : TypeMember
        {
            method.Modifiers = method.Modifiers | TypeMemberModifiers.Private;
            return method;
        }
        public static Method As<T>(this Method method){
            method.ReturnType = typeof (T).GetTypeReference();
            return method;
        }
        public static Field As<T>(this Field method)
        {
            method.Type = typeof(T).GetTypeReference();
            return method;
        }
        public static Field Init(this Field method,string methodName,params Expression[] args)
        {
            method.Initializer = new MethodInvocationExpression(new ReferenceExpression(methodName), args);
            return method;
        }
        public static Property As<T>(this Property method)
        {
            method.Type = typeof(T).GetTypeReference();
            return method;
        }
        public static Property NoSetter(this Property method)
        {
            method.Setter = null;
            return method;
        }

        public static Property ReturnsField(this Property method,string name)
        {
            method.Getter = new Method(); 
            method.Getter.Body.Add(new ReturnStatement(new ReferenceExpression(name)));
            return method;
        }

        public static Method Append(this Method method, Statement statement)
        {
            return Append(method, 0, statement);
        }

        public static Method Append(this Method method, int keepRest, Statement statement)
        {
            var length = method.Body.Statements.Count();
            var index = length;
            if (length > keepRest) index = length - keepRest;
            method.Body.Statements.Insert(index, statement);
            return method;
        }
    }
}