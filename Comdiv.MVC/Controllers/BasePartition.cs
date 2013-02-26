using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Brail;
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

namespace Comdiv.MVC.Controllers{
    public class BasePartition : IPartition{
        private readonly object sync = new object();
        private IInversionContainer _container;

        public IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (this){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        public BasePartition(){
            ControllerName = "";
            ActionName = "";
            Roles = "";
            Users = "";
            Role = "";
            PropertyBagPrefix = "";
            CollapseEffect = "blind";
            Help = "";
        }

        public string Help { get; set; }


        public string Class { get; set; }

        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string Roles { get; set; }
        public string Users { get; set; }

        public string ViewName { get; set; }

        public IBrailRender RenderObject { get; set; }

        public object this[string key]{
            get{
                lock (sync){
                    return Controller.PropertyBag[PropertyBagPrefix + key];
                }
            }
            set{
                lock (sync){
                    Controller.PropertyBag[PropertyBagPrefix + key] = value;
                }
            }
        }

        public string PropertyBagPrefix { get; set; }
        public Controller Controller { get; set; }
        public string Title { get; set; }
        public bool UseCollapseBlock { get; set; }
        public bool CollapsedAtStart { get; set; }
        public string CollapseEffect { get; set; }
        public string Id { get; set; }

        #region IPartition Members

        public virtual bool IsMatch(IMvcContext context){
            if (ControllerName.hasContent() && !ControllerName.ToUpper().Equals(context.Name.ToUpper())){
                return false;
            }
            if (ActionName.hasContent() && !ActionName.ToUpper().Equals(context.Action.ToUpper())){
                return false;
            }

            if (Roles.hasContent()){
                var match = false;
                foreach (var role in Roles.split()){
                    if (myapp.roles.IsInRole(context.User, role, false)){
                        match = true;
                        break;
                    }
                }
                if (!match){
                    return false;
                }
            }
            if (Users.hasContent()){
                if (!Users.split().Contains(myapp.usrName)){
                    return false;
                }
            }
            return true;
        }

        public string Role { get; set; }

        public int Idx { get; set; }

        public void Execute(object controller){
            lock (sync){
                Controller = (Controller) controller;

                InternalExecute();
            }
        }

        public void Render(object view){
            if (!(view is BrailBase)){
                throw new NotSupportedException("Поддерживаются только Brail - view");
            }
            Render((BrailBase) view);
        }

        #endregion

        public BasePartition SetHelp(string help){
            Help = help;
            return this;
        }

        public BasePartition For(string controller){
            return For(controller, null);
        }

        public BasePartition For(string controller, string action){
            ControllerName = controller ?? ControllerName;
            ActionName = action ?? ActionName;
            return this;
        }

        public BasePartition Collapse(string title){
            return Collapse(title, false);
        }

        public BasePartition Collapse(string title, bool closed){
            return Collapse(title, closed, "blind");
        }

        public BasePartition Collapse(string title, bool closed, string effect){
            UseCollapseBlock = true;
            Title = title;
            CollapsedAtStart = closed;
            CollapseEffect = effect;
            Id = Id ?? title.toSystemName().toShort();
            return this;
        }

        public BasePartition SetIdx(int idx){
            Idx = idx;
            return this;
        }

        public BasePartition SetRoles(string roles){
            Roles = roles;
            return this;
        }

        public BasePartition SetUsers(string users){
            Users = users;
            return this;
        }

        public BasePartition SetView(string view){
            ViewName = view;
            return this;
        }

        protected virtual void InternalExecute() {}

        protected void Render(BrailBase view){
            lock (sync){
                if (UseCollapseBlock){
                    view.OutputSubView("partStart",
                                       new Dictionary<string, object>
                                       {{"this", this}, {"wiki", myapp.files.ReadXml("route.xml?$wiki")}});
                }
                if (ViewName.hasContent()){
                    view.OutputSubView(ViewName);
                }
                else if (null != RenderObject){
                    RenderObject.Render(view);
                }
                else{
                    InternalRender(view);
                }
                if (UseCollapseBlock){
                    view.OutputSubView("partEnd", new Dictionary<string, object>{{"this", this}});
                }
            }
        }


        protected virtual void InternalRender(BrailBase view) {}

        public BasePartition SetClass(string cls){
            Class = cls;
            return this;
        }
    }
}