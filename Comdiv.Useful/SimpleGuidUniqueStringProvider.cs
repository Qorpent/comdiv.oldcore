using System;
using System.Collections.Generic;
using System.Linq;
using Comdiv.Useful;

namespace Comdiv.Useful{
    public class SimpleGuidUniqueStringProvider : IUniqueStringProvider{
        public string New(){
            return Guid.NewGuid().ToString();
        }

        public void Lock(IEnumerable<string> existed){
            
        }

        public void Release(IEnumerable<string> notexisted){
            
        }

        public void Release(string code){
            
        }
    }
}