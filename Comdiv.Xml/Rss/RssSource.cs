using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rss{
    public class RssSource : IRssSource
    {
        public virtual bool IsRetranslator{
            get { return false;}
        }

        public virtual string GetRaw(){
            return new DefaultRssWriter().ToString(this);
        }

        public IRssChannel Channel { get; set; }


        public IList<IRssItem> Items { get; set; }

        public RssSource()
        {
            Items = new List<IRssItem>();
            Channel = new RssChannel();
        }

    }
}