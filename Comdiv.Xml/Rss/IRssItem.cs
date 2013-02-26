using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rss{
    public interface IRssItem
    {
        string Title { get; }
        string Link { get; }
        string Description { get; }
        IDictionary<string, string> Categories { get; }
        string Guid { get; }
        DateTime PubDate { get; }
        string Source { get; }
    }
}