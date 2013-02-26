using System;
using System.Threading;
using Comdiv.Extensions;

namespace Comdiv.Application
{
    public abstract class LockObjectBase:IDisposable {

        protected LockObjectBase(string lockobject, string identity, string system, int wait, int retrycount, int retryrate, bool emmidiatly, bool throws)
        {
            if(lockobject.noContent()) {
                throw new ArgumentException("need lockobject",lockobject);
            }
            if(system.noContent()) {
                system = "Default";
            }
            if(identity.noContent()) {
                identity = Guid.NewGuid().ToString();
            }
            if(emmidiatly) {
                throws = true;
            }
            if(retrycount==0) {
                wait = 0;
                retryrate = 0;
            }else {
                if(wait==-1) {
                    wait = 1000*60*30; //default total wait is seted to 30 min
                }
                if(retryrate==-1) {
                    retryrate = 1000*5; //default retry rate is set to 5 sec
                }
                if(retrycount==-1) {
                    retrycount = wait/retryrate;
                }else {
                    retryrate = wait/retrycount;
                }
            }

            this.LockObject = lockobject;
            this.System = system;
            this.Identity = identity;
            this.ThrowError = throws;
            this.TotalWaitTime = wait;
            this.RetryCount = retrycount;
            this.RetryRate = retryrate;
            if(emmidiatly) {
                var result = this.Get();
                if(!result) {
                    throw  new Exception("cannot get lock on "+lockobject);
                }
            }
        }


        public bool Get() {
            int c = 0;
            while (c<=this.RetryCount) {
                var result = NativeGet();
                if(result) return true;
                if (c == this.RetryCount) return false;
                Thread.Sleep(RetryRate);
                c++;
            }
            return false;
        }

        public void Release() {
            NativeRelease();
        }

        public abstract bool NativeGet();
        public abstract bool NativeRelease();


        public bool ThrowError { get; private set; }
        public string LockObject { get; private set; }
        public string Identity { get; private set; }
        public string System { get; private set; }
        public int TotalWaitTime { get; private set; }
        public int RetryCount { get; private set; }
        public int RetryRate { get; private set; }
        public void Dispose() {
            Release();
        }
    }
}
