using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MonoRail.Framework;
using Comdiv.Application;
using Comdiv.Authorization;
using Comdiv.Extensions;

namespace Comdiv.Controllers {
    [Filter(ExecuteWhen.BeforeAction, typeof (AuthorizeFilter), ExecutionOrder = -100)]
    [Filter(ExecuteWhen.BeforeAction, typeof(OutputSelectionFilter))]
    [Rescue(typeof (RescueController), "showerror", typeof (ControllerAuthorizationException))]
    public abstract class BaseController : SmartDispatcherController {
        protected ControllerResourceHelper me;
        public string ViewRoot { get; set; }
        protected string ViewPrefix  { get; set; }

        protected virtual void WriteToProfile(string code, string content)
        {
            IProfileRepository profile = myapp.getProfile();
            try {
                profile.Save(code, content);
            }
            finally {
                myapp.release(profile);
            }
        }

        public string GetParam(string name,string def) {
            if(Params.AllKeys.Contains(name)) {
                return Params[name];
            }
            return def;
        }

        protected virtual string ReadFromProfile(string code) {
            IProfileRepository profile = myapp.getProfile();
            try {
                return profile.Load(code);
            }
            finally {
                myapp.release(profile);
            }
        }

        public IDictionary<string , string > ConvertPrefixedParametersToDict(string prefix)
        {
            var result = new Dictionary<string, string>();
            foreach (string key in this.Params.Keys)
            {
                if(key.StartsWith(prefix))
                {
                    result[key.Substring(prefix.Length)] = this.Params[key];
                }
            }
            return result;
        }

        protected virtual string[] GetProfileFiles(string mask, string format = "{PATH}{NAME}{EXT}") {
            format = format ?? "{PATH}{NAME}{EXT}";
            var rootdir = myapp.files.Resolve("~/profile/" + myapp.usrName.Replace("\\", "_") + "/", false);
            var maskdir = Path.GetDirectoryName(mask);
            if(maskdir.hasContent()&&maskdir!="/") {
                rootdir = Path.Combine(rootdir, maskdir);
            }
            Directory.CreateDirectory(rootdir);
           
            return Directory.GetFiles(rootdir, Path.GetFileName(mask)).OrderBy(x => x).Select(x =>{
                                                                                                 return format.Replace(
                                                                                                      "{PATH}",
                                                                                                      Path.
                                                                                                          GetDirectoryName
                                                                                                          (x) + "\\").
                                                                                                      Replace("{NAME}",
                                                                                                              Path.
                                                                                                                  GetFileNameWithoutExtension
                                                                                                                  (x)).
                                                                                                      Replace("{EXT}",
                                                                                                              Path.
                                                                                                                  GetExtension
                                                                                                                  (x));
                                                                                              }).ToArray();
        }

        protected virtual void DeleteProfileFiles(string mask) {
            var files = GetProfileFiles(mask);
            foreach (var file in files) {
                File.Delete(file);
            }
        }

        public override void Contextualize(IEngineContext engineContext, IControllerContext context) {
            this.me = new ControllerResourceHelper();
            me.SetController(this,context);
            me.SetContext(engineContext);
            base.Contextualize(engineContext, context);
            if(ViewRoot.hasContent()) {
                this.SelectedViewName = this.SelectedViewName.ToLower().Replace(this.Name + "\\", ViewRoot);
            }
            PropertyBag["me"] = me;
            PropertyBag["mycss"] = me.getCss();
            PropertyBag["myjs"] = me.getJs();
            PropertyBag["myname"] = context.Name.ToLower();
            if(this.Params["ajax"].noContent()) {
                try {
                    PropertyBag["widgets"] = myapp.widgets.GetMyWidgets();
                }catch(Exception ex) {
                    
                }
            }
        }

        public override void PreSendView(object view)
        {
            base.PreSendView(view);

        }
    }
}