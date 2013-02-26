namespace Comdiv.IO {
    public class DirectorySynchronizerBehaviour {
        public DirectorySynchronizerBehaviour() {
            UpdateOldFiles = true;
            CreateNewFiles = true;
        }
        public bool Emulate { get; set; }
        public bool UpdateOldFiles { get; set; }
        public bool RewriteNewFiles { get; set; }
        public bool DeleteFiles { get; set;}
        public bool CreateNewFiles { get; set; }
    }
}