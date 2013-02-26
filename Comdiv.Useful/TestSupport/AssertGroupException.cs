using System;
using System.Runtime.Serialization;

namespace Comdiv.Test.Extensions{
    public class AssertGroupException:Exception{
        public AssertGroupException() {}

        public AssertGroupException(AssertGroup group):base("error in \" "+group.Name+"\"",group.Error){
            
            
        }

        public AssertGroupException(string message) : base(message) {}

        public AssertGroupException(string message, Exception innerException) : base(message, innerException) {}

        protected AssertGroupException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        public override string ToString()
        {
            return "\r\n===================\r\n" + this.Message+"\r\n" + InnerException.ToString() + "\r\n===================";
        }
    }
}