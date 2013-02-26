using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.MVC;
using Comdiv.Persistence;
using Comdiv.Security;
using Qorpent.Log;

namespace Comdiv.Extensibility.Brail {
    public abstract class BrailBaseCommon {
        public string __write(object obj) {
            if (null == obj) return null;
            OutputStream.Write(obj);
            return null;
        }

		public BrailBaseCommon() {
			this.log = myapp.QorpentApplication.LogManager.GetLog(this.GetType().FullName + ";MvcHandler", this);
		}
        private const string fiopattern = @"^(\w+)\s+(\w+)\s+(\w+)$";
        protected TextWriter _catchwriter = null;
        private string __getCacheKeyResult = null;
        protected readonly IDictionary<string, object> exportProperties = new Dictionary<string, object>();
        protected Hashtable properties = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        public string LastVariableAccessed;

        /// <summary>
        /// usually used by the layout to refer to its view, or a subview to its parent
        /// </summary>
        protected BrailBaseCommon _parent;

        protected TextWriter outputStream;

        protected string getfilename(string path, bool existed) {
            return myapp.files.Resolve(path, existed);
        }

        public string siteroot {
            get { return GetParameter("siteroot").toStr(); }
        }

        /// <summary>
        /// This is used by layout scripts only, for outputing the child's content
        /// </summary>
        protected TextWriter childOutput;

        private static int __id = 1;
	    protected IUserLog log;

	    public virtual string inspect(){

//debug propose;
            int i = 1;
            return i.ToString();
        }

        public string FileName { get; set; }
        protected TextWriter CacheIndependedWriter { get; set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public IDictionary Properties{
            get { return properties; }
        }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        /// <value>The output stream.</value>
        public TextWriter OutputStream{
            get { return _catchwriter ?? outputStream; }
			set { _catchwriter = value; }
        }

        /// <summary>
        /// Gets or sets the child output.
        /// </summary>
        /// <value>The child output.</value>
        public TextWriter ChildOutput{
            get { return childOutput; }
            set { childOutput = value; }
        }

        public BrailBaseCommon Layout { get; set; }
        protected virtual IBooViewEngine engineByInterface { get { return null; } }

        public StorageWrapper mydb{
            get{
                if (null == this.Properties["mydb"]){ 
                    this.Properties["mydb"] = myapp.storage.GetDefault();
                }
                return this.Properties["mydb"] as StorageWrapper;
            }
        }

		public ISqlService mysql
		{
			get
			{
				if (null == this.Properties["mysql"])
				{
					this.Properties["mysql"] = myapp.sql;
				}
				return this.Properties["mysql"] as ISqlService;
			}
		}

        public IFilePathResolver myfiles
        {
            get
            {
                return this.Properties["myfiles"] as IFilePathResolver;
            }
        }

        public int lastreload{
            get{
                return myapp.LastReloadStamp;
            }

        }

        public IProfileRepository usrdata{
            get{
                return myapp.getProfile();
            }
        }

        public string usrname
        {
            get { return myapp.usrName; }

        }

        protected T _convert<T>(object obj){
            return obj.to<T>();
        }

        protected bool isempty(object e){
            if (null == e){
                return true;
            }
            return (e as IEnumerable).Cast<object>().Count() == 0;
        }

        public void _catchoutput(){
            _catchwriter = new StringWriter();
        }

        public string _endcatchoutput(){
            string result = _catchwriter.ToString();
            _catchwriter = null;
            return result;
        }

        protected IEnumerable<T> range<T>(T start,T finish, Func<T, T> next){
            var current = start;
            do{
                yield return current;
                current = next(current);
            } while (!current.Equals(finish));
            
        }

        protected IEnumerable<int> range(int start,int finish){
            return range(start, finish, x=>{
                                               if(start<=finish){
                                                   return x + 1;
                                               }else{
                                                   return x - 1;
                                               }
            });
        }

        protected IList<object> _wrapcollection(object col){
            if (null == col){
                return null;
            }
            if (col is IEnumerable){
                return ((IEnumerable) col).Cast<object>().ToList();
            }
            return new[]{col}.ToList();
        }

        protected string _escape(object html){
            return WebUtility.HtmlEncode((html ?? "").ToString()).Replace("'", "&apos;");
        }

        protected internal virtual string _key(){
            return null;
        }

        public bool __isCacheable(){
            return _key() != null;
        }

        public string __getCacheKey(){
            if (null == __getCacheKeyResult){
                Type t = GetType();
                __getCacheKeyResult = GetType().FullName + "_" + t.Assembly.GetHashCode() + "_" + t.GetHashCode() + "_"
                                      + _key();
            }
            return __getCacheKeyResult;
        }

        public void __Export(string name){
            __Export(name, Missing.Value);

            
        }

        public void __Export(string name,object obj){
            if(!exportProperties.ContainsKey(name)){
                exportProperties.Add(name,obj);
            }
        }

        public object TryGetParameterNoIgnoreNull(string name){
            if (!Properties.Contains(name)){
                return null;
            }
            return Properties[name];
        }

        public void SetProperty(string name, object value){
            Properties[name] = value;
        }

        protected object best(params object[] parameters){
            object current = null;
            foreach (object parameter in parameters){
                current = parameter;
                if (null != current){
                    break;
                }
            }
            return current;
        }

        protected class ParameterSearch{
            private readonly bool found;
            private readonly object value;

            public ParameterSearch(object value, bool found){
                this.found = found;
                this.value = value;
            }

            public bool Found{
                get { return found; }
            }

            public object Value{
                get { return value; }
            }
        }

        public T[] FindAllProperties<T>()
        {
            IDictionary<string,object> cache = new Dictionary<string, object>();
            var current = this;
            while(null!=current)
            {
                foreach (string property in current.Properties.Keys)
                {
                    
                    if(cache.ContainsKey(property)) continue;
                    var value = current.Properties[property];
                    if(!(value is T))continue;
                    cache[property] = value;
                }
                current = current._parent;
            }
            return cache.Values.OfType<T>().ToArray();
        }

        /// <summary>
        /// Gets the parameter - implements the logic for searching parameters.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected ParameterSearch GetParameterInternal(string name){
            LastVariableAccessed = name;
            //temporary syntax to turn @variable to varaible, imitating :symbol in ruby
            if (name.StartsWith("@")){
                return new ParameterSearch(name.Substring(1), true);
            }
            if (properties.Contains(name)){
                return new ParameterSearch(properties[name], true);
            }
            if (_parent != null){
                return _parent.GetParameterInternal(name);
            }
            return new ParameterSearch(null, false);
        }

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="myParent">My parent.</param>
        public void SetParent(BrailBaseCommon myParent){
            _parent = myParent;
        }

