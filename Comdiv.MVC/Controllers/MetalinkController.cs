using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using Comdiv.Persistence;

namespace Comdiv.MVC.Controllers
{
    
    public class MetalinkController:BaseController
    {
        
        public void save(string system, string code, string srctype, string trgtype, string src, string trg, string type, string subtype, string tag, string active) {
            var mlr = new MetalinkRecord();
            if(active.noContent()) {
                active = "True";
            }
            mlr.Code = code;
            mlr.SrcType = srctype;
            mlr.TrgType = trgtype;
            mlr.Src = src;
            mlr.Trg = trg;
            mlr.Type = type;
            mlr.SubType = subtype;
            mlr.Tag = tag;
            mlr.Active = active.toBool();
            new MetalinkRepository().Save(mlr,system);
            RenderText("OK");
        }
        public void search(string system, string srctype, string trgtype, string src, string trg, string type, string subtype) {
            var mlr = new MetalinkRecord();

            mlr.SrcType = srctype;
            mlr.TrgType = trgtype;
            mlr.Src = src;
            mlr.Trg = trg;
            mlr.Type = type;
            mlr.SubType = subtype;
            mlr.Active = true;
            var result = new MetalinkRepository().Search(mlr, system);

            PropertyBag["items"] = result;

        }
    }
}
