using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Application {
    public class WidgetCollection : Dictionary<string , Widget[]> {

        public WidgetCollection():base() {
        }
        public WidgetCollection(IDictionary<string ,Widget[]> dict):base(dict) {
           
        }

        public Widget[] GetTabs() {
            return GetTabs("toolbar");
        }

        public Widget[] GetTabs(string  toolbar) {
            if(this.ContainsKey(toolbar.ToLower())) {
                var toolbars = this[toolbar.ToLower()].Where(x => x.Type == WidgetType.Tab);
                var uniques = toolbars.Select(x => x.Code).Distinct();
                var uniquetoolbars = uniques.Select(x => toolbars.FirstOrDefault(t => t.Code == x)).OrderBy(
                    x => x.Idx).ToList();

                var missedtoolbars =
                    GetButtons(toolbar).Select(x => x.Tab).Distinct().Where(
                        x => uniquetoolbars.FirstOrDefault(t => t.Code == x) == null);

                foreach (var missedtoolbar in missedtoolbars) {
                    uniquetoolbars.Add(new Widget{
                                                     Type = WidgetType.Tab,
                                                     Code = missedtoolbar,
                                                     Name = missedtoolbar,
                                                     Position = toolbar,
                                                 });
                }

                return uniquetoolbars.ToArray();
            }
            return new Widget[]{};
        }

        public Widget[] GetButtons(Widget tab)
        {
            
            return this[tab.Position.ToLower()].Where(x => x.Type == WidgetType.Button && x.Tab==tab.Code).OrderBy(x => x.Idx).ToArray();
            
        }

        public Widget[] GetButtons() {
            return GetButtons("toolbar");
        }

        public Widget[] GetButtons(string position)
        {
            if (this.ContainsKey(position.ToLower()))
            {
                var buttons = this[position.ToLower()].Where(x => x.Type == WidgetType.Button).OrderBy(x=>x.Idx).ToArray();
                
                return buttons;
            }
            return new Widget[] { };
        }

    }
}