using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.MVC;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.Caching{
    public class SlidingLease : AbstractCacheLease{
        public SlidingLease(){
            LastAccessTime = DateTime.Now;
            LeaseTime = TimeSpan.FromMinutes(10);
        }

        public DateTime LastAccessTime { get; set; }
        public TimeSpan LeaseTime { get; set; }


        public override bool IsValid{
            get { return (LastAccessTime + LeaseTime) > DateTime.Now; }
        }

        public override void Retrieved(){
            LastAccessTime = DateTime.Now;
        }

        public override string ToString(){
            var rest = LastAccessTime + LeaseTime - DateTime.Now;
            return string.Format("BY {0}, LAST {1}, INV {2}:{3}", LeaseTime.Minutes, LastAccessTime.ToString("hh:mm"),
                                 rest.Minutes, rest.Seconds);
        }
    }
}