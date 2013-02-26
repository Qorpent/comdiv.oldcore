using System;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler.Ast;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Boo.Dsl
{
    public static class DslAstUtil{

        public static ExpressionStatement Assign(this Expression left, Expression right){
            return new ExpressionStatement(
                new BinaryExpression(
                    BinaryOperatorType.Assign,
                    left,right
                    )
                );
        }

        public static SlicingExpression ByIndex(this Expression expression,Expression indexer){
            return new SlicingExpression(expression,indexer);
        } 

        public static T  GetParentNodeOfType<T>(this Node node)where T:Node{
            Node p = node;
            while(null!=(p=p.ParentNode)){
                if(p is T) return (T)p;
            }
            return null;
        }

        public static Statement AsLocalVar(this Type type,string name){
            return new DeclarationStatement(
                                                                                      new Declaration(name, type.GetTypeReference()),
                                                                                      type.GetConstructorInvocation()
                                                                                      );
        }

        public static string LiftToString(this Expression expression){
            if (expression is StringLiteralExpression) return ((StringLiteralExpression) expression).Value;
            return expression.ToCodeString();
        }

         public static IEnumerable<TypeMember> CreateSimpleProperty(string name,Type type){
             return CreateSimpleProperty(name, type.FullName);
         }
        public static IEnumerable<TypeMember> CreateSimpleProperty(string name,string typeName){
            var t = new SimpleTypeReference(typeName);
            var f = new Field();
            f.Modifiers = TypeMemberModifiers.Private;
            f.Type = t;
            f.Name = name.ToFieldName();
            var p = new Property(name.ToPropertyName());
            p.Type = t;
            p.Modifiers = TypeMemberModifiers.Virtual;
            p.Getter = new Method("getz");
            p.Setter = new Method("setz");
            p.Getter.Body.Add(new ReturnStatement(new ReferenceExpression(f.Name)));
            p.Setter.Body.Add(new BinaryExpression(BinaryOperatorType.Assign, new ReferenceExpression(f.Name), new ReferenceExpression("value")));
            return new TypeMember[]{f, p};
        }

        

        public static TypeReference GetTypeReference(this Type type){
            if(!type.IsGenericType)return new SimpleTypeReference(type.FullName);
            var result = new GenericTypeReference();
            result.Name = type.FullName.Split('`')[0];
            foreach (var generic in type.GetGenericArguments()  ){
                result.GenericArguments.Add(new SimpleTypeReference(generic.FullName));
            }
            return result;
        }

        public static MethodInvocationExpression GetConstructorInvocation(this Type type,params Expression[] args)
        {
            MethodInvocationExpression result = new MethodInvocationExpression();
            Expression ctor = null;
            if (type.IsGenericType){
                ctor = new GenericReferenceExpression();
                ((GenericReferenceExpression)ctor).Target =
                    AstUtil.CreateReferenceExpression(
                       type.FullName.Split('`')[0]);
                foreach (var generic in type.GetGenericArguments())
                {
                    ((GenericReferenceExpression)ctor).GenericArguments.Add(new SimpleTypeReference(generic.FullName));
                }
            }else{
                ctor = AstUtil.CreateReferenceExpression(
                    type.FullName);
            }
            result.Target = ctor;
            foreach (var declaration in args){
                result.Arguments.Add(declaration);
            }
            return result;
        }

        public static ClassDefinition FindOrCreateClass(this CompileUnit unit, string nameSpace, string name,Action<Module> modPreparator, Action<ClassDefinition> ctor){
            Module module = null;
            var result = findExistedClass(unit, nameSpace, name, out module);
            if(null==result){ 
                if (modPreparator.yes()) modPreparator(module);
                result = new ClassDefinition();
                result.Name = name;
                module.Members.Add(result);
                if (ctor.yes()) ctor(result);
                
            }
            return result;
        }

        public static T FindOrCreateMember<T>(this ClassDefinition cls, string name, Action<T> ctor) where T : TypeMember,new()
        {
            var result = findExistedMember<T>(cls, name);
            if(null==result){
                result = new T();
                result.Name = name;
                cls.Members.Add(result);
                if (ctor.yes()) ctor(result);
            }
            return result;
        }

        public static NamespaceDeclaration GetEnclosingNamespace(this Node node){
            bool resolved = false;
            NamespaceDeclaration result;
            Func<Node, NamespaceDeclaration> resolve = n =>
                                                       {
                                                           resolved = true;
                                                           if (null == n) return null;
                                                           if (n is CompileUnit) return null;
                                                           if (n is NamespaceDeclaration)
                                                               return (NamespaceDeclaration) n;
                                                           if (n is Module) return ((Module) n).Namespace;
                                                           resolved = false;
                                                           return null;
                                                       };
            var current = node;
            while (null!=current){
                result = resolve(current);
                if (resolved) return result;
                current = current.ParentNode;
            }
            return null;
        }
        public static Module GetEnclosingModule(this Node node)
        {
            bool resolved = false;
            Module result;
            Func<Node, Module> resolve = n =>
            {
                resolved = true;
                if (null == n) return null;
                if (n is CompileUnit) return null;
                if (n is Module) return (Module)n;
                resolved = false;
                return null;
            };
            var current = node;
            while (null != current)
            {
                result = resolve(current);
                if (resolved) return result;
                current = current.ParentNode;
            }
            return null;
        }
        

        private static T findExistedMember<T>(ClassDefinition cls, string name) where T:TypeMember{
            return cls.Members.OfType<T>().FirstOrDefault(m => m.Name == name);
        }

        private static ClassDefinition findExistedClass(CompileUnit unit, string nameSpace, string name,out Module bestModule) {
            bestModule = null;
            foreach (var module in unit.Modules){
                if (module.Namespace == null) continue;
                if (module.Namespace.Name != nameSpace) continue;
                bestModule = module;
                var cls = module.Members.FirstOrDefault(m => m.Name == name);
                if(cls.no()) continue;
                return (ClassDefinition)cls;
            }
            return null;
        }
    }
}
