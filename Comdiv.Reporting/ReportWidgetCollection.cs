using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using Comdiv.Extensions;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Logging;
using Comdiv.Application;
using Comdiv.Persistence;
using Comdiv.Inversion;
using Comdiv.Security.Acl;
using Comdiv.Conversations;
using Comdiv.IO;
using Comdiv.Security;
using System.Linq;

namespace Comdiv.Reporting{
    public class ReportWidgetCollection: Dictionary<string , ReportWidget>{

        public ReportDefinitionBase Definition
        {
            get;
            set;
        }

        public IList<ReportWidget> Preparators{
            get{
                return this.Values.Where(x => x.OnPrepare).ToList();
            }
        }

        public bool IsForRender(ReportWidget widget, string zone){
            if (!widget.OnRender) return false;
            if (!(widget.Zone == zone)) return false;
            if(widget.Role.hasContent()){
                if(!(myapp.roles.IsAdmin() || myapp.roles.IsInRole(widget.Role))){
                    return false;
                }
            }
            if(widget.Condition.hasContent()){
                return Definition.IsConditionMatch(widget.Condition);
            }
            return true;
        }

        public IList<ReportWidget> ForZone(string zone){
            return this.Values.Where(x => x.OnRender && x.Zone == zone ).ToList();
        }

        public void ReadFromXml(XElement element){
            var widgetxml = element.XPathSelectElements("./show").Union(element.XPathSelectElements("./exec")).ToArray();
            foreach (var x in widgetxml){
                var widget = new ReportWidget();
               
                x.update(widget);
                widget.SrcXml = x.ToString();
                if(x.Name.LocalName=="show"){
                    widget.OnRender = true;
                }
                else{
                    widget.OnPrepare = true;
                }
                if (widget.View.noContent() && widget.Text.noContent()){
                    widget.View = widget.Code;
                }
                if(widget.Zone.noContent()){
                    widget.Zone = x.attr("at");
                }
                if(widget.Zone.noContent()){
                    widget.Zone = "content";
                }
                
                
                if(widget.View.hasContent() && !widget.View.StartsWith("/")){
                    widget.View = "/widgets/" + widget.View;
                }
                foreach (var xparam in x.XPathSelectElements("./param")){
                    widget.Parameters[xparam.attr("code")] = xparam.Value;
                }
                this[widget.Code] = widget;
            }
        }
    }
}