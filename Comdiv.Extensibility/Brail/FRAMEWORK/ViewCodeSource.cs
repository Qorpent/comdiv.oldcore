using System;
using System.IO;
using Comdiv.Extensions;

namespace Comdiv.Extensibility.Brail {
    public class ViewCodeSource {
        public string Key { get; set; }
        public int Level { get; set; }
        private string _fileName;
		public IBrailPreprocessorFactory PreprocessFactory { get; set; }
        public string FileName {
            get { return _fileName; }
            set {
                _fileName = value;
                if (_fileName.hasContent()) {
                    DirectContent = "";
                    LastModified = File.GetLastWriteTime(_fileName);
                }
            }
        }

        public DateTime LastModified { get; set; }
        public string GetContent() {
            if(FileName.hasContent()) {
                var result = File.ReadAllText(FileName);
				if(null!=PreprocessFactory) {
					var preprocessors = PreprocessFactory.GetPreprocessors();
					foreach (var p in preprocessors) {
						result = p.Preprocess(result);
					}
				}
            	return result;
            }else {
                return DirectContent;
            }
               
        }

        private string _directContent;
        public string DirectContent {
            get { return _directContent; }
            set {
                if(value.hasContent()) {
                    FileName = "";
                    
                }
                _directContent = value;
                LastModified = DateTime.Now;
            }
        }
        public bool IsValid(DateTime last) {
            return last >= LastModified;
        }

        public ViewCodeSource Copy() {
            return (ViewCodeSource) MemberwiseClone();
        }
    }
}