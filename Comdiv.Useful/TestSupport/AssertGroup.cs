using System;
using System.Collections.Generic;

namespace Comdiv.Test.Extensions{
    public class AssertGroup{
        public AssertGroup Parent { get; set; }
        public Exception Error { get; set; }
        public string Name { get; set; }
        public AssertGroup(){
            Name = "noname";
        }
        protected Exception getError(){
            if(null==Error){
                return null;
            }
            return new AssertGroupException(this);
        }
        protected void getErrors(IList<Exception> exceptions){
            var error = getError();
            if(null!=error)exceptions.Insert(0,error);
            if(null!=Parent)Parent.getErrors(exceptions);
        }
        public void check(){
            var errors = new List<Exception>();
            getErrors(errors);
            if(0!=errors.Count){
                throw new AssertGroupWrapperException(errors);
            }
        }
    }
}