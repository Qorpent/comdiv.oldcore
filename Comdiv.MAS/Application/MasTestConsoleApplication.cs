using System.Threading;

namespace Comdiv.MAS {
    public class MasTestConsoleApplication : MasConsoleApplication {
        //just procees events
        protected override void execute() {
            while(!ForceQuit) {
                Thread.Sleep(500);
            }
        }
    }
}