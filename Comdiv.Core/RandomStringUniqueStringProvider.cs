using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Useful{
    public class RandomStringUniqueStringProvider:AbstractUniqueStringProvider{
        public int Size { get; set; }
        public int StartCharIndex { get; set; }
        public int EndCharIndex { get; set; }
        protected Random random = new Random(Environment.TickCount);

        protected override string InternalNew(string currentResult){
            var result = "";
            for (int i=0;i<Size;i++){
                result += nextChar();
            }
            return result;
        }

        private char nextChar() {
            return (char)random.Next(StartCharIndex,EndCharIndex);
        }

        public RandomStringUniqueStringProvider() {
            Size = 2;
            StartCharIndex =  'a';
            EndCharIndex = 'z';
        }
    }
}