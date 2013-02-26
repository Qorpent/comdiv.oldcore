using System;
using System.Linq;
using System.Text;
using Comdiv.Wiki;

namespace Comdiv.Reporting
{
    public interface IWikiHelper
    {
        WikiPage get(string code);
        WikiPage[] find(string pattern);
        string render(WikiPage page);
        string render(string code);
    }
}
