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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Parser;
using Boo.Lang.Runtime;
using Comdiv;
using Comdiv.Extensibility;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;

using MvcContrib.Comdiv.Layers;

namespace MvcContrib.Comdiv.ViewEngines.Brail{
    public partial class BooViewEngine : IBooViewEngine {
        private static MvcViewEngineOptions options;

        public string ResolveSubViewName(string parentfile, string subviewname) {
            return Factory.ResolveViewName(parentfile, subviewname + ".brail");
        }

        /// <summary>
        /// This field holds all the cache of all the 
        /// compiled types (not instances) of all the views that Brail nows of.
        /// </summary>
        private readonly Hashtable compilations = Hashtable.Synchronized(
            new Hashtable(StringComparer.InvariantCultureIgnoreCase));

        /// <summary>
        /// used to hold the constructors of types, so we can avoid using
        /// Activator (which takes a long time
        /// </summary>
        private readonly Hashtable constructors = new Hashtable();

        /// <summary>
        /// Used to map between type and file name, this is useful when we
        /// want to remove a script by its type.
        /// </summary>
        private readonly Hashtable typeToFileName = Hashtable.Synchronized(new Hashtable());


        private string baseSavePath;

        /// <summary>
        /// This is used to add a reference to the common scripts for each compiled scripts
        /// </summary>
        private Assembly common;


        public virtual bool SupportsJSGeneration{
            get { return true; }
        }

        public virtual string ViewFileExtension{
            get { return ".brail"; }
        }

        public virtual string JSGeneratorFileExtension{
            get { return ".brailjs"; }
        }

		public virtual Type BaseTypeInstance { get; set; }

        public MvcViewEngineOptions Options{
            get { return options; }
            set { options = value; }
        }

        public void Initialize(){
            if (options == null){
                InitializeConfig();
            }
            string baseDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            Log("Base Directory: " + baseDir);
            baseSavePath = Path.Combine(baseDir, options.SaveDirectory);
            Log("Base Save Path: " + baseSavePath);
            

            if (options.SaveToDisk && !Directory.Exists(baseSavePath)){
                Directory.CreateDirectory(baseSavePath);
                Log("Created directory " + baseSavePath);
            }

            CompileCommonScripts();

            // Register extension methods
            foreach (Assembly assembly in options.AssembliesToReference){
                if (assembly.GetCustomAttributes(typeof (ExtensionAttribute), true).Length > 0){
                    foreach (Type type in assembly.GetTypes()){
                        foreach (string nmespace in options.NamespacesToImport){
                            if (type != null && type.Namespace != null && type.Namespace.Equals(nmespace)){
                                RuntimeServices.RegisterExtensions(type);
                            }
                        }
                    }
                }
            }
        }


        // _Process a template name and output the results to the user
        // This may throw if an error occured and the user is not local (which would 
        // cause the yellow screen of death)
        public virtual BrailBaseCommon Process(string viewName, string masterName)
//		(String templateName, TextWriter output, IEngineContext context, IController controller, IControllerContext controllerContext)
        {
            if (!Path.IsPathRooted(viewName)){
                viewName = Path.Combine(SelfResolveRoot, viewName + ".brail").mapPath();
            }
            if (!string.IsNullOrEmpty(masterName) && !Path.IsPathRooted(masterName)){
                masterName = Path.Combine(SelfResolveRoot, masterName + ".brail").mapPath();
            }
            Log("Starting to process request for {0}", viewName);
            //string file = viewName + ViewFileExtension;
            // Will compile on first time, then save the assembly on the cache.
            var view = GetCompiledScriptInstance(viewName);

            view.Layout = GetOutput(masterName);

            return view;
        }

        public virtual BrailBaseCommon ProcessPartial(string viewName){
            Log("Generating partial for {0}", viewName);

            try{
                string file = ResolveTemplateName(viewName, ViewFileExtension);
                var view = GetCompiledScriptInstance(file);
                return view;
            }
            catch (Exception ex){
                throw new Exception("Error generating partial: " + viewName, ex);
            }
        }


