using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;

namespace Comdiv.Extensibility.Brail {
    public interface IViewSourceResolver {
        string ResolveSubView(string parentkey, string subviewkey);
        ViewCodeSource[] GetAll();
        ViewCodeSource GetFullInfo(string key);
        string[] GetAllExcludes { get; set; }
    }

    public abstract class ViewSourceResolverBase : IDisposable, IViewSourceResolver {
        private IFilePathResolver _fileSystem;
        private bool invalid;
        private IDictionary<string , ViewCodeSource> sources = new Dictionary<string, ViewCodeSource>();
        private IList<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        private string _extension;

        public ViewSourceResolverBase() {
            invalid = true;
            myapp.OnReload += myapp_OnReload;
        	this.PreprocessFactory = myapp.ioc.get<IBrailPreprocessorFactory>() ?? new PreprocessorFactory();
        }

    	public IBrailPreprocessorFactory PreprocessFactory { get; set; }

    	public string Identity { get; set; }

        public IFilePathResolver FileSystem {
            get {
                if (null == _fileSystem) {
                    _fileSystem = myapp.files;
                    this.invalid = true;
                }
                return _fileSystem;
            }
            set { 
                _fileSystem = value;
                this.invalid = true;
            }
        }

        public string[] GetAllExcludes { get; set; }

        public string Extension {
            get { return _extension ?? (_extension = getDefaultExtension()); }
            set { _extension = value; }
        }

        private void myapp_OnReload(object sender, Common.EventWithDataArgs<int> args) {
            this.Reload();
        }

        public void Reload() {
            this.invalid = true;
        }

        public ViewCodeSource[] GetAll() {
            if(invalid)setup();
            IEnumerable<ViewCodeSource> result = this.sources.Values;
            if (null != GetAllExcludes) {
                result = result.Where(r => null == GetAllExcludes.FirstOrDefault(x => r.Key.like(x)));
            }
            return result.ToArray();
        }

        public bool Exists(string key) {
            if(invalid)setup();
            key = adaptKey(key);
            return this.sources.ContainsKey(key);
        }

        public bool IsValid(string  key,DateTime timetocheck) {
            if (invalid) setup();
            key = adaptKey(key);
            if(!Exists(key))throw new Exception("view "+key+" not exists");
            return sources[key].LastModified <= timetocheck;
        }

        private void setup() {
            lock(this) {
                var statics = sources.Values.Where(x => x.Level == -10).ToArray();
                sources.Clear();
                var dirs = getDirectoriesOfViews();
                indexFiles(dirs);
                prepareListeners(dirs);
                foreach (var staticview in statics) {
                    sources[staticview.Key] = staticview;
                }
                invalid = false;
            }

        }

        public ViewCodeSource GetFullInfo(string key) {
            if (invalid) setup();
            key = adaptKey(key);
            if(!Exists(key))throw new Exception("view "+key+" not exists");
            return sources[key].Copy();
        }

        private string adaptKey(string key) {
            if(!key.StartsWith("/")) key = "/" + key;
            return key;
        }

        public void SetStatic(string key, string code) {
            if (invalid) setup();
            key = adaptKey(key);
            this.sources[key] = new ViewCodeSource {Key = key, DirectContent = code,Level = -10};
        }

        private void prepareListeners(string[] dirs) {
            foreach (var watcher in watchers) {
                watcher.EnableRaisingEvents = false;
            }
            watchers.Clear();
            var level = 1;
            foreach (var dir in dirs) {
                var watcher = new LeveledFileWatcher();
                watcher.Path = dir;
                watcher.Filter = "*"+Extension;
                watcher.Level = level;
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Created += watcher_Changed;
                watcher.Changed += watcher_Changed;
                level++;
                watchers.Add(watcher);
                
            }
            foreach (var watcher in watchers) {
                watcher.EnableRaisingEvents = true;
            }
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e) {
            var watcher = (LeveledFileWatcher) sender;
            var filename = e.FullPath.normalizePath();
            var key = getKey(watcher.Path, filename);
            var source = new ViewCodeSource() {Key = key, FileName = filename, Level = watcher.Level};
            bool affected = false;
            var existed = sources.get(key);
            if(null==existed || (source.Level<=existed.Level && existed.LastModified<source.LastModified)) {
                affected = true;
                sources[source.Key] = source;
            }
            if(null==existed || existed.LastModified<source.LastModified) {
                if (null != Changed)
                {
                    Changed(this, source, affected);
                }
            }
            
        }

        private void indexFiles(string[] dirs) {
            int level = 1;
            foreach (var dir in dirs) {
                var files = Directory.GetFiles(dir, "*"+Extension, SearchOption.AllDirectories);
                foreach (var file in files) {
                    var f = file.normalizePath().ToLower();
                    var key = getKey(dir, f);
                    
                    if(!sources.ContainsKey(key)) {
                        var source = new ViewCodeSource {Key = key, FileName = f, Level = level,PreprocessFactory = this.PreprocessFactory};
                        sources[key] = source;
                    }
                }
                level++;
            }
        }

        protected abstract string getDefaultExtension();

        private string getKey(string dir, string file) {
            dir = dir.normalizePath().ToLower();
            file = file.normalizePath().ToLower();
            var key = ("/" + file.Replace(dir, "")).normalizePath().Replace(Extension,"");
            return key.ToLower();
        }

        private string[] getDirectoriesOfViews() {
            var result = new List<string>();

            string[] probes = getProbePaths();

            foreach (var probe in probes) {
                var dir = FileSystem.Resolve(probe, true);
                if(null!=dir)result.Add(dir);
            }
            return result.ToArray();
        }

        protected abstract string[] getProbePaths();

        public string ResolveSubView(string parentkey, string subviewkey) {
            parentkey = adaptKey(parentkey);
            if (subviewkey.StartsWith("/")) return subviewkey;
            var root = Path.GetDirectoryName(parentkey).normalizePath();
            return root + subviewkey;
        }

        public string GetCode(string key) {
            if (invalid) setup();
            key = adaptKey(key);
            if(!Exists(key))throw new Exception("no such view "+key);
            return sources[key].GetContent();
        }

        public string GetCode(string parent, string subview) {
            return GetCode(ResolveSubView(parent, subview));
        }

        public void Dispose() {
            foreach (FileSystemWatcher watcher in watchers) {
                watcher.EnableRaisingEvents = false;

            }
            watchers.Clear();
        }

        public event Action<IViewSourceResolver, ViewCodeSource,bool> Changed;
    }
}