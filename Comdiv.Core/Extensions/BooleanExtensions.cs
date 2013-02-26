// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Comdiv.Extensions{

    [Serializable]
    public class AssertException : Exception{
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AssertException() {}
        public AssertException(string message) : base(message) {}
        public AssertException(string message, Exception inner) : base(message, inner) {}

        protected AssertException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }



    public static class AssertExtensions{
        public static T isnotnull<T>(this T obj){
            isnotnull(obj, true);
            return obj;
        }

        public static bool isnotnull(this object obj, bool throws){
            return isnotnull(obj, throws, "");
        }

        public static bool isnotnull(this object obj, bool throws, string message){
            if(null ==  obj){
                if(throws){
                    message = "notnull exception " + (message ?? "");        
                    throw new AssertException(message);
                }else{
                    return false;
                }
            }
            return true;
            
            
        }
    }


    /// <summary>
    /// provides methods to quick bool checking for most known
    /// types
    /// </summary>
    public static class BooleanExtensions{
        /// <summary>
        /// returns true for not-null, non-empty (including logicall nulls)
        /// objects , to avoid using many null!=, IsNullOrEmpty and so on
        /// </summary>
        /// <param name="obj">object to test</param>
        /// <returns>true - object is not null, logical null, empty (for strings and enumerables),
        /// zero for numerics and so on</returns>
        /// <remarks>have ability to check IConvertible  interafece, supports nullables 
        /// </remarks>
        public static bool yes(this object obj){
            //nulls are treated as false
            if (null == obj){
                return false;
            }
            //bools are bools
            if (obj is bool){
                return (bool) obj;
            }
            // strings are tested to be non-empty
            if (obj is string){
                return !String.IsNullOrEmpty((string) obj);
            }
            //dates are compared with DateExtensions to match Begin-End range
            if (obj is DateTime){
                return !((DateTime) obj).isNull();
            }
            //all value types are checked to be non-zero, that is for enums too
            if (obj is ValueType){
                return 0 != Convert.ToDouble(obj);
            }
            //enumerables are checked to be non-empty
            if (obj is IEnumerable){
                IEnumerable<object> totest = ((IEnumerable) obj).Cast<object>();
                if (totest.Count() == 0){
                    return false;
                }
                return true;
            }
            // IConvertible is the way to provide custom effective boolean
            if (obj is IConvertible){
                //we can still proceed it ToBoolean is not supported or not implemented
                try{
                    return ((IConvertible) obj).ToBoolean(CultureInfo.InvariantCulture);
                }
                catch (NotSupportedException){}
                catch (NotImplementedException){}
            }
            // by default, not-null objects are treated as true
            return true;
        }


        /// <summary>
        /// anti-yes
        /// </summary>
        /// <seealso cref="yes"/>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool no(this object obj){
            return !obj.yes();
        }
    }
}