        protected static string ResolveTemplateName(string templateName, string extention){
            if (Path.HasExtension(templateName)){
                return templateName.ToUpper();
            }
            return templateName.ToUpper() + extention;
        }

        // Check if a layout has been defined. If it was, then the layout would be created
        // and will take over the output, otherwise, the context.Reposne.Output is used, 
        // and layout is null
        private BrailBaseCommon GetOutput(string masterName){
            BrailBaseCommon layout = null;
            if (!string.IsNullOrEmpty(masterName)){
                layout = GetCompiledScriptInstance(masterName);
            }
            return layout;
        }

        /// <summary>
        /// This takes a filename and return an instance of the view ready to be used.
        /// If the file does not exist, an exception is raised
        /// The cache is checked to see if the file has already been compiled, and it had been
        /// a check is made to see that the compiled instance is newer then the file's modification date.
        /// If the file has not been compiled, or the version on disk is newer than the one in memory, a new
        /// version is compiled.
        /// Finally, an instance is created and returned	
        /// </summary>
        public BrailBaseCommon GetCompiledScriptInstance(string filename){
            //assume that file name is (and must be always full filename of view
            bool batch = options.BatchCompile;
            Log("Getting compiled instnace of {0}", filename);
            Type type;
            if (compilations.ContainsKey(filename)){
                if (File.GetLastWriteTime(filename) <= lastCompile){
                    type = (Type) compilations[filename];
                    if (type != null){
                        Log("Got compiled instance of {0} from cache", filename);
                        return CreateBrailBase(type, filename);
                    }
                    // if file is in compilations and the type is null,
                    // this means that we need to recompile. Since this usually means that 
                    // the file was changed, we'll set batch to false and procceed to compile just
                    // this file.
                    Log("Cache miss! Need to recompile {0}", filename);
                    batch = false;
                }
            }
            type = CompileScript(filename, batch);
            lastCompile = DateTime.Now;
            if (type == null){
                throw new Exception("Could not find a view with path " + filename);
            }
            return CreateBrailBase(type, filename);
        }

        public BrailBaseCommon CreateBrailBase(Type type, string filename)
//		(IEngineContext context, IController controller, IControllerContext controllerContext,TextWriter output, Type type)
        {
            var constructor = (ConstructorInfo) constructors[type];
            var self = (BrailBaseCommon) FormatterServices.GetUninitializedObject(type);
            constructor.Invoke(self, new object[]{this}); //, context, controller, controllerContext});
            self.FileName = filename;
            ((BrailBase)self).Factory = Factory;
            return self;
        }

        // Compile a script (or all scripts in a directory), save the compiled result
        // to the cache and return the compiled type.
        // If an error occurs in batch compilation, then an attempt is made to compile just the single
        // request file.
        public Type CompileScript(string filename, bool batch){
            IDictionary<ICompilerInput, string> inputs2FileName = GetInput(filename, batch);
            string name = NormalizeName(filename);
            Log("Compiling {0} to {1} with batch: {2}", filename, name, batch);
            CompilationResult result = DoCompile(inputs2FileName.Keys, name);
            if (options.SaveBooResult){
                Directory.CreateDirectory(
                    options.SaveBooResultDirectory.mapPath()
                    );
                File.WriteAllText(Path.Combine(options.SaveBooResultDirectory.mapPath(), "last_boo.txt"),
                                  result.Context.CompileUnit.ToCodeString());
            }
            if (result.Context.Errors.Count > 0){
                if (batch == false){
                    RaiseCompilationException(filename, inputs2FileName, result);
                }
                //error compiling a batch, let's try a single file
                return CompileScript(filename, false);
            }
            Type type;
            foreach (ICompilerInput input in inputs2FileName.Keys){
                string viewName = Path.GetFileNameWithoutExtension(input.Name);
                string typeName = TransformToBrailStep.GetViewTypeName(viewName);
                type = result.Context.GeneratedAssembly.GetType(typeName);
                Log("Adding {0} to the cache", type.FullName);
                constructors[type] = type.GetConstructor(new[]{typeof (BooViewEngine)});
                string compilationName = inputs2FileName[input];
                typeToFileName[type] = compilationName;
                compilations[compilationName] = type;
            }
            type = (Type) compilations[filename];
            IEnumerable<string> keys = _outputCache.Keys.Where(x => x.StartsWith(type.FullName + "_"));
            foreach (string key in keys){
                _outputCache.Remove(key);
            }

            return type;
        }

