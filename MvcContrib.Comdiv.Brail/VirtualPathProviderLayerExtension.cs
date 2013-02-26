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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Comdiv;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;


namespace MvcContrib.Comdiv.Layers{
    public static class VirtualPathProviderLayerExtension{
        private static object sync = new object();
        private static ILayeredFilePathResolver _resolver;
        private static IInversionContainer _container;

        public static IInversionContainer Container{
            get{
                if (_container.invalid()){
                    lock (typeof(VirtualPathProviderLayerExtension)){
                        if (_container.invalid()){
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }
        public static ILayeredFilePathResolver Resolver{
            get{
                lock (sync){
                    if(null==_resolver){
                        _resolver = (ILayeredFilePathResolver)myapp.files;
                    }
                    return _resolver;
                }
            }
        }
        private static readonly IDictionary<VirtualPathProviderViewEngine, string[]> _roots =
            new Dictionary<VirtualPathProviderViewEngine, string[]>();

        public static T setRoots<T>(this T engine, string[] roots) where T : VirtualPathProviderViewEngine{
            _roots[engine] = roots;
            return engine;
        }

        public static string ResolveSubView(this VirtualPathProviderViewEngine engine, string currentView,
                                            string subview){
            if (subview.StartsWith("/")){
                return resolveAbsoluteSubview(engine, subview.Substring(1), currentView);
            }
            string root = resolveRoot(engine, currentView);
            string path = Path.Combine(Path.GetDirectoryName(root), subview);
            return resolveAbsoluteSubview(engine, path, currentView);
        }

        private static string resolveRoot(VirtualPathProviderViewEngine engine, string view){
            foreach (string root in engine.getRoots()){
                string absroot = root.mapPath().ToLower();
                if (view.ToLower().StartsWith(absroot)){
                    return view.Substring(absroot.Length);
                }
            }
            string bypassroot =
                Regex.Match(view, @"^(?i)[\s\S]+?views[\\/](layouts[\\/])?", RegexOptions.Compiled).Value;
            if (!string.IsNullOrEmpty(bypassroot)){
                return bypassroot;
            }
            throw new Exception("root for " + view + " was not found");
        }

        private static string resolveAbsoluteSubview(VirtualPathProviderViewEngine engine, string subview,
                                                     string currentView){
            foreach (string root in engine.getRoots()){
                string fn = Path.Combine(
                    root.mapPath(), subview
                    );
                if (File.Exists(fn)){
                    return fn;
                }
            }
            string bypassroot = resolveRoot(engine, currentView);
            string file = Path.Combine(bypassroot, subview);
            if (File.Exists(file)){
                return file;
            }
            throw new FileNotFoundException(subview);
        }

        public static string[] getRoots<T>(this T engine) where T : VirtualPathProviderViewEngine{
            if (_roots.ContainsKey(engine)){
                return _roots[engine];
            }
            return new string[]{};
        }

        public static T configureLayers<T>(
            this T engine,
            string extension
            ) where T : VirtualPathProviderViewEngine{
            return configureLayers(engine, extension, Resolver.Layers.Select(x=>x.Replace("~/","")).ToArray());
        }

        public static T configureLayers<T>(
            this T engine,
            string extension,
            params string[] roots
            ) where T : VirtualPathProviderViewEngine{
            return configureLayers(engine, extension, true, roots);
        }

        public static T configureLayers<T>(
            this T engine,
            string extension,
            bool useLayouts,
            params string[] roots
            ) where T : VirtualPathProviderViewEngine{
            var rs = new List<string>();

            if (useLayouts){
                foreach (string r in roots){
                    rs.Add("~/" + r + "/views/layouts/");
                }
            }
            foreach (string r in roots){
                rs.Add("~/" + r + "/views/");
            }
            engine.setRoots(rs.ToArray());

            engine.ViewLocationFormats =
                (from r in rs where !r.Contains("layouts") select r + "{1}/{0}." + extension).ToArray();
            if (useLayouts){
                engine.MasterLocationFormats =
                    (from r in rs where r.Contains("layouts") select r + "{0}." + extension).ToArray();
            }
            else{
                engine.MasterLocationFormats = engine.ViewLocationFormats;
            }
            engine.PartialViewLocationFormats = engine.ViewLocationFormats;
            return engine;
        }
    }
} 