using Comdiv.FluentView;

namespace Comdiv.MVC.Preprocess{
    public class FluentImportPreprocessor:RegexBasedBrailPreprocessor{
        public FluentImportPreprocessor(){
            Pattern = @"\#fluent";
            Replace = "import "+typeof (div).Namespace;
        }
    }

    public class OutputSubViewPreprocessor_INCODE:RegexBasedBrailPreprocessor{
        public OutputSubViewPreprocessor_INCODE()
        {
            Pattern = @"\*\*\*(\w+)(\{([^\}]+)})?";
            Replace = @"OutputSubView('$1',{$3})";
            Idx = 10;
        }
    }
    public class OutputSubViewPreprocessor_INHTML : RegexBasedBrailPreprocessor
    {
        public OutputSubViewPreprocessor_INHTML()
        {
            Pattern = @"\*\*(\w+)(\{([^\}]+)})?";
            Replace = @"<%OutputSubView('$1',{$3})%>";
            Idx = 20;
        }
    }
}