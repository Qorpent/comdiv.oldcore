namespace Comdiv.Extensibility.Brail{
    public abstract class BrailPreprocessorBase : IBrailPreprocessor{
        #region IBrailPreprocessor Members

        public int Idx { get; set; }
        public abstract string Preprocess(string code);

        #endregion
    }
}