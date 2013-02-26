using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Comdiv.Extensions;
using Comdiv.IO;
using Comdiv.Persistence;
using Comdiv.Xml.Smart;

namespace Comdiv.Controllers {
    public class HqlColumnLoader {
        public IDictionary<string ,HqlColumn> GetColumns(string typename, string view) {
            var cols = new Dictionary<string, HqlColumn>();
            var l = new BxlApplicationXmlReader();
            l.TotalSearch = true;
            var x = l.Read("hqlcolumns.bxl");
            var s = new SmartXml(x);
            
            var elements = x.Elements("col").OrderBy(z => z, new HqlColumnSqlComparer()).ToArray();
            foreach (var e in elements) {
                e.Remove();
            }
            foreach (var e in elements) {
                s.Element.Add(e);
                var tf = e.attr("types");
                if (tf != "" && !tf.ToLower().Contains("/" + typename.ToLower() + "/")) {
                    e.Remove();
                    continue;
                }
                var vn = e.attr("views");
                if (e.attr("idx").noContent())
                {
                    e.SetAttributeValue("idx", 50);
                }
                if (vn != "" && !vn.Contains("/" + view + "/")) {
                    e.SetAttributeValue("hidden", "true");
                }
            }
            s.RemoveDuplicates();
            s.BindTo(cols);
        	foreach (var col in cols) {
        		refineColumn(col.Value);
        	}
            return cols;
        }

    	private void refineColumn(HqlColumn col) {
    		if(col.LookupType.hasContent()) {
				if (col.View.noContent()) col.View = "/hql/lookup";
    		}
			if(col.Dictionary.hasContent()) {
				if (col.View.noContent()) col.View = "/hql/dictionary";
			}
    	}
    }

    public class HqlColumnSqlComparer : IComparer<XElement> {
        public int Compare(XElement x, XElement y) {
            if (x.attr("id") != y.attr("id")) return 0;
            if (x.attr("types").noContent() && y.attr("types").hasContent()) return -1;
            if (x.attr("views").noContent() && y.attr("views").hasContent()) return -1;
            return 0;
        }
    }
}