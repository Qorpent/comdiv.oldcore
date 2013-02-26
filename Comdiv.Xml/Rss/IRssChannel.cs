using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rss{
    public interface IRssChannel
    {
        string Title { get; set; }
        string Link { get; set; }
        string Description { get; set; }
        DateTime PubDate { get; set; }
        DateTime LastBuildDate { get; set; }
        string Generator { get; set; }
    }
}