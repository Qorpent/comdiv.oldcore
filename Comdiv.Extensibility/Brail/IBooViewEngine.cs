using System.Collections.Generic;

namespace Comdiv.Extensibility.Brail {
    public interface IBooViewEngine {
        IDictionary<string, string> OutputCache { get; }
        bool ConditionalPreProcessingOnly(string name);
        string ResolveSubViewName(string parentfile, string subviewname);

        /// <summary>
        /// This takes a filename and return an instance of the view ready to be used.
        /// If the file does not exist, an exception is raised
        /// The cache is checked to see if the file has already been compiled, and it had been
        /// a check is made to see that the compiled instance is newer then the file's modification date.
        /// If the file has not been compiled, or the version on disk is newer than the one in memory, a new
        /// version is compiled.
        /// Finally, an instance is created and returned	
        /// </summary>
        BrailBaseCommon GetCompiledScriptInstance(string filename);
    }
}