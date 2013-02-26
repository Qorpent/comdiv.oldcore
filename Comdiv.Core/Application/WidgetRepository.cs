using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Xml;
using Comdiv.Extensions;
using Comdiv.IO;

namespace Comdiv.Application {
    public class WidgetRepository : IWidgetRepository {

        private WidgetRepository parent;

        public WidgetRepository GetInstance() {
            var result = new WidgetRepository{parent = this};
            myapp.OnReload -= result.myapp_OnReload;
            return result;
        }

        public WidgetRepository() {
            myapp.OnReload += myapp_OnReload;
        }

        private Widget[] widgets;

        void myapp_OnReload(object sender, Common.EventWithDataArgs<int> args) {
            widgets = null;
        }

        public Widget[] GetAllWidgets() {
            lock (this) {
                if(this.parent!=null) {
                    return parent.GetAllWidgets();
                }
                if (widgets == null) {
                    reloadwidgets();
                }
                return widgets;
            }
        }
        public WidgetCollection GetMyWidgets()
        {
            lock (this) {
                return new WidgetCollection(GetMyWidgets(myapp.usr, HttpContext.Current.Request.Url.ToString())
                                                .GroupBy(x => x.Position.ToLower())
                                                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Idx).ToArray()));
            }
        }


        public IEnumerable<Widget> GetMyWidgets(IPrincipal usr, string url)
        {
            lock (this) {
                var result = new List<Widget>();
                foreach (var widget in GetAllWidgets())
                {
                    if (widget.Roles.no() && widget.UrlRegex.noContent()) {
                        result.Add(widget);
                        continue;
                        ;
                    }
                    bool proceed = true;
                    if (widget.Roles.yes())
                    {
                        proceed = false;
                        foreach (var role in widget.Roles)
                        {
                            if (myapp.roles.IsInRole(usr, role))
                            {
                                proceed = true;
                                break;

                            }

                        }
                    }
                    if (proceed)
                    {
                        if (widget.UrlRegex.hasContent())
                        {
                            if (!url.like(widget.UrlRegex))
                            {
                                proceed = false;
                            }
                        }
                    }
                    if (proceed) {
                        result.Add(widget);
                    }

                }
                return result;
            }
        }

        public ApplicationXmlReader XmlReader { get; set; }

        private void reloadwidgets() {
            var xml = XmlReader.Read("widgets.xml");
            IList<Widget> result = new List<Widget>();
            foreach (var element in xml.Elements()) {
                var w = new Widget();
                w.Read(element);
                result.Add(w);
            }
            widgets = result.ToArray();
        }
    }
}