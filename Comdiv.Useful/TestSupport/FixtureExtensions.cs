using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.Contracts;
using NUnit.Framework;

namespace Comdiv.Test.Extensions{

    public static class AssertExtensions{
        public static bool isTrue(this bool test){
           Assert.IsTrue(test);
            return test;
        }
        public static T isNotNull<T>(this T obj){
            Assert.NotNull(obj);
            return obj;
        }
        public static T isNull<T>(this T obj)
        {
            Assert.Null(obj);
            return obj;
        }
        public static T isEqual<T>(this T obj, object test)
        {
            Assert.AreEqual(obj,test);
            return obj;
        }
        public static T countIs<T>(this T obj, int test) where T:IEnumerable{
            Assert.AreEqual(obj.Cast<Object>().Count(), test);
            return obj;
        }
    }

    public static class FixtureExtensions{
        /// <summary>
        /// ensure marks prerequsition part of test - fails in them is mark of framework, not functional problems
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="execute"></param>
        public static IExtendedFixture ensure(this IExtendedFixture fixture,Action execute){
            return ensure(fixture, "noname", execute);
        }

        /// <summary>
        /// ensure marks prerequsition part of test - fails in them is mark of framework, not functional problems
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="execute"></param>
        public static IExtendedFixture ensure(this IExtendedFixture fixture, string comment,Action execute)
        {
            try{
                execute();
            }catch(Exception exception)
            {
                throw new Exception("was fail in ENSURE part of test - "+comment,exception);
            }
            return fixture;
        }

        public static AssertGroup test(this IExtendedFixture fixture,Action execute){
            return test(fixture, "noname", execute);
        }

        public static AssertGroup test(this IExtendedFixture fixture,string name,Action execute){
            return (null as AssertGroup).test(name, execute);
        }

        public static AssertGroup test(this AssertGroup group, string name, Action execute){
            Console.Write(name);
            var result = new AssertGroup { Name = name, Parent =  group};
            try
            {
                execute();
                Console.WriteLine("... Œ ");
            }
            catch (Exception ex)
            {
                result.Error = ex;
                Console.WriteLine("... Error");
            }
            return result;
        }
    }
}