using System;
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Brail {
    public class OutputWriteUnification : ICompilerStep {
        private CompilerContext context;
        public class TranslateOutputWriteToWrite:AbstractVisitorCompilerStep {
            public TranslateOutputWriteToWrite(CompilerContext context) {
                Initialize(context);
            }
            public override void OnMethodInvocationExpression(MethodInvocationExpression node)
            {
                if(isOutputStreamWrite(node)) {
                    make__write(node);
                }
            }
            public override void Run() {
                Visit(Context.CompileUnit.Modules);
            }
        }

        public class ExpandsNestedWrites : AbstractVisitorCompilerStep
        {
            public ExpandsNestedWrites(CompilerContext context)
            {
                Initialize(context);
            }
            public override void OnMethodInvocationExpression(MethodInvocationExpression node)
            {
                if (isWrite(node))
                {
                    while(isWrite(node.Arguments[0])) {
                        node.Arguments[0] = get_arg(node.Arguments[0]);
                    }
                }
                base.OnMethodInvocationExpression(node);
            }
            public override void Run()
            {
                Visit(Context.CompileUnit.Modules);
            }
           
        }


        public class JoinInterpolations : AbstractVisitorCompilerStep
        {
            public JoinInterpolations(CompilerContext context)
            {
                Initialize(context);
            }
            public override void OnMethodInvocationExpression(MethodInvocationExpression node)
            {
                if (isWrite(node)) {
                    var arg = get_arg(node);
                    if(arg is ExpressionInterpolationExpression) {
                        arg = joinInterpolations((ExpressionInterpolationExpression) arg);
                        set_arg(node, arg);
                    }
                }
                base.OnMethodInvocationExpression(node);
            }

            public override void Run()
            {
                Visit(Context.CompileUnit.Modules);
            }
        }


        public class JoinFollowingWrites : AbstractVisitorCompilerStep
        {
            public JoinFollowingWrites(CompilerContext context)
            {
                Initialize(context);
            }
            public override void OnExpressionStatement(ExpressionStatement node) {

                if(isWrite(node.Expression)) {
                    var arg = get_arg(node.Expression);
                    var first = arg;
                    if (!(arg is ExpressionInterpolationExpression))
                    {
                        
                        arg = new ExpressionInterpolationExpression();
                    }
                    var block = ((Block) node.ParentNode);
                    var index = block.Statements.IndexOf(node);
                    if(index==-1) return;
                    while (true) {
                        if (index > block.Statements.Count - 2) break;
                        var next = block.Statements[index + 1];
                        if(!(next is ExpressionStatement))break;
                        if(!isWrite(((ExpressionStatement)next).Expression)) break;
                        block.Statements.Remove(next);
                        var nextarg = get_arg(((ExpressionStatement) next).Expression);
                        if (nextarg is ExpressionInterpolationExpression)
                        {
                            foreach (var e in ((ExpressionInterpolationExpression)nextarg).Expressions) {
                                ((ExpressionInterpolationExpression)arg).Expressions.Add(e);  
                            }
                        }
                        else {
                            ((ExpressionInterpolationExpression) arg).Expressions.Add(nextarg);
                        }
                    }
                    if(((ExpressionInterpolationExpression)arg).Expressions.Count>0) {
                        if (first != arg) {
                            ((ExpressionInterpolationExpression) arg).Expressions.Insert(0, first);
                        }
                        set_arg(node.Expression, arg);
                    }
                }
            }
            public override void Run()
            {
                Visit(Context.CompileUnit.Modules);
            }

        }


        public void Run()
        {
         //   Console.WriteLine(Context.CompileUnit.Modules[0].ToCodeString());
          //  Console.WriteLine("-----------------------------------------");
            new TranslateOutputWriteToWrite(context).Run();
            new ExpandsNestedWrites(context).Run();
            new JoinFollowingWrites(context).Run();
            new JoinInterpolations(context).Run();
          //  Console.WriteLine(Context.CompileUnit.Modules[0].ToCodeString());
        }

        public void Initialize(CompilerContext context) {
            this.context = context;
        }

        public void Dispose() {
            
        }

        private static bool isOutputStreamWrite(MethodInvocationExpression node) {
            return node.Target.ToCodeString() == "OutputStream.Write";

        }
        private static bool isWrite(Expression node)
        {
            if (!(node is MethodInvocationExpression)) return false;
            return ((MethodInvocationExpression)node).Target.ToCodeString() == "__write";

        }

        private static void make__write(MethodInvocationExpression node) {
            node.Target = new ReferenceExpression("__write");
        }

        private static Expression get_arg(Expression expression) {
            return ((MethodInvocationExpression) expression).Arguments[0];
        }
        private static Expression set_arg(Expression expression, Expression arg)
        {
            return ((MethodInvocationExpression)expression).Arguments[0] = arg;
        }

        private static Expression joinInterpolations(ExpressionInterpolationExpression expressionInterpolationExpression) {
            var srcexpressions = expressionInterpolationExpression.Expressions.ToList();
            ExpressionInterpolationExpression result = new ExpressionInterpolationExpression();
            StringLiteralExpression current = null;
            foreach (var srcexpression in srcexpressions) {
                if(srcexpression is LiteralExpression) {
                    var str = new StringLiteralExpression(((LiteralExpression) srcexpression).ValueObject.toStr());
                    if(null==current) {
                        current = str;
                    }else {
                        current.Value += str.Value;
                    }
                }else {
                    if(current!=null) {
                        result.Expressions.Add(current);
                        current = null;
                    }
                    result.Expressions.Add(srcexpression);
                }
            }
            if (current != null)
            {
                result.Expressions.Add(current);
            }
            if(result.Expressions.Count>1) return result;
            return result.Expressions[0];
        }

    }

   
}