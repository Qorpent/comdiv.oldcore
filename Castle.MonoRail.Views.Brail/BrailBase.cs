// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System.Diagnostics;
using System.Linq;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;

namespace Castle.MonoRail.Views.Brail
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Web;
	using Framework;

	/// <summary>
	///Base class for all the view scripts, this is the class that is responsible for
	/// support all the behind the scenes magic such as variable to PropertyBag trasnlation, 
	/// resources usage, etc. 
	/// </summary>
	public abstract partial class BrailBase:BrailBaseCommon
	{
		

        protected override IBooViewEngine engineByInterface
        {
            get { return (IBooViewEngine)viewEngine; }
        }    

		protected IController __controller;
		protected IControllerContext __controllerContext;

		/// <summary>
		/// Reference to the DSL service
		/// </summary>
		private DslProvider _dsl;

		/// <summary>
		/// This is used by layout scripts only, for outputing the child's content
		/// </summary>
		

		protected internal IEngineContext context;
		
		

		/// <summary>
		/// used to hold the ComponentParams from the view, so their views/sections could access them
		/// </summary>
		private IList viewComponentsParameters;

		protected BooViewEngine viewEngine;

		/// <summary>
		/// Initializes a new instance of the <see cref="BrailBase"/> class.
		/// </summary>
		/// <param name="viewEngine">The view engine.</param>
		/// <param name="output">The output.</param>
		/// <param name="context">The context.</param>
		/// <param name="__controller">The controller.</param>
		/// <param name="__controllerContext">The __controller context.</param>
		public BrailBase(BooViewEngine viewEngine, TextWriter output,
		                 IEngineContext context, IController __controller, IControllerContext __controllerContext)
		{
			this.viewEngine = viewEngine;
			outputStream = output;
			this.context = context;
			this.__controller = __controller;
			this.__controllerContext = __controllerContext;
			InitProperties(context, __controller, __controllerContext);
		}

        

		/// <summary>
		///The path of the script, this is filled by AddBrailBaseClassStep
		/// and is used for sub views 
		/// </summary>
		public virtual string ScriptDirectory
		{
			get { return viewEngine.ViewRootDir; }
		}

        

       
		public abstract string ViewFileName
		{ 
			get;
		}

		/// <summary>
		/// Gets the view engine.
		/// </summary>
		/// <value>The view engine.</value>
		public BooViewEngine ViewEngine
		{
			get { return viewEngine; }
		}

		/// <summary>
		/// Gets the DSL provider
		/// </summary>
		/// <value>Reference to the current DSL Provider</value>
		public DslProvider Dsl
		{
			get
			{
				BrailBase view = this;
				if (null == view._dsl)
				{
					view._dsl = new DslProvider(view);
				}

				return view._dsl;
				//while (view.parent != null)
				//{
				//    view = view.parent;
				//}

				//if (view._dsl == null)
				//{
				//    view._dsl = new DslProvider(view);
				//}

				//return view._dsl;
			}
		}

		/// <summary>
		/// Gets the flash.
		/// </summary>
		/// <value>The flash.</value>
		public Flash Flash
		{
			get { return context.Flash; }
		}

		

		


	
		/// <summary>
		/// Outputs the sub view to the writer
		/// </summary>
		/// <param name="subviewName">Name of the subview.</param>
		/// <param name="writer">The writer.</param>
		/// <param name="parameters">The parameters.</param>
		public override void OutputSubView(string subviewName, TextWriter writer, IDictionary parameters)
		{
			string subViewFileName = GetSubViewFilename(subviewName).normalizePath().ToLower();

		    subviewName = subViewFileName;
            if (!subviewName.StartsWith("/")) subViewFileName = "/" + subViewFileName;
            /*if (!subviewName.StartsWith("/")) {
                subviewName = (IoExtensions.GetDirectoryAndFile(subViewFileName));
                if (subViewFileName.Contains("/extensions/")) {
                    subviewName = subViewFileName.replace(@"^[\s\S]+?/extensions/", "");
                }
                else if (subViewFileName.normalizePath().Contains("/views/")) {
                    subviewName = subViewFileName.replace(@"^[\s\S]+?/views/", "");
                }
            }*/
		    subviewName =subviewName.Replace(".brail", "");



		    BrailBase view = null;
            try {
                 view =
                    viewEngine.GetCompiledScriptInstance(subViewFileName, writer, context, __controller,
                                                         __controllerContext, subviewName);
            }catch(Exception e) {
                throw new Exception("subview: "+subviewName,e);
            }
		    view.SetParent(this);
			foreach(DictionaryEntry entry in parameters)
			{
				view.properties[entry.Key] = entry.Value;
			
            }
            TextWriter oldos = null;
            string key = "";
            bool fromcache = false;

            Stopwatch sw = null;
            if (ViewEngine.Options.CollectStatistics)
            {
                sw = Stopwatch.StartNew();
            }


            if (view.__isCacheable())
            {
                key = view.__getCacheKey();
                if (viewEngine.OutputCache.ContainsKey(key))
                {
                    view.OutputStream.Write(viewEngine.OutputCache[key]);
                    fromcache = true;
                }
                else
                {
                    oldos = view.OutputStream;
                    view.SetOutputStream(new StringWriter());
                }
                
            }

            if (!fromcache)
            {
                view.Run();
                if (view.__isCacheable())
                {
                    var content = view.OutputStream.ToString();
                    viewEngine.OutputCache[key] = content;
                    view.SetOutputStream(oldos);
                    view.OutputStream.Write(content);
                }
            }

            if (this.ViewEngine.Options.CollectStatistics)
            {
                sw.Stop();
                this.ViewEngine.Statistics.Rendered(subviewName, fromcache, sw.ElapsedTicks);
            }

            //old style bubbling up
#if USE_OLD_BUBBLING
			foreach(DictionaryEntry entry in view.Properties)
			{
				if (view.Properties.Contains(entry.Key + ".@bubbleUp") == false)
					continue;
				properties[entry.Key] = entry.Value;
				properties[entry.Key + ".@bubbleUp"] = true;
			}
#endif
            
            //new bubling up
            view.BindExportedProperties();
		}

		/// <summary>
		/// Get the sub view file name, if the subview starts with a '/' 
		/// then the filename is considered relative to ViewDirRoot
		/// otherwise, it's relative to the current script directory
		/// </summary>
		/// <param name="subviewName"></param>
		/// <returns></returns>
		public string GetSubViewFilename(string subviewName)
		{
			//absolute path from Views directory
			if (subviewName[0] == '/')
				return subviewName.Substring(1) + viewEngine.ViewFileExtension;
			return Path.Combine(ScriptDirectory, subviewName) + viewEngine.ViewFileExtension;
		}


		
		/// <summary>
		/// Wraps the possilbe null value in an ignore value object
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public object WrapPossilbeNullValue(object value)
		{
			return new IgnoreNull(value);
		}


		/// <summary>
		/// Sets the parent.
		/// </summary>
		/// <param name="myParent">My parent.</param>
		public void SetParent(BrailBase myParent)
		{
			_parent = myParent;
		}

	

		/// <summary>
		/// This is required because we may want to replace the output stream and get the correct
		/// behavior from components call RenderText() or RenderSection()
		/// </summary>
		public IDisposable SetOutputStream(TextWriter newOutputStream)
		{
			ReturnOutputStreamToInitialWriter disposable = new ReturnOutputStreamToInitialWriter(OutputStream, this);
			outputStream = newOutputStream;
			return disposable;
		}

		


		/// <summary>
		/// Adds the view component newProperties.
		/// This will be included in the parameters searching, note that this override
		/// the current parameters if there are clashing.
		/// The search order is LIFO
		/// </summary>
		/// <param name="newProperties">The newProperties.</param>
		public void AddViewComponentProperties(IDictionary newProperties)
		{
			if (viewComponentsParameters == null)
				viewComponentsParameters = new ArrayList();
			viewComponentsParameters.Insert(0, newProperties);
		}

		/// <summary>
		/// Removes the view component properties, so they will no longer be visible to the views.
		/// </summary>
		/// <param name="propertiesToRemove">The properties to remove.</param>
		public void RemoveViewComponentProperties(IDictionary propertiesToRemove)
		{
			if (viewComponentsParameters == null)
				return;
			viewComponentsParameters.Remove(propertiesToRemove);
		}

		public void RenderComponent(string componentName)
		{
			RenderComponent(componentName, new Hashtable());
		}

		public void RenderComponent(string componentName, IDictionary parameters)
		{
			BrailViewComponentContext componentContext =
				new BrailViewComponentContext(this, null, componentName, OutputStream,
				                              new Hashtable(parameters, StringComparer.InvariantCultureIgnoreCase));
			AddViewComponentProperties(componentContext.ComponentParameters);
			IViewComponentFactory componentFactory = (IViewComponentFactory) context.GetService(typeof(IViewComponentFactory));
			ViewComponent component = componentFactory.Create(componentName);
			component.Init(context, componentContext);
			component.Render();
			if (componentContext.ViewToRender != null)
			{
				OutputSubView("/" + componentContext.ViewToRender, componentContext.ComponentParameters);
			}
			RemoveViewComponentProperties(componentContext.ComponentParameters);
		}

		/// <summary>
		/// Initialize all the properties that a script may need
		/// One thing to note here is that resources are wrapped in ResourceToDuck wrapper
		/// to enable easy use by the script
		/// </summary>
		/// <param name="myContext">My context.</param>
		/// <param name="myController">My controller.</param>
		/// <param name="controllerContext">The controller context.</param>
		public virtual void InitProperties(IEngineContext myContext, IController myController, IControllerContext controllerContext)
		{
			properties = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			properties.Add("Controller", myController);
			
			if (myContext != null)
			{
				properties.Add("request", myContext.Request);
				properties.Add("response", myContext.Response);
				properties.Add("session", myContext.Session);
			}

			if (controllerContext.Resources != null)
			{
				foreach(string key in controllerContext.Resources.Keys)
				{
					properties.Add(key, new ResourceToDuck(controllerContext.Resources[key]));
				}
			}

			if (myContext != null && myContext.Request.QueryString != null)
			{
				foreach(string key in myContext.Request.QueryString.AllKeys)
				{
					if (key == null) continue;
					properties[key] = myContext.Request.QueryString[key];
				}
			}

			if (myContext != null && myContext.Request.Form != null)
			{
				foreach(string key in myContext.Request.Form.AllKeys)
				{
					if (key == null) continue;
					properties[key] = myContext.Request.Form[key];
				}
			}


			if (myContext != null && myContext.Flash != null)
			{
				foreach(DictionaryEntry entry in myContext.Flash)
				{
					properties[entry.Key] = entry.Value;
				}
			}

			if (controllerContext.PropertyBag != null)
			{
				foreach(DictionaryEntry entry in controllerContext.PropertyBag)
				{
					properties[entry.Key] = entry.Value;
				}
			}

			if (controllerContext.CustomActionParameters != null)
			{
				foreach (KeyValuePair<string,object> entry in controllerContext.CustomActionParameters)
				{
					properties[entry.Key] = entry.Value;
				}
			}

			if (controllerContext.Helpers != null)
			{
				foreach(DictionaryEntry entry in controllerContext.Helpers)
				{
					properties[entry.Key] = entry.Value;
				}
			}

			if (myContext != null)
			{
				properties["siteRoot"] = myContext.ApplicationPath;
			}

			if(_parent != null)
			{
				foreach(DictionaryEntry entry in _parent.Properties)
				{
					properties[entry.Key] = entry.Value;
				}
			}

		}

		
		#region Nested type: ReturnOutputStreamToInitialWriter

		private class ReturnOutputStreamToInitialWriter : IDisposable
		{
			private TextWriter initialWriter;
			private BrailBase parent;

			public ReturnOutputStreamToInitialWriter(TextWriter initialWriter, BrailBase parent)
			{
				this.initialWriter = initialWriter;
				this.parent = parent;
			}

			#region IDisposable Members

			public void Dispose()
			{
				parent.outputStream = initialWriter;
			}

			#endregion
		}

		#endregion
	}
}
