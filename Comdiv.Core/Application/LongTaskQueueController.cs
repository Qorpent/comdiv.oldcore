using System;
using System.Threading;
using Comdiv.Extensions;

namespace Comdiv.Application {
    ///<summary>
    ///</summary>
    public class LongTaskQueueController {
        private LongTaskList tasks;

        public LongTaskQueueController(LongTaskList tasks) {
            this.tasks = tasks;
        }

        public void EnQueue(LongTask task) {
            this.tasks.Add(task);
            if(task.Queue.noContent())return;
            task.WaitQueue = true;
            try {



                while (getCurrentWeight(task.Queue) > (task.QueueLimit + task.QueueWeight)) {
                    if (task.HaveToTerminate) {
                        task.Terminate();
                        throw new Exception("ожидание очереди прервано");
                    }
                    if (null!=task.Context && null!=task.Context.Response && !task.Context.Response.IsClientConnected) {
                        task.Terminate();
                        throw new Exception("ожидание очереди прервано");
                    }

                    Thread.Sleep(task.QueueWaitRate);
                }
            }
            finally {
                task.WaitQueue = false;
            }
            
        }

        private decimal getCurrentWeight(string queue) {
            decimal result = 0;
            foreach (var task in tasks) {
                if(task.Terminated) continue;
                if(task.Queue!=queue) continue;
                result += task.QueueWeight;
            }
            return result;
        }
    }
}