using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Comdiv.Design;
using NHibernate.Criterion;
using Expression=System.Linq.Expressions.Expression;

namespace Comdiv.Persistence
{
    public class ByExpressionsNHibernateQueryExecutor<T,R>{
        private StorageWrapper storage;
        private IEnumerable<Expression<Func<T, object>>> parameters;

        public ByExpressionsNHibernateQueryExecutor(StorageWrapper storage,Expression<Func<T, object>> first, params Expression<Func<T, object>>[] commonParameters){
            this.storage = storage;
            this.parameters = new[]{first}.Union(commonParameters);

        }
        
        public IEnumerable<R> Execute(){
            var realparameters = getParameters(this.parameters);
            return storage.Query<T, R>(realparameters.First(),(object[]) realparameters.Skip(1).ToArray());
        }

        private IEnumerable<object> getParameters<T>(IEnumerable<Expression<Func<T, object>>> expressions)
        {
            foreach (var e in expressions)
            {
                yield return getParameter<T>(e);
            }
        }

        private object getParameter<T>(Expression<Func<T, object>> expression)
        {
            var binexp = ((UnaryExpression)expression.Body).Operand as BinaryExpression;
            var unexp = ((UnaryExpression)expression.Body).Operand as UnaryExpression;
            if (null != binexp)
            {
                return getByBinaryExpression(binexp);
            }
            if(null!=unexp && unexp.NodeType==ExpressionType.Not){
                return Restrictions.Not(
                    (ICriterion) getByBinaryExpression((BinaryExpression) unexp.Operand)
                    );
            }
            throw new NotSupportedException("cannot proceed "+expression);
        }

        private object getByBinaryExpression(BinaryExpression expression)
        {

            if(expression.NodeType == ExpressionType.OrElse){
                return Restrictions.Or(
                    (ICriterion) getByBinaryExpression((BinaryExpression) expression.Left),
                    (ICriterion) getByBinaryExpression((BinaryExpression) expression.Right)
                    );
            }

            if(expression.NodeType == ExpressionType.AndAlso){
                return Restrictions.And(
                    (ICriterion)getByBinaryExpression((BinaryExpression)expression.Left),
                    (ICriterion)getByBinaryExpression((BinaryExpression)expression.Right)
                    );
            }

            var prop = expression.Left as MemberExpression;
            
            if(prop==null||prop.Member.MemberType!=MemberTypes.Property || !(prop.Expression is ParameterExpression))throw new NotSupportedException("left side must be property reference");

            var my = (prop.Expression as ParameterExpression).Name;
            
            var propname = prop.Member.Name;
            var proptest = expression.Right is MemberExpression 
                && ((MemberExpression)expression.Right).Expression is ParameterExpression
                && ((MemberExpression)expression.Right).Member.MemberType==MemberTypes.Property
                ;
            var altpropname = "";
            if(proptest){
                altpropname = (expression.Right as MemberExpression).Member.Name;
            }
            if(expression.NodeType== ExpressionType.Equal){
                return proptest
                           ? Restrictions.EqProperty(propname, altpropname)
                           : Restrictions.Eq(propname,Expression.Lambda( expression.Right).Compile().DynamicInvoke(null));
            }
            if (expression.NodeType == ExpressionType.NotEqual)
            {

                return Restrictions.Not(proptest
                           ? Restrictions.EqProperty(propname, altpropname)
                           : Restrictions.Eq(propname, Expression.Lambda(expression.Right).Compile().DynamicInvoke(null)));
            }

            if (expression.NodeType == ExpressionType.GreaterThan)
            {

                return proptest
                           ? Restrictions.GtProperty(propname, altpropname)
                           : Restrictions.Gt(propname, Expression.Lambda(expression.Right).Compile().DynamicInvoke(null));
            }

            if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
            {

                return proptest
                           ? Restrictions.GeProperty(propname, altpropname)
                           : Restrictions.Ge(propname, Expression.Lambda(expression.Right).Compile().DynamicInvoke(null));
            }
            if (expression.NodeType == ExpressionType.LessThan)
            {

                return proptest
                           ? Restrictions.LtProperty(propname, altpropname)
                           : Restrictions.Lt(propname, Expression.Lambda(expression.Right).Compile().DynamicInvoke(null));
            }
            if (expression.NodeType == ExpressionType.LessThanOrEqual)
            {

                return proptest
                           ? Restrictions.LeProperty(propname, altpropname)
                           : Restrictions.Le(propname, Expression.Lambda(expression.Right).Compile().DynamicInvoke(null));
            }

            throw new NotImplementedException();
        }

        private object lift(Expression expression){
            if(expression is ConstantExpression){
                return ((ConstantExpression) expression).Value;
            }
            throw new NotImplementedException();
        }
    }
    public static class FluentNHibernateQueryExtensions
    {
        [Overload]
        public static IEnumerable<T> fQuery<T>(this StorageWrapper<T> storage, Expression<Func<T, object>> first, params Expression<Func<T, object>>[] commonParameters)
        {
            return storage.Query<T, T>(first, commonParameters);
        }

         [Overload]
        public static IEnumerable<R> fQuery<T,R>(this StorageWrapper<T> storage, Expression<Func<T,object >> first, params Expression<Func<T,object >> [] commonParameters)
        {
             return new ByExpressionsNHibernateQueryExecutor<T, R>(storage, first, commonParameters).Execute();
             
        }

         [Overload]
         public static IEnumerable<T> fQuery<T>(this StorageWrapper<object> storage, Expression<Func<T, object>> first, params Expression<Func<T, object>>[] commonParameters)
         {
             return storage.Query<T, T>(first, commonParameters);
         }

         [Overload]
         public static IEnumerable<R> fQuery<T, R>(this StorageWrapper<object> storage, Expression<Func<T, object>> first, params Expression<Func<T, object>>[] commonParameters)
         {
             return new ByExpressionsNHibernateQueryExecutor<T, R>(storage, first, commonParameters).Execute();

         }

      
    }
}
