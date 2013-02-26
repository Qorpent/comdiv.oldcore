using System;
using System.Collections.Generic;
using Comdiv.Extensions;

namespace Comdiv.Persistence {
    public class MetalinkRecord:IParametersProvider {
        public MetalinkRecord() {
            Active = true;
            Start = DateExtensions.Begin;
            Finish = DateExtensions.End;
        }
        public int Id { get; set; }
        [Param]
        public string Code { get; set; }
        [Param]
        public string SrcType { get; set; }
        [Param]
        public string TrgType { get; set; }
        [Param]
        public string Src { get; set; }
        [Param]
        public string Trg { get; set; }
        [Param]
        public string Type { get; set; }
        [Param]
        public string SubType { get; set; }
        [Param]
        public string Tag { get; set;}
        [Param]
        public bool Active { get; set; }

        [Param]
        public DateTime Start { get; set; }

        [Param]
        public DateTime Finish { get; set; }

        public bool UseCustomMapping {
            get { return false; }
        }

        public IDictionary<string, object> GetParameters() {
            throw new NotImplementedException();
        }
    }
}