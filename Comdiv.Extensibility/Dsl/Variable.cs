using System;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility.Boo.Dsl{
    public static class Variable{
        public static DeclarationStatement Define(string name,Type type){
            return Define(name, type, true);
        }
        public static ReferenceExpression Ref(string  name){
            return new ReferenceExpression(name);
        }

        public static DeclarationStatement Define(string name,Type type,bool init){
            return Define(name, type, init, type);
        }

        public static DeclarationStatement Define(string name,Type type,bool init, Type initType){
            return new DeclarationStatement(
                new Declaration(name, type.GetTypeReference()),
                init?(Expression)initType.GetConstructorInvocation():new NullLiteralExpression()
                );
        }

        public static ReturnStatement Return(string name)
        {
            return new ReturnStatement(new ReferenceExpression(name));
        }
    }
}