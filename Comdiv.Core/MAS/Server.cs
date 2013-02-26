using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.MAS {
    [Schema("mas")]
    public class Server:IWithId,IWithVersion,IWithCode,IWithName {
        public Server() {
            this.Code = Guid.NewGuid().ToString();
            this.Name = "";
            this.Comment = "";
            this.LNetUrl = "";
            this.INetUrl = "";
            this.Config = "";
        }
        public virtual int Id { get; set; }
        public virtual DateTime Version { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        [Map]
        public virtual string Comment { get; set; }
        [Map]
        public virtual string LNetUrl { get; set; }
        [Map]
        public virtual string INetUrl { get; set; }
        private string _config;

        [Map]
        public virtual string Config
        {
            get { return _config; }
            set
            {
                if (value != _config)
                {
                    CachedConfig = null;
                }
                _config = value;
            }
        }

        [Map("Server")]
        public virtual IList<App> Applications { get; set; }

        public virtual XElement CachedConfig { get; set; }
    }
}