        private void RaiseCompilationException(string filename, IDictionary<ICompilerInput, string> inputs2FileName,
                                               CompilationResult result){
            string errors = result.Context.Errors.ToString(true);
            Log("Failed to compile {0} because {1}", filename, errors);
            var code = new StringBuilder();
            if (inputs2FileName != null){
                foreach (ICompilerInput input in inputs2FileName.Keys){
                    code.AppendLine()
                        .Append(result.Processor.GetInputCode(input))
                        .AppendLine();
                }
            }
            string root = "~/".mapPath().ToLower();
            string fn = filename.ToLower().Replace(root, "~/");

            if (HttpContext.Current != null){
                throw new HttpParseException("Error compiling Brail code",
                                             result.Context.Errors[0],
                                             fn,
                                             code.ToString(), result.Context.Errors[0].LexicalInfo.Line);
            }
            else{
                throw new Exception(result.Context.Errors.ToString(true));
            }
        }

        // If batch compilation is set to true, this would return all the view scripts
        // in the director (not recursive!)
        // Otherwise, it would return just the single file
        private IDictionary<ICompilerInput, string> GetInput(string filename, bool batch){

            var input2FileName = new Dictionary<ICompilerInput, string>();

            //HACK: Special case for common scripts
            if (filename.Contains("CommonScripts"))
            {
                return loadCommonScripts(input2FileName);
            }

            var customcode = Factory.CustomResolveCode(filename);
            if(customcode!=null){
                input2FileName.Add(new StringInput(filename,customcode),filename);
                return input2FileName;
            }

            
            if (batch == false){
                input2FileName.Add(CreateInput(filename), filename);
                return input2FileName;
            }
            // use the System.IO.Path to get the folder name even though
            // we are using the ViewSourceLoader to load the actual file
            string directory = Path.GetDirectoryName(filename);
            foreach (string file in Directory.GetFiles(directory, "*.brail")){
                ICompilerInput input = CreateInput(file);
                input2FileName.Add(input, file);
            }
            return input2FileName;
        }

        private IDictionary<ICompilerInput, string> loadCommonScripts(IDictionary<ICompilerInput, string> input2FileName){
            foreach (string root in Factory.getRoots()){
                string absroot = root.mapPath();
                string commonscriptdir = Path.Combine(absroot, "commonscripts");
                if (Directory.Exists(commonscriptdir)){
                    foreach (string file in Directory.GetFiles(commonscriptdir, "*.brail")){
                        ICompilerInput input = CreateInput(file);
                        input2FileName.Add(input, file);
                    }
                }
            }
            return input2FileName;
        }

        // create an input from a resource name
        public ICompilerInput CreateInput(string name){
            string src = File.ReadAllText(name);
            
            return new StringInput(name, src);
        }


        /// <summary>
        /// Perform the actual compilation of the scripts
        /// Things to note here:
        /// * The generated assembly reference the Castle.MonoRail.MonoRailBrail and Castle.MonoRail.Framework assemblies
        /// * If a common scripts assembly exist, it is also referenced
        /// * The AddBrailBaseClassStep compiler step is added - to create a class from the view's code
        /// * The ProcessMethodBodiesWithDuckTyping is replaced with ReplaceUknownWithParameters
        ///   this allows to use naked parameters such as (output context.IsLocal) without using 
        ///   any special syntax
        /// * The FixTryGetParameterConditionalChecks is run afterward, to transform "if ?Error" to "if not ?Error isa IgnoreNull"
        /// * The ExpandDuckTypedExpressions is replace with a derived step that allows the use of Dynamic Proxy assemblies
        /// * The IntroduceGlobalNamespaces step is removed, to allow to use common variables such as 
        ///   date and list without accidently using the Boo.Lang.BuiltIn versions
        /// </summary>
        /// <param name="files"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private CompilationResult DoCompile(IEnumerable<ICompilerInput> files, string name){
            bool boomode = false;
            List<ICompilerInput> filesAsArray = new List<ICompilerInput>(files).ToList();

