using System;
using System.Xml.Linq;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.MAS {
    [Schema("mas")]
    public class App : IWithId, IWithVersion, IWithCode, IWithName
    {
        public virtual int Id { get; set; }
        public virtual DateTime Version { get; set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        [Map]
        public virtual string Comment { get; set; }
        [Map]
        public virtual AppType Type { get; set; }
        [Map]
        public virtual Server Server { get; set; }
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

        public virtual XElement CachedConfig { get; set; }


        
    }
}