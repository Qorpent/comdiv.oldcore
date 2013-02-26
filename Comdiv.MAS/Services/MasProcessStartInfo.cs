namespace Comdiv.MAS {
    public class MasProcessStartInfo {
        public string Name { get; set;}
        public string Code { get; set; }
        public string Args { get; set; }
        public string FileName { get; set; }
        public System.Diagnostics.Process NativeProcess { get; set; }
        public MasProcessStartInfo Copy() {
            return (MasProcessStartInfo)this.MemberwiseClone();
        }
    }
}