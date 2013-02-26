using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rss{
    public class RssItem : IRssItem
    {
        public string Title { get; set; }

        public string Link { get; set; }

        public string Description { get; set; }

        public IDictionary<string, string> Categories { get; private set; }

        public string Guid { get; set; }

        public DateTime PubDate { get; set; }

        public string Source { get; set; }
        public RssItem()
        {
            Categories = new Dictionary<string, string>();
        }
    }
}