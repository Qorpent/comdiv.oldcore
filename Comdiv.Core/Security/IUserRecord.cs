using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Model.Interfaces;

namespace Comdiv.Security
{
    public interface IUserRecord: IWithName,IWithTags
    {
        string Login { get; set; }
        string Domain { get; set; }
        string Occupation { get; set; }
        string Contact { get; set; }
        string DomainId { get; set; }
        string GetFio();
    }
}