        /// <summary>
        /// Allows to check that a parameter was defined
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsDefined(string name){
            ParameterSearch search = GetParameterInternal(name);
            return search.Found;
        }

        public void BindExportedProperties(){
            BindExportedProperties(_parent);
        }

        public void BindExportedProperties(BrailBaseCommon target){
            foreach (var propname in exportProperties){
                if (!(propname.Value is Missing))
                {
                    target.Properties[propname.Key] = propname.Value;
                }
                else{
                    target.Properties[propname.Key] = TryGetParameterNoIgnoreNull(propname.Key);
                }
            }
        }

        /// <summary>
        /// Output the subview to the client, this is either a relative path "SubView" which
        /// is relative to the current /script/ or an "absolute" path "/home/menu" which is
        /// actually relative to ViewDirRoot
        /// </summary>
        /// <param name="subviewName"></param>
        public string OutputSubView(string subviewName){
            return OutputSubView(subviewName, new Hashtable());
        }

        /// <summary>
        /// Similiar to the OutputSubView(string) function, but with a bunch of parameters that are used
        /// just for this subview. This parameters are /not/ inheritable.
        /// </summary>
        /// <returns>An empty string, just to make it possible to use inline ${OutputSubView("foo")}</returns>
        public string OutputSubView(string subviewName, IDictionary parameters){
            OutputSubView(subviewName, OutputStream, parameters);
            return string.Empty;
        }

        /// <summary>
        /// this is called by ReplaceUnknownWithParameters step to create a more dynamic experiance
        /// any uknown identifier will be translate into a call for GetParameter('identifier name').
        /// This mean that when an uknonwn identifier is in the script, it will only be found on runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual object GetParameter(string name){
            ParameterSearch search = GetParameterInternal(name);
            if (search.Found == false){
                throw new Exception("Parameter '" + name + "' was not found!");
            }
            return search.Value;
        }

