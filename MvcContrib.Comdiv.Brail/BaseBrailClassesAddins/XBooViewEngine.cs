// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
// COMDIV:
//  1. NEW: string preprocessing on opening source
//  2. CHG: simplified real-path based brail file loading with simple "last-write-checking"
//  3. REF: commented mono-rail old code parts cleaned
//  4. CHG: dropped dependency from viewsourceloader since it meant to be wrong class that badly
//     overrides behaviour of factory. it's just copy from monorail 
//        (viewsourceloader, viewrootdir, onviewchanged)
//  5. NOTE: while viewsourceloader is not used - for now you cannot provide not-file based views, avoid it by copying non-files 
//     views but it not seems to be great disadvantage
//  6. NEW: backref to factory

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Comdiv;
using System.Text.RegularExpressions;
#if !MONORAIL

using System.Web.Mvc;
using Boo.Lang.Compiler.IO;
using Comdiv.Extensibility.Brail;
using Comdiv.Extensions;
using MvcContrib.ViewFactories;
#endif
#if MONORAIL
namespace Castle.MonoRail.Views.Brail
#else

namespace MvcContrib.Comdiv.ViewEngines.Brail
#endif
{
    public partial class BooViewEngine : IBooViewEngine {
#if MONORAIL
        private string preprocess(string name, string src){
            string srcContent = src;
            foreach (IBrailPreprocessor preprocessor in Options.Preprocessors.OrderBy(p => p.Idx)){
                srcContent = preprocessor.Preprocess(srcContent);
            }
            if (!string.IsNullOrEmpty(basePreprocessSavePath)){
                string subpath = Path.GetDirectoryName(name) + "_" + Path.GetFileName(name);
                string saveFileName = Path.Combine(basePreprocessSavePath, subpath.Replace("/", "_").Replace("\\", "_"));
                File.WriteAllText(saveFileName, srcContent);
            }
            return srcContent;
        }

        private string basePreprocessSavePath;
#endif
        private DateTime lastCompile = DateTime.MinValue;
#if !MONORAIL
        public BrailViewFactory Factory { get; set; }
#endif
        public string SelfResolveRoot { get; set; }

        public IDictionary<string, string> OutputCache {
            get { return _outputCache; }
        }

        private readonly IDictionary<string, string> _outputCache = new Dictionary<string, string>();

#if !MONORAIL
        public string ProcessFile(string filename, object viewdata){
            return ProcessFile(filename, viewdata, null);
        }

        public string ProcessFile(string filename, object viewdata, string layout){
            return ProcessCode(File.ReadAllText(filename), viewdata, layout);
        }

        public string ProcessCode(string code, object viewdata){
            return ProcessCode(code, viewdata, null);
        }

        public string ProcessCode(string code, object viewdata, string layout){
            BrailBase instance = CreateViewFromCode(code);
            var writer = new StringWriter();
            ViewContext context = CreateViewContext(viewdata);
            instance.Render(context, writer);
            if (null != layout)
            {
                var laywr = new StringWriter();
                BrailBase layoutv = CreateViewFromCode(Factory.CustomResolveCode(layout));
                layoutv.ChildOutput = writer;
                layoutv.Render(context,laywr);
                return laywr.ToString();
            }
            else{
                return writer.ToString();
            }
        }

        public readonly IDictionary<string, Type> TemporaryCompiledCache = new Dictionary<string, Type>();

        private BrailBase CreateViewFromCode(string code){
            MD5 hasher = MD5.Create();
            string hash = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(code)));
            Type viewtype = null;
            if (TemporaryCompiledCache.ContainsKey(hash)){
                viewtype = TemporaryCompiledCache[hash];
            }
            else{
                CompilationResult compilation = DoCompile(new[]{new StringInput("_temp_.boo", code)}, "_temp_.boo");
                if (compilation.Context.Errors.Count != 0){
                    RaiseCompilationException("_temp_", null, compilation);
                }
                viewtype =
                    compilation.Context.GeneratedAssembly.GetTypes().First(x => typeof (BrailBase).IsAssignableFrom(x));
                TemporaryCompiledCache[hash] = viewtype;
            }
            return (BrailBase) viewtype.GetConstructor(new[]{typeof (BooViewEngine)}).Invoke(new[]{this});
        }

        public ViewContext CreateViewContext(object viewdata){
            if (null == viewdata){
                return new ViewContext();
            }

            if (viewdata is ViewContext){
                return (ViewContext) viewdata;
            }
            var result = new ViewContext();
            if (viewdata is ViewDataDictionary){
                result.ViewData = (ViewDataDictionary) viewdata;
            }
            else if (viewdata is IDictionary<string, object>){
                result.ViewData = new ViewDataDictionary();
                foreach (var i in (IDictionary<string, object>) viewdata){
                    result.ViewData[i.Key] = i.Value;
                }
            }
            else if (viewdata.GetType().Name.Contains("Anonymous")){
                result.ViewData = new ViewDataDictionary();
                foreach (PropertyInfo prop in viewdata.GetType().GetProperties()){
                    result.ViewData[prop.Name] = prop.GetValue(viewdata, null);
                }
            }
            else{
                result.ViewData = new ViewDataDictionary(viewdata);
            }
            return result;
        }

#endif

        private bool has_boo_only_mode(string firstcode){
            if (Regex.IsMatch(firstcode, @"^\s*\#pragma boo")){
                return true;
            }
            return firstcode.like(@"\bbml\s*:") && !firstcode.like("\bend\b");
        }
    }
}