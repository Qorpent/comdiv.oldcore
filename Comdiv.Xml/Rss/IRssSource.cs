using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rss{
    public interface IRssSource
    {
        bool IsRetranslator
        {
            get;
        }

        string GetRaw();
        IRssChannel Channel { get; }
        IList<IRssItem> Items { get; }
    }
}