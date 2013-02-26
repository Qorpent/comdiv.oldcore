using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.MAS {
    [Schema("mas")]
    public class AppType : IWithId, IWithVersion, IWithCode, IWithName
    {
        public AppType() {
            Name = "";
            Comment = "";
            Commands = "";
            Config = "";
            
        }
        public virtual int Id { get; set; }
        public virtual DateTime Version { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        [Map]
        public virtual string Comment { get; set; }
        [Map]
        public virtual string Commands { get; set; }

        private string _config;

        [Map]
        public virtual string Config {
            get { return _config; }
            set {
                if(value!=_config) {
                    CachedConfig = null;
                }
                _config = value;
            }
        }

        [Map("Type")]
        public virtual IList<App> Applications { get; set; }
        [Map("Parent",NotNull=false)]
        public virtual AppType Parent { get; set; }
        [Map("Parent")]
        public virtual IList<AppType> Children { get; set; }

        public virtual XElement CachedConfig { get; set; }

        public virtual XElement CachedCommands { get; set; }
    }
}