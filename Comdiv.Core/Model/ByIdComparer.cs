using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Model.Interfaces;

namespace Comdiv.Model
{
    public class ByIdComparer<T>:IEqualityComparer<T> where T:IWithId
    {
     

        public bool Equals(T x, T y){
            return x.Id().Equals(y.Id());
        }

        public int GetHashCode(T obj){
            return obj.Id().GetHashCode();
        }
    }
}
