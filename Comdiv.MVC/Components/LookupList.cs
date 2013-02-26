#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

#endregion

namespace Comdiv.Web{

    #region

    #endregion

    #region

    #endregion

    public class LookupListComponent : ViewComponent{
        private string advancedAttributes = string.Empty;
        private string id;
        private IEnumerable items;
        private string name;
        private string nullText;
        private string nullValue = "null";
        private string selectedValue;
        private bool useNullValue = true;

        public IEnumerable Items{
            get { return items ?? new object[]{}; }
            set { items = value; }
        }

        public string SelectedValue{
            get { return selectedValue ?? "null"; }
            set { selectedValue = value; }
        }

        public string NullValue{
            get { return nullValue ?? "null"; }
            set { nullValue = value; }
        }

        public string NullText{
            get { return nullText ?? "--"; }
            set { nullText = value; }
        }

        public bool UseNullValue{
            get { return useNullValue; }
            set { useNullValue = value; }
        }

        public string AdvancedAttributes{
            get { return advancedAttributes; }
            set { advancedAttributes = value; }
        }

        public string Id{
            get { return id ?? Name; }
            set { id = value; }
        }

        public string Name{
            get { return name ?? "lookup"; }
            set { name = value; }
        }

        public override void Initialize(){
            if (ComponentParams.Contains("nullValue")){
                NullValue = (string) ComponentParams["nullValue"];
            }
            if (ComponentParams.Contains("nullValue")){
                NullText = (string) ComponentParams["nullText"];
            }
            if (ComponentParams.Contains("useNullValue")){
                UseNullValue = (bool) ComponentParams["useNullValue"];
            }
            if (ComponentParams.Contains("selectedValue")){
                SelectedValue = (string) ComponentParams["selectedValue"];
            }
            if (ComponentParams.Contains("attr")){
                AdvancedAttributes = (string) ComponentParams["attr"];
            }
            if (ComponentParams.Contains("items")){
                Items = PrepareItems((IEnumerable) ComponentParams["items"]);
            }
            if (ComponentParams.Contains("id")){
                Id = (string) ComponentParams["id"];
            }
            if (ComponentParams.Contains("name")){
                Name = (string) ComponentParams["name"];
            }

            base.Initialize();
        }

        private IEnumerable PrepareItems(IEnumerable objects){
            var result = new List<object>();
            if (UseNullValue){
                result.Add(new{Id = NullValue, Name = NullText});
            }
            foreach (var o in objects){
                result.Add(o);
            }
            return result;
        }

        public override void Render(){
            PropertyBag["fullitems"] = Items;
            PropertyBag["id"] = Id;
            PropertyBag["name"] = Name;
            PropertyBag["Is"] = new SelectedHelper(this);
            PropertyBag["attr"] = AdvancedAttributes;
            base.Render();
        }

        #region Nested type: SelectedHelper

        public class SelectedHelper{
            private readonly LookupListComponent list;

            public SelectedHelper(LookupListComponent list){
                this.list = list;
            }

            public string Sel(object val){
                if (val.ToString() == list.SelectedValue){
                    return " selected = 'selected' ";
                }
                return string.Empty;
            }
        }

        #endregion
    }
}