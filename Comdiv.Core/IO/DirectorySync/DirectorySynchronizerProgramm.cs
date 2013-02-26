using System;
using System.Collections.Generic;

namespace Comdiv.IO {
    public class DirectorySynchronizerProgramm : List<DirectorySynchronizerTask> {
        public void Execute(Action<string> writelog = null) {
            writelog = writelog ?? (s => {});
            foreach (var task in this) {
                task.Execute(writelog);
            }
        }
    }
}