using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.IO;
using FilePathResolverExtensions = Comdiv.IO.FilePathResolverExtensions;

namespace Comdiv.Wiki {
    ///<summary>
    ///</summary>
    public class WikiPersistenceProvider : IWikiPersistenceProvider {

        public WikiPersistenceProvider() {
            myapp.OnReload += new Common.EventWithDataHandler<object, int>(myapp_OnReload);
        }

        void myapp_OnReload(object sender, Common.EventWithDataArgs<int> args) {
            Reload(true);
        }
        IDictionary<string,WikiPage> cache = null;


        public WikiPage Load(string code) {
            lock (this) {
                Reload(false);
                if(cache.ContainsKey(code)) {
                    return cache[code];
                }
                return null;
            }
        }

        private WikiPage internalLoad(string code) {
            var path = getPath(code);
            if (myapp.files.Exists(path)) {
                
                var content = myapp.files.Read(path);
                IDictionary<string, string> tags = extractTags(content);
                var result = new WikiPage();
                result.Code = code;
                result.Level = IoExtensions.GetLevel(FilePathResolverExtensions.Resolve(myapp.files, path));
                result.Content = content;
                result.FilePath = myapp.files.Resolve(path, true);
                result.LastWriteTime = File.GetLastWriteTime(result.FilePath);
                foreach (var t in tags) {
                    result.Properties[t.Key] = t.Value;
                }
                return result;
            }
            return null;
        }

        public void Reload(bool full,string code = null) {
            if(null==cache) {
                cache = new Dictionary<string, WikiPage>();
                full = true;
            }
            if(full) {
                rereadFiles(code);
            }
        }

        private void rereadFiles(string code = null) {
            
            foreach (var key in cache.Keys.Where(x=>code.noContent() || x==code).ToArray()) {
                var page = cache[key];
                if(File.Exists(page.FilePath)) {
                    if(File.GetLastWriteTime(page.FilePath)>page.LastWriteTime) {
                        cache[key] = internalLoad(key);
                    }
                }else {
                    cache.Remove(key);
                }
            }
            var files = myapp.files.ResolveAll("wiki/", string.Format("{0}.html",code.hasContent()?code:"*"), true, true);
            
            foreach (var file in files) {

                var key = file.replace(@"^[\s\S]+?/wiki/", "");
                key = key.Replace(".html", "");
                if(!cache.ContainsKey(key)) {
                    cache[key] = internalLoad(key);
                }
            }

            files = myapp.files.ResolveAll("~/extensions/", "*.wiki.html", true, true);
            foreach (var file in files) {

                var key = file.replace(@"^[\s\S]+?extensions/", "-");
                key = key.Replace(".wiki.html", "");
                if (code.noContent() || code == key) {
                    if (!cache.ContainsKey(key)) {
                        cache[key] = internalLoad(key);
                    }
                }
            }
        }

        private IDictionary<string, string> extractTags(string content) {
            var tag = content.find(@"\<\!--@@(?<tag>[\s\S]*?)--\>", "tag");
            return TagHelper.Parse(tag);
        }

        private string getPath(string code) {
            if (code.StartsWith("-")) {
                return "~/extensions/" + code.Substring(1) + ".wiki.html";
            }
            else {
                return "wiki/" + code + ".html";
            }
        }

        public bool Exists(string code) {
            Reload(false);
            return cache.ContainsKey(code);
        }


        public WikiPage[] Search(string  mask) {
            Reload(false);
            var result = cache.Where(x => mask.noContent() || x.Key.like(mask)).Select(x => x.Value).ToArray();
            return result;
        }

        public void Write(WikiPage page) {
            Reload(false);
            var path = getWritePath(page);
            
            var existedtags = extractTags(page.Content);
            foreach (var existedtag in existedtags) {
               // if(!page.Properties.ContainsKey(existedtag.Key)) {
                    page.Properties[existedtag.Key] = existedtag.Value;
                //}
            }
            if(!page.Content.Contains("<!--@@")) {
                page.Content = "<!--@@ -->"+ page.Content;
            }
            page.Content = page.Content.replace(@"\<\!--@@[\s\S]*? --\>",
                                                "<!--@@ \r\n" + TagHelper.ToString(page.Properties).Replace("/ /","/\r\n/") + "\r\n -->\r\n");
            myapp.files.Write(path, page.Content);
            var filepath = myapp.files.Resolve(path, true);
            page.FilePath = filepath;
            page.LastWriteTime = DateTime.Now;
            cache[page.Code] = page;
        }

        private string getWritePath(WikiPage page) {
            if(page.Code.StartsWith("-")) {
                return "~/extensions/" + page.Code.Substring(1) + ".wiki.html";
            }
            return "~/" + page.Level + "/wiki/" + page.Code + ".html";
        }
    }
}