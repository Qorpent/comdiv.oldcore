using System;
using System.Collections;
using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility.Brail
{
    public class ViewEngineOptions
    {
        private readonly IList assembliesToReference = new ArrayList();
        private readonly IList namespacesToImport = new ArrayList();
        private bool batchCompile;
        private string commonScriptsDirectory = "CommonScripts";
        private bool debug;
        private string saveDirectory = "Brail_Generated_Code";
        private bool saveToDisk;
        private bool _saveBooResult = false;
        private string _saveBooResultDirectory = "~/tmp";
        private IList<IBrailPreprocessor> _preprocessors = new List<IBrailPreprocessor>();
        private string baseType;

        public ViewEngineOptions()
        {
            AssembliesToReference.Add(typeof(System.Linq.Enumerable).Assembly);
            NamespacesToImport.Add("System");
            NamespacesToImport.Add("System.Collections");
            NamespacesToImport.Add("System.Collections.Generic");
            namespacesToImport.Add("System.Linq");
            namespacesToImport.Add("System.Linq.Enumerable");
            namespacesToImport.Add("Comdiv.Extensibility");
            namespacesToImport.Add("Comdiv.Extensibility.Brail");

        }

        public IList<IBrailPreprocessor> Preprocessors
        {
            get { return _preprocessors; }
            private set { _preprocessors = value; }
        }

        public string BaseType
        {
            get { return baseType; }
            set { baseType = value; }
        }

        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }

        public bool CollectStatistics { get; set; }
        public string SavePreprocessedDirectory { get; set; }

        public bool SaveToDisk
        {
            get { return saveToDisk; }
            set { saveToDisk = value; }
        }

        public bool BatchCompile
        {
            get { return batchCompile; }
            set { batchCompile = value; }
        }

        public string CommonScriptsDirectory
        {
            get { return commonScriptsDirectory; }
            set { commonScriptsDirectory = value; }
        }

        public string SaveDirectory
        {
            get { return saveDirectory; }
            set { saveDirectory = value; }
        }

        public IList AssembliesToReference
        {
            get { return assembliesToReference; }
        }

        public IList NamespacesToImport
        {
            get { return namespacesToImport; }
        }

        public bool SaveBooResult
        {
            get { return _saveBooResult; }
            set { _saveBooResult = value; }
        }

        public string SaveBooResultDirectory
        {
            get { return _saveBooResultDirectory; }
            set { _saveBooResultDirectory = value; }
        }

        public string ViewEngineType { get; set; }
    }
}