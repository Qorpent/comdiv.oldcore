using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Comdiv.Extensions;

namespace Comdiv.IO
{
    public class DirectorySynchronizer
    {
        private string source;
        private string target;
        private Dictionary<string, FileInfo> sourcefiles;
        private Dictionary<string, FileInfo> targetfiles;

        public DirectorySynchronizer() : this(null, null) {
        }

        public DirectorySynchronizer(string source,string  target) {
            ExcludePattern = "";
            Source = source;
            Target = target;
            Behaviour = new DirectorySynchronizerBehaviour();
        }
        public string Source { get; set; }
        public string Target { get; set; }
        public string ExcludePattern { get; set; }
        public DirectorySynchronizerBehaviour Behaviour { get; private set; }
        public DirectorySynchronizerProgramm Prepare(Action<string> writelog = null) {
            if(Source.noContent()) throw new InvalidOperationException("source is empty");
            if(Target.noContent()) throw new InvalidOperationException("target is empty");
            writelog = writelog ?? (s => {});
            source = Source.normalizePath();
            target = Target.normalizePath();
            if(!Directory.Exists(source)) throw new InvalidOperationException("source directory not exists");
            if(!Directory.Exists(target)) Directory.CreateDirectory(target);
            sourcefiles = getFileDictionary(source);
            targetfiles = getFileDictionary(target);
            var result = new DirectorySynchronizerProgramm();
            if(this.Behaviour.CreateNewFiles && !this.Behaviour.RewriteNewFiles) {
                collectNewFiles(result, writelog);
            }
            if(this.Behaviour.UpdateOldFiles && !this.Behaviour.RewriteNewFiles) {
                collectUpdates(result, writelog);
            }
            if(this.Behaviour.DeleteFiles) {
                collectDeletes(result, writelog);
            }
            if(this.Behaviour.RewriteNewFiles) {
                collectRewrites(result, writelog);
            }
            foreach (var task in result) {
                task.Emulate = Behaviour.Emulate;
            }
            return result;
        }

        private string adaptSourceNameToTarget(string sourcename) {
            var srclocal = sourcename.Replace(source, "");
            if(srclocal.StartsWith("/")) srclocal = srclocal.Substring(1);
            var result = Path.Combine(target, srclocal).normalizePath();
            return result;
        }
        private string adaptTargetNameToSource(string trgname) {
            var trglocal = trgname.Replace(target, "");
            if(trglocal.StartsWith("/")) trglocal = trglocal.Substring(1);
            var result = Path.Combine(source, trglocal).normalizePath();
            return result;
        }

        private void collectRewrites(DirectorySynchronizerProgramm result, Action<string> writelog) {
            foreach (var sf in sourcefiles) {
                var tf = adaptSourceNameToTarget(sf.Key);
                var task = new DirectorySynchronizerTask
                               {
                                   Command = DirectorySynchronizerCommandType.Create,
                                   Source = sf.Key,
                                   Target = tf
                               };
                result.Add(task);
            }
        }

        private void collectDeletes(DirectorySynchronizerProgramm result, Action<string> writelog) {
            foreach (var tf in targetfiles) {
                var sf = adaptTargetNameToSource(tf.Key);
                if(!sourcefiles.ContainsKey(sf)) {
                    var task = new DirectorySynchronizerTask
                                   {
                                       Command = DirectorySynchronizerCommandType.Delete,
                                       Source = tf.Key,
                                       Target = tf.Key,
                                   };
                    result.Add(task);
                }
            }
        }

        private void collectUpdates(DirectorySynchronizerProgramm result, Action<string> writelog) {
            foreach (var sf in sourcefiles) {
                var tf = adaptSourceNameToTarget(sf.Key);
                if(targetfiles.ContainsKey(tf)) {
                    if(targetfiles[tf].LastWriteTime < sf.Value.LastWriteTime) {
                        var task = new DirectorySynchronizerTask
                                       {
                                           Command = DirectorySynchronizerCommandType.Update,
                                           Source = sf.Key,
                                           Target = tf
                                       };
                        result.Add(task);
                    }
                }
            }
        }

        private void collectNewFiles(DirectorySynchronizerProgramm result, Action<string> writelog) {
            foreach (var sf in sourcefiles) {
                var tf = adaptSourceNameToTarget(sf.Key);
                if(!targetfiles.ContainsKey(tf)) {

                        var task = new DirectorySynchronizerTask
                                       {
                                           Command = DirectorySynchronizerCommandType.Create,
                                           Source = sf.Key,
                                           Target = tf
                                       };
                        result.Add(task);
                }
            }
        }

        private Dictionary<string, FileInfo> getFileDictionary(string dir) {
            var filenames = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            var sourcefiles = new Dictionary<string, FileInfo>();
            foreach (var sourcefilename in filenames) {
                if(0!=(File.GetAttributes(sourcefilename) & (FileAttributes.Hidden | FileAttributes.System))) {
                    continue;
                }

                var f = sourcefilename.normalizePath();
                if(ExcludePattern.hasContent() && f.like(ExcludePattern)) {
                    continue;
                }
                sourcefiles.Add(f,new FileInfo(f));
            }
            return sourcefiles;
        }
    }
}
