using System;

namespace Comdiv.Test
{
    ///<summary>
    ///</summary>
    public class StopwatchResult
    {
        public long First { get; set; }
        public long Second { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return "Прирост производительности по " + Name +" "+
                   Math.Round(100-(Convert.ToDouble(First)/Convert.ToDouble(Second)*100), 0)
                   + "%";
        }
    }
}