using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using Castle.MonoRail.Framework;
using Castle.Windsor;
using Comdiv.Application;
using Comdiv.Caching;
using Comdiv.Controllers;
using Comdiv.Conversations;
using Comdiv.Extensibility;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.MVC.Core;
using Comdiv.Persistence;
using Qorpent.Log;
using Qorpent.Mvc;

namespace Comdiv.MVC.Controllers{
    [Helper(typeof (ConfigurationHelper), "cfg")]
    [Helper(typeof (ResourceHelper), "resx")]
    [Helper(typeof (SecurityHelper), "security")]
    [Helper(typeof (InversionHelper), "inverse")]
    public abstract class BaseController : SmartDispatcherController,IWithContainer, IWithScriptExtensions, IWithConversationController{

        public BaseController(){
            this.Db = new StorageWrapper(myapp.storage.GetAllStorages().OfType<HibernateStorage>().FirstOrDefault());
			this.QorpentLog = myapp.QorpentApplication.LogManager.GetLog(this.GetType().FullName + ";comdiv.mvc;MvcHandler", this);
        }

	    protected IUserLog QorpentLog { get; set; }

	  

		public override void PostSendView(object view)
		{
			base.PostSendView(view);
			this.PropertyBag.Clear();
			((WindsorContainer)((WindsoreInversionContainer)this.Container).Container).Kernel.ReleasePolicy.Release(this);
			((WindsorContainer)((WindsoreInversionContainer)this.Container).Container).Kernel.ReleasePolicy.Release(this.Context);
			((WindsorContainer)((WindsoreInversionContainer)this.Container).Container).Kernel.ReleasePolicy.Release(this.ControllerContext);
		}

        protected ControllerResourceHelper me;

        protected virtual void WriteToProfile(string code, string content)
        {
            IProfileRepository profile = myapp.getProfile();
            try
            {
                profile.Save(code, content);
            }
            finally
            {
                myapp.release(profile);
            }
            
        }

        protected virtual string ReadFromProfile(string code)
        {
            IProfileRepository profile = myapp.getProfile();
            try
            {
                return profile.Load(code);
            }
            finally
            {
                myapp.release(profile);
            }
        }


        public object this[string name]{
            get{
                return PropertyBag[name];
            }
            set{
                PropertyBag[name] = value;
            }
        }

        protected virtual void DeleteProfileFiles(string mask)
        {
            var files = GetProfileFiles(mask);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }


