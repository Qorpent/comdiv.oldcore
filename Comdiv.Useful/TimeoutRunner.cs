using System;
using System.Threading;

namespace Comdiv.Useful{
    public class TimeoutRun {
        private int timeout;
        private ThreadStart action;

        public TimeoutRun(int timeout, ThreadStart action){
            this.timeout = timeout;
            this.action = action;
        }

        public void Run(){
            if (0 >= timeout) action();
            else{
                var finished = false;
                var thread = new Thread(() =>
                                            {
                                                action();
                                                finished = true;
                                            });
                thread.Start();
                thread.Join(timeout);
                if (!finished) throw new TimeoutException();
            }
        }
    }
}