            string firstcode = files.First().Open().ReadToEnd();
            string firstname = files.First().Name;
            filesAsArray.RemoveAt(0);
            filesAsArray.Insert(0, new StringInput(firstname, firstcode));
            boomode = has_boo_only_mode(firstcode);

            BooCompiler compiler = SetupCompiler(filesAsArray, boomode);
            string filename = Path.Combine(baseSavePath ?? Path.GetTempPath(), name);
            compiler.Parameters.OutputAssembly = filename;
            // this is here and not in SetupCompiler since CompileCommon is also
            // using SetupCompiler, and we don't want reference to the old common from the new one
            if (common != null){
                compiler.Parameters.References.Add(common);
            }
            // pre procsssor needs to run before the parser
            var processor = new BrailPreProcessor(this, boomode);
            compiler.Parameters.Pipeline.Insert(0, processor);
#if !LIB2
            if (boomode){
                compiler.Parameters.Pipeline.InsertAfter(typeof (BooParsingStep), new ExpandBmlStep());
                compiler.Parameters.Pipeline.InsertAfter(typeof (BooParsingStep), new IncludeAstMacroExpandStep());
            }
            else{
                compiler.Parameters.Pipeline.InsertAfter(typeof (WSABooParsingStep), new ExpandBmlStep());
                compiler.Parameters.Pipeline.InsertAfter(typeof (WSABooParsingStep), new IncludeAstMacroExpandStep());
            }
#else
            if (boomode)
            {
                compiler.Parameters.Pipeline.InsertAfter(typeof(BooParsingStep), new ExpandBmlStep());
                compiler.Parameters.Pipeline.InsertAfter(typeof(BooParsingStep), new IncludeAstMacroExpandStep());
            }
            else
            {
                compiler.Parameters.Pipeline.InsertAfter(typeof(WSABooParsingStep), new ExpandBmlStep());
                compiler.Parameters.Pipeline.InsertAfter(typeof(WSABooParsingStep), new IncludeAstMacroExpandStep());
            }
#endif
            // inserting the add class step after the parser
            compiler.Parameters.Pipeline.Insert(2, new TransformToBrailStep(options));
            compiler.Parameters.Pipeline.Replace(typeof (ProcessMethodBodiesWithDuckTyping),
                                                 new ReplaceUknownWithParameters());
            compiler.Parameters.Pipeline.Replace(typeof (ExpandDuckTypedExpressions),
                                                 new ExpandDuckTypedExpressions_WorkaroundForDuplicateVirtualMethods());
           
#if !LIB2
            compiler.Parameters.Pipeline.Replace(typeof (InitializeTypeSystemServices),
                                                 new InitializeCustomTypeSystem());
#endif
            compiler.Parameters.Pipeline.InsertBefore(typeof (MacroAndAttributeExpansion),
                                                      new FixTryGetParameterConditionalChecks());
            compiler.Parameters.Pipeline.RemoveAt(compiler.Parameters.Pipeline.Find(typeof (IntroduceGlobalNamespaces)));
            //compiler.Parameters.Pipeline.InsertAfter(typeof(MacroAndAttributeExpansion),
            //  new global::Boo.Lang.Compiler.Steps.)

            var result = new CompilationResult(compiler.Run(), processor);

            if (options.SaveBooResult){
                if (!string.IsNullOrEmpty(options.SaveBooResultDirectory)){
                    Directory.CreateDirectory(options.SaveBooResultDirectory);
                    string file = Path.Combine(options.SaveBooResultDirectory, "last_boo.txt");
                    File.WriteAllText(file, result.Context.CompileUnit.ToCodeString());
                }
            }

