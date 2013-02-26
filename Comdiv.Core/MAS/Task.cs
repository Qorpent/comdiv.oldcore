using System;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.MAS
{
    ///<summary>
    ///</summary>
    public class Task : IWithId, IWithVersion,IWithCode,IWithName {
        public Task() {
            this.Code = Guid.NewGuid().ToString();
            this.Name = this.Code;
        }

        public int Id { get; set; }

        public DateTime Version { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        [Map]
        public bool Accepted { get; set; }


    }
}