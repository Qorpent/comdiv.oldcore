using System;

namespace Comdiv.Extensibility{
    public abstract class RootBasedExpressionEvaluatorBase:IRootBasedExpressionEvaluator{
        public object Eval(object root){
            lock (this){
                return internalEval(root);
            }
        }

        protected abstract object internalEval(object root);
    }

    public abstract class RootBasedExpressionEvaluatorBase<T> : RootBasedExpressionEvaluatorBase
    {
        protected T target;
        protected T x
        {
            get { return target; }
        }
        protected override object internalEval(object root)
        {
            this.target = (T)root;
            try{
                return expression();
            }catch(NullReferenceException){
                return "";
            }
        }
        public abstract object expression();
    }
}