        /// <summary>
        /// this is called by ReplaceUnknownWithParameters step to create a more dynamic experiance
        /// any uknown identifier with the prefix of ? will be translated into a call for 
        /// TryGetParameter('identifier name without the ? prefix').
        /// This method will return null if the value it not found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual object TryGetParameter(string name){
            ParameterSearch search = GetParameterInternal(name);
            return new IgnoreNull(search.Value);
        }

        /// <summary>
        /// Will output the given value as escaped HTML
        /// </summary>
        /// <param name="toOutput"></param>
        public void OutputEscaped(object toOutput){
            if (toOutput == null){
                return;
            }
            string str = toOutput.ToString();
            OutputStream.Write(WebUtility.HtmlEncode(str));
        }

        /// <summary>
        /// Note that this will overwrite any existing property.
        /// </summary>
        public void AddProperty(string name, object item){
            properties[name] = item;
        }

        protected void HandleException(string templateName, BrailBaseCommon view, Exception e){
            var sb = new StringBuilder();
            sb.Append("Exception on RenderView: ").AppendLine(templateName);
            sb.Append("Last accessed variable: ").Append(view.LastVariableAccessed);
            string msg = sb.ToString();
            sb.Append("Exception: ").AppendLine(e.ToString());
//			Log(msg);
            var ex = new Exception(msg, e);
			log.Error(ex.Message,ex,this);
	        throw ex;
        }

        /// <summary>
        /// Runs this instance, this is generated by the script
        /// </summary>
        public abstract void Run();

        protected void _render(){
#if IViewAttachs_SUPPORTED
            foreach (var x in FindAllProperties<IAttachToView>())
            {
                x.Push(this);
            }
#endif
            try{
                
                Run();
            }
            
            catch (Exception e){
                HandleException("", this, e);
            }
            finally
            {
            #if IViewAttachs_SUPPORTED
            foreach (var x in FindAllProperties<IAttachToView>())
                {
                    x.Pop(this);
                }
#endif
            }
        }

        protected virtual bool readCache(){
            bool fromcache = false;
            if (__isCacheable()){
                if (engineByInterface.OutputCache.ContainsKey(__getCacheKey())){
                    outputStream.Write(engineByInterface.OutputCache[__getCacheKey()]);
                    fromcache = true;
                }
                else{
                    outputStream = new StringWriter();
                }
            }
            return fromcache;
        }

        protected virtual void writeToCache(){
            if (__isCacheable()){
                string content1 = outputStream.ToString();
                engineByInterface.OutputCache[__getCacheKey()] = content1;
                string content = content1;
                outputStream = CacheIndependedWriter;
                outputStream.Write(content);
            }
        }

        protected virtual void renderSubView(BrailBaseCommon subView, TextWriter writer) {
            
        }

        /// <summary>
        /// Outputs the sub view to the writer
        /// </summary>
        /// <param name="subviewName">Name of the subview.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="parameters">The parameters.</param>
        public virtual void OutputSubView(string subviewName, TextWriter writer, IDictionary parameters){
            string subViewFileName = engineByInterface.ResolveSubViewName(FileName, subviewName);
            BrailBaseCommon subView = engineByInterface.GetCompiledScriptInstance(subViewFileName);
            subView.SetParent(this);
            foreach (DictionaryEntry entry in parameters){
                subView.Properties[entry.Key] = entry.Value;
            }
            renderSubView(subView, writer);
            subView.BindExportedProperties();
        }

        public void Render(TextWriter writer) {
            if (Layout != null){
                Layout.ChildOutput = outputStream = new StringWriter();
            }
            else{
                outputStream = writer;
            }
            CacheIndependedWriter = outputStream;
            if (!readCache()){
                _render();
                writeToCache();
            }
            if (Layout != null){
                Layout.SetParent(this);
                try {
                    renderSubView(Layout, writer);
                }
                catch (Exception e){
                    HandleException("", Layout, e);
                }
            }
        }

        public string newid(){
            return newid("e");
        }

        public string newid(string prefix){
            return prefix + __id++;
        }

        public T resolve<T>(){
            return myapp.ioc.get<T>();
        }

        public string render(IEnumerable call){
            return render(call, null);
        }

        public string render(IEnumerable call,IDictionary advances)
        {
            var result = "";
            foreach (var viewCall in call.OfType<IViewCall>().OrderBy(x=>x.Idx)){
                result += render(viewCall, advances);
                result += Environment.NewLine;
            }
            return result;
        }

        public string render(IViewCall call){
            return render(call, null);
        }

        public string render(IViewCall call,IDictionary advances){
            try{
                var nameval = new Hashtable();
                foreach (var parameter in call.Parameters){
                    nameval[parameter.Key] = parameter.Value;
                }
                if(null!=advances){
                    foreach (var advance in advances){
                        nameval[advance] = advances[advance];
                    }
                }
                nameval["this"] = call;
                if (call.Text.hasContent())
                {
                    OutputStream.WriteLine(call.Text._formatex(nameval));
                }
                else{
                    this.OutputSubView(call.View, nameval);
                }
                return "";
            }catch(Exception ex){
                return ex.ToString();
            }
        }

        public string tochecked(object tocheck)
        {
            if (tocheck.yes()) return "checked";
            return "";
        }

		public int asint(object obj) {
			return obj.toInt();
		}
		public bool asbool(object obj)
		{
			return obj.toBool();
		}

		public DateTime asdate(object obj)
		{
			return obj.toDate();
		}
		public decimal asdecimal(object obj) {
			return obj.toDecimal();
		}

		public bool isinrole(string login, string role, bool adminisall) {
			return myapp.roles.IsInRole(login.toPrincipal(), role, adminisall);
		}

		public bool isinroleexact(string role) {
			return myapp.roles.IsInRole(role, false);
		}

        public bool isinrole(string role){
            if(myapp.roles.IsAdmin()) return true;
            return myapp.roles.IsInRole(role);
        }

        public string 
            tofio(string name)
        {
            if (name.noContent()) return "";
            if (name.like(fiopattern))
            {
                return name.replace(fiopattern, m => "{1}.{2}. {0}"._format(m.Groups[1].Value,
                                                                            m.Groups[2].Value.Substring(0, 1).ToUpper(),
                                                                            m.Groups[3].Value.Substring(0, 1).ToUpper()));
            }
            return name;
        }

        public string escape(string str) {
            return "<root><!CDATA[" + str + "]]>";
        }
    }

    public interface IAttachToView
    {
        void Push(BrailBaseCommon view);
        void Pop(BrailBaseCommon view);
    }
}