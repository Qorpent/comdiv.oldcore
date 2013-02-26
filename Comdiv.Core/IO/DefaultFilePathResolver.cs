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
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;

namespace Comdiv.IO
{
    public class DefaultFilePathResolver : ILayeredFilePathResolver, IWithContainer
    {



        public DefaultFilePathResolver(string root)
            : this()
        {
            Root = root;
            myapp.OnReload += (s, a) => this.Reload();
        }

        readonly IDictionary<string, string> resolveCache = new Dictionary<string, string>();
        readonly IDictionary<string, string[]> resolveAllCache = new Dictionary<string, string[]>();


        public void Reload()
        {
            this.resolveCache.Clear();
            this.resolveAllCache.Clear();
        }

        private IInversionContainer _container;

        public IInversionContainer Container
        {
            get
            {
                if (_container.invalid())
                {
                    lock (this)
                    {
                        if (_container.invalid())
                        {
                            Container = myapp.ioc;
                        }
                    }
                }
                return _container;
            }
            set { _container = value; }
        }

        public DefaultFilePathResolver()
        {
            Layers.Add("~/usr");
            Layers.Add("~/mod");
            Layers.Add("~/sys");
            Layers.Add("~/");
            foreach (var rewriter in Container.all<IFilePathRewriter>())
            {
                this.Rewriters.Add(rewriter);
            }
            myapp.OnReload += (s, a) => this.Reload();
        }
        bool isRoot(string path)
        {
            if (Path.IsPathRooted(path)) return true;
            if (path.StartsWith("~/")) return true;
            return false;
        }


        public string Root { get; set; }
        public bool NoUseCache { get; set; }
        public string Resolve(string path, bool existedOnly)
        {
            path = path.Replace("//", "/");
            var key = path + ":" + existedOnly;
            return resolveCache.get(key, () =>
            {
                var rewrited = rewrite(path);
                IList<string> candidates = new List<string>();
                Func<string, string> rootMapper = null;
                if (!string.IsNullOrWhiteSpace(Root))
                {
                    rootMapper = x =>
                    {
                        return x.Replace("~", Root).Replace("//", "/");
                    };
                }
                if (isRoot(rewrited)) candidates.Add(rewrited.mapPath(rootMapper));
                else
                {
                    foreach (var layer in Layers)
                    {
                        var _subpath = layer;
                        if (DefaultPrefix.hasContent())
                        {
                            _subpath = combine(layer, DefaultPrefix);
                        }
                        _subpath = combine(_subpath, rewrited);
                        candidates.Add(_subpath.mapPath(rootMapper));


                    }
                }
                foreach (var file in candidates)
                {
                    if (!existedOnly || File.Exists(file) || Directory.Exists(file))
                    {
                        return Path.GetFullPath(file).clearPath();
                    }
                }
                return null;
            }, !NoUseCache);
        }


        public string DefaultPrefix { get; set; }


        protected string rewrite(string path)
        {
            //HACK: recognizes layer names and make path routed if it's started with layer
            if (!isRoot(path))
            {
                foreach (var layer in Layers)
                {
                    if (path.StartsWith(layer.find(@"\w+") + "/"))
                    {
                        path = "~/" + path;
                        break;

                    }
                }
            }
            bool changed = true;
            string current = path;
            while (changed)
            {
                path = current;
                foreach (var rewriter in Rewriters.OrderByDescending(x => x.Idx))
                {
                    current = rewriter.Rewrite(current) ?? current;
                }
                changed = (path != current);
            }

            return path;

        }

        public IEnumerable<string> ResolveAll(string path, string searchMask, bool existedOnly, Action<string> writeLog)
        {
            return ResolveAll(path, searchMask, existedOnly, true, writeLog);
        }

        public IEnumerable<string> ResolveAll(string path, string searchMask, bool existedOnly, bool recursive, Action<string> writeLog)
        {
            writeLog = writeLog ?? (s => { });
            var key = path + ":" + (searchMask ?? "") + ":" + existedOnly + ":" + recursive;
            return resolveAllCache.get(key, () =>
            {
                var rewrited = rewrite(path);
                IEnumerable<string> result = null;
                if (searchMask.yes())
                {
                    writeLog("directory resolving");
                    result = resolveAllOfDirectory(rewrited, searchMask, recursive, writeLog).ToArray();
                }
                else
                {
                    writeLog("layer resolving");
                    result = resolveAllFilesByLayers(rewrited, existedOnly, writeLog).ToArray();
                }
                result = result.Select(x => x.normalizePath()).Distinct();
                return result.ToArray();
            }, !NoUseCache);
        }

        private IDictionary<string, object> _data = new Dictionary<string, object>();

        public IDictionary<string, object> Data
        {
            get { return _data; }
        }

        private IEnumerable<string> resolveAllFilesByLayers(string path, bool existedOnly, Action<string> writeLog)
        {
            writeLog = writeLog ?? (s => { });
            IList<string> roots = isRoot(path) ? new[] { path } : Layers;
            foreach (var layer in roots)
            {
                var subpath = isRoot(path) ? path : combine(layer, path);
                writeLog("try " + subpath);
                var resolved = Resolve(subpath, existedOnly);

                if (null != resolved)
                {
                    writeLog("ok");
                    yield return resolved;
                }
                else
                {
                    writeLog("not finded");
                }
            }

        }

        string combine(string left, string right)
        {
            return (left + "/" + right).replace(@"\\", "/").replace("/+", "/");
        }

        private IEnumerable<string> resolveAllOfDirectory(string path, string searchMask, bool recursive, Action<string> writeLog)
        {
            writeLog(string.Format("rofd: {0}:{1}:{2}", path, searchMask, recursive));
            writeLog = writeLog ?? (s => { });
            IList<string> roots = isRoot(path) ? new[] { path } : Layers;
            var opts = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            writeLog("rofd roots: " + roots.concat(", "));
            foreach (var layer in roots)
            {
                var subpath = isRoot(path) ? path : combine(layer, path);

                var dir = Resolve(subpath, false);
                writeLog("dir " + dir);
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.GetFiles(dir, searchMask, opts))
                    {
                        writeLog("found " + file);
                        yield return file;
                    }
                }
                else
                {
                    writeLog("not existed");
                }
            }
        }

        private IList<string> _layers = new List<string>();

        public IList<string> Layers
        {
            get { return _layers; }

        }

        private IList<IFilePathRewriter> _rewriters = new List<IFilePathRewriter>();

        public IList<IFilePathRewriter> Rewriters
        {
            get { return _rewriters; }

        }
    }
}