            return result;
        }

        // Return the output filename for the generated assembly
        // The filename is dependant on whatever we are doing a batch
        // compile or not, if it's a batch compile, then the directory name
        // is used, if it's just a single file, we're using the file's name.
        // '/' and '\' are replaced with '_', I'm not handling ':' since the path
        // should never include it since I'm converting this to a relative path
        public string NormalizeName(string filename){
            string name = filename;
            name = name.Replace(Path.AltDirectorySeparatorChar, '_');
            name = name.Replace(Path.DirectorySeparatorChar, '_');

            return name + "_BrailView.dll";
        }

        // Compile all the common scripts to a common assemblies
        // an error in the common scripts would raise an exception.
        public bool CompileCommonScripts(){
            if (options.CommonScriptsDirectory == null){
                return false;
            }

            // the demi.boo is stripped, but GetInput require it.
            string demiFile = Path.Combine(options.CommonScriptsDirectory, "demi.brail");
            IDictionary<ICompilerInput, string> inputs = GetInput(demiFile, true);
            ICompilerInput[] inputsAsArray = new List<ICompilerInput>(inputs.Keys).ToArray();
            BooCompiler compiler = SetupCompiler(inputsAsArray);
            string outputFile = Path.Combine(baseSavePath, "CommonScripts.dll");
            compiler.Parameters.OutputAssembly = outputFile;
            CompilerContext result = compiler.Run();
            if (result.Errors.Count > 0){
                throw new Exception(result.Errors.ToString(true));
            }
            common = result.GeneratedAssembly;
            compilations.Clear();
            return true;
        }

        // common setup for the compiler
        private static BooCompiler SetupCompiler(IEnumerable<ICompilerInput> files){
            return SetupCompiler(files, false);
        }

        private static BooCompiler SetupCompiler(IEnumerable<ICompilerInput> files, bool boomode){
            var compiler = new BooCompiler();
            compiler.Parameters.Ducky = true;
            compiler.Parameters.Debug = options.Debug;
            compiler.Parameters.Pipeline = options.SaveToDisk ? new CompileToFile() : new CompileToMemory();
            // replace the normal parser with white space agnostic one.
            compiler.Parameters.Pipeline.RemoveAt(0);
            compiler.Parameters.Pipeline.Insert(0, boomode ? new BooParsingStep() : new WSABooParsingStep());
            foreach (ICompilerInput file in files){
                compiler.Parameters.Input.Add(file);
            }
            foreach (Assembly assembly in options.AssembliesToReference){
                compiler.Parameters.References.Add(assembly);
            }
            compiler.Parameters.OutputType = CompilerOutputType.Library;
            return compiler;
        }

        private static void InitializeConfig(){
            InitializeConfig("brail");

            if (options == null){
                InitializeConfig("Brail");
            }

            if (options == null){
                options = new MvcViewEngineOptions();
            }
        }

        private static void InitializeConfig(string sectionName){
            options = ConfigurationManager.GetSection(sectionName) as MvcViewEngineOptions;
        }

        private void Log(string msg, params object[] items){
//			if (logger == null || logger.IsDebugEnabled == false)
//				return;
//			logger.DebugFormat(msg, items);
        }

        public bool ConditionalPreProcessingOnly(string name){
            return String.Equals(
                Path.GetExtension(name),
                JSGeneratorFileExtension,
                StringComparison.InvariantCultureIgnoreCase);
        }

        #region Nested type: CompilationResult

        private class CompilationResult{
            private readonly CompilerContext context;
            private readonly BrailPreProcessor processor;

            public CompilationResult(CompilerContext context, BrailPreProcessor processor){
                this.context = context;
                this.processor = processor;
            }

            public CompilerContext Context{
                get { return context; }
            }

            public BrailPreProcessor Processor{
                get { return processor; }
            }
        }

        #endregion
    }
}