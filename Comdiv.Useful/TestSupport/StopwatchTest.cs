using System;
using System.Diagnostics;
using NUnit.Framework;


namespace Comdiv.Test
{
    ///<summary>
    ///</summary>
    public class StopwatchTest
    {
        private Action first;
        private Action second;
 



        public StopwatchTest(Action first,Action second)
        {
            this.first = first;
            this.second = second;

        }

        public StopwatchResult Run(string name)
        {
            return Run(name,10000);
        }

        public StopwatchResult Run(string name,int count)
        {
            var x = Stopwatch.StartNew();
            var result = new StopwatchResult();
            result.Name = name;
            x.Start();
            for(int i = 0;i<count;i++)
            {
                first();
            }
            x.Stop();
            result.First = x.ElapsedTicks;
            x.Reset();
            x.Start();
            for (int i = 0; i < count; i++)
            {
                second();
            }
            x.Stop();
            result.Second = x.ElapsedTicks;
            result.First.Should().Be.LessThan(result.Second);
            Console.WriteLine(result.ToString());
            return result;
        }
    }
}