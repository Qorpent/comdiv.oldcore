namespace Comdiv.MAS {
    public class CanRunRecord {
        public CanRunRecord() {
            this.CanRun = true;
            this.Description = "";
        }
        public bool CanRun { get; set; }
        public string Description { get; set; }
    }
}