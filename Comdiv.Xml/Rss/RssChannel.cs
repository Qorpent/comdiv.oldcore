using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rss{
    public class RssChannel : IRssChannel
    {
        public string Title { get; set; }

        public string Link { get; set; }

        public string Description { get; set; }

        public DateTime PubDate { get; set; }

        public DateTime LastBuildDate { get; set; }

        public string Generator { get; set; }
    }
}