        protected virtual string[] GetProfileFiles(string mask, string format = "{PATH}{NAME}{EXT}")
        {
            var rootdir = CreateRootDir(mask, ref format);

            return Directory.GetFiles(rootdir, Path.GetFileName(mask)).OrderBy(x => x).Select(x =>
            {
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

        private static string CreateRootDir(string mask, ref string format)
        {
            format = format ?? "{PATH}{NAME}{EXT}";
            var rootdir = myapp.files.Resolve("~/profile/" + myapp.usrName.Replace("\\", "_") + "/", false);
            var maskdir = Path.GetDirectoryName(mask);
            if (maskdir.hasContent() && maskdir != "/")
            {
                rootdir = Path.Combine(rootdir, maskdir);
            }
            Directory.CreateDirectory(rootdir);
            return rootdir;
        }


        public IDictionary<string, string> ConvertPrefixedParametersToDict(string prefix)
        {
            var result = new Dictionary<string, string>();
            foreach (string key in this.Params.Keys)
            {
                if (key.StartsWith(prefix))
                {
                    result[key] = this.Params[key];
                }
            }
            return result;
        }

        public override void Contextualize(IEngineContext engineContext, IControllerContext context)
        {
            this.me = new ControllerResourceHelper();
            me.SetController(this, context);
            me.SetContext(engineContext);

            base.Contextualize(engineContext, context);
            PropertyBag["mydb"] = Db;
            PropertyBag["myfiles"] = Resolver;
            PropertyBag["usrdata"] = myapp.getProfile();

            var roots = PkgAttribute.Get(context).ToList();
            var mainroot = new[]{context.AreaName.hasContent() ? context.AreaName : null, context.Name}.concat(".")+".controller";
            if (mainroot.StartsWith(".")) mainroot = mainroot.Substring(1);
            roots.Insert(0,mainroot);
            if (roots.yes()){
                PropertyBag["webpackages"] = roots;
            }

            foreach (var parameter in GetPrefixedParameters("param")){
                PropertyBag[parameter.Key] = parameter.Value;
            }

            PropertyBag["me"] = me;
            PropertyBag["mycss"] = me.getCss();
            PropertyBag["myjs"] = me.getJs();
            PropertyBag["myname"] = context.Name.ToLower();
            if (this.Params["ajax"].noContent())
            {
                try
                {
                    PropertyBag["widgets"] = myapp.widgets.GetMyWidgets();
                }
                catch (Exception ex)
                {

                }
            }
        }


        protected StorageWrapper Db { get; set; }

        public IFilePathResolver Resolver { get; set; }
       

        private string _readedPostData;
        private IScriptMachine _scriptMachine;

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

        public IPrincipal User{
            get { return myapp.usr; }
        }

        public bool IsAjax{
            get { return Params["ajax"] == "1"; }
        }

        protected string PostData{
            get{
                if (null == _readedPostData){
                    using (var reader = new StreamReader(Request.InputStream)){
                        _readedPostData = reader.ReadToEnd();
                    }
                    return _readedPostData;
                }
                return _readedPostData;
            }
        }

        
        public IFilePathResolver PathResolver { get; set; }

        public IApplicationCache Cache { get; set; }

        

        #region IWithConversationController Members

        public IConversation Conversation{
            get { return myapp.conversation.Current; }
        }

        #endregion

        #region IWithScriptExtensions Members

        public IScriptMachine ScriptMachine{
            get{
                if (_scriptMachine == null){
                    return Container.get<IDefaultScriptMachine>();
                }
                return _scriptMachine;
            }
            set { _scriptMachine = value; }
        }

        void IWithScriptExtensions.ExecuteExtenders(string namePattern, params object[] advanced){
            if (null == ScriptMachine){
                return;
            }
            var commonExtender = string.Format(namePattern, string.Empty);
            var localExtender = string.Format(namePattern, Name + ".");
            foreach (var o in GetExtenders(commonExtender)){
                o(this, advanced);
            }
            foreach (var o in GetExtenders(localExtender)){
                o(this, advanced);
            }
        }

        #endregion

        protected void RenderAsFile(string mime, string name){
            Response.ContentType = mime;
            Response.AppendHeader("Content-Disposition", "filename=\"" + name.Replace("\"", "'"));
        }

        protected override object[] BuildMethodArguments(ParameterInfo[] parameters, IRequest request,
                                                         IDictionary<string, object> actionArgs){
            preparePropertiesFromParameters();
            return base.BuildMethodArguments(parameters, request, actionArgs);
        }

        protected void preparePropertiesFromParameters(){
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.hasAttribute<RequestBoundAttribute>());
            foreach (var prop in props){
                var attr = prop.getFirstAttribute<RequestBoundAttribute>();
                var paramname = attr.ParamName;
                if (paramname.noContent()){
                    paramname = prop.Name.ToLower();
                }
                var paramvalue = Params[paramname];
                if (null != paramvalue){
                    this.setPropertySafe(prop.Name, paramvalue);
                    //  this.PropertyBag[prop.Name.ToLower()] = paramvalue;
                }
            }
        }

        protected override void ProcessScaffoldIfAvailable(){
            LoadDynamics();
        }

        protected void LoadDynamics(){
            Container.all<IDynamicActionHandler>()
                .Where(p => p.Match(this))
                .map(p => { DynamicActions[p.ActionName] = p.Action; });
        }

        protected IEnumerable<ControllerProc> GetExtenders(string namePattern){
            return ScriptMachine.Registry
                .Where(p => p.Key.like(namePattern))
                .Select(p => p.Value)
                .OfType<ControllerProc>();
        }

        protected void redirectToError(Exception e){
            ErrorsController.RedirectToException(Context, this, e);
        }

        protected virtual IDictionary<string, string> GetPrefixedParameters(string prefix){
            var result = new Dictionary<string, string>();
            if(null==prefix) return result;
            foreach (var key in Params.AllKeys){
                if (key == null) continue;
                
                if (!key.StartsWith(prefix + ".")){
                    continue;
                }
                result[key.Substring(prefix.Length + 1)] = Params[key];
            }
            return result;
        }


        protected void RedirectToStableConversation(NameValueCollection dict){
            RedirectToStableConversation(Action, dict);
        }

        protected void RedirectToStableConversation(string action){
            RedirectToStableConversation(action, new NameValueCollection());
        }

        protected void RedirectToStableConversation(string action, NameValueCollection dict){
            //myapp.conversation.Current.Options.DropOnEndRequest = false;
            dict["__"] = Conversation.Code;
            Conversation.Finished = false;
            Conversation.cancelneedcleanonleave();
            RedirectToAction(action, dict);
        }

        protected void Commit() {
            var s = myapp.ioc.getSession();
            if(null!=s.Transaction && s.Transaction.IsActive) {
                Conversation.canbecommited();    
            }else {
                s.Flush();
            }
            
        }

        protected HttpPostedFile GetFile(){
            return GetFile("file");
        }

        protected HttpPostedFile GetFile(string name){
            return (HttpPostedFile) Context.Request.Files[name];
        }

        protected IEnumerable<HttpPostedFile> GetFiles(string prefix){
            return
                Context.Request.Files.Keys.OfType<string>().Where(x => x.StartsWith(prefix)).Select(x => GetFile(x)).
                    ToArray();
        }

        protected void tranaction(Action action){
            using (var sw = new TemporaryTransactionSession()){
                action();
                sw.Commit();
            }
        }
    }
}