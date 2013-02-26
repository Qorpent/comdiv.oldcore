using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.FluentView
{
    public class jsref
    {
        private string js;
        private string txt;
        private string img;
        public jsref(string js,string txt) : this(js, txt, "") {}

        public jsref(string js,string txt,string img){
            this.js = js;
            this.txt = txt;
            this.img = img;
        }
        public override string ToString()
        {
            var inner = txt;
            if(img.hasContent()){
                var path = myapp.files.ResolveAsLocal("content/image/"+img + ".png");
                
                if(null!=HttpContext.Current){
                    path = HttpContext.Current.Request.ApplicationPath +"/"+ path;
                    path = path.Replace("//", "/");
                }

                inner = string.Format("<img src='{0}' alt='{1}' title='{1}'/>", path, txt);
            }
            return string.Format("<a href='#' onclick='" + js.Replace("'","&apos;") + ";return false;'>" + inner + "</a>");
        }
    }
}
