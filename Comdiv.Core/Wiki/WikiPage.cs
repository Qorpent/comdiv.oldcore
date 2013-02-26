using System;
using System.Collections.Generic;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Wiki {
    public class WikiPage {
        public WikiPage() {
            Properties = new Dictionary<string, string>();
        } 
        public string Code { get; set; }
        public FileLevel Level { get; set; }
        public string Content { get; set; }
        public string RenderedContent { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string FilePath { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public string Title {
            get { return Properties.get("title", ""); }
            set { Properties["title"] = value; }
        }
    }
}