using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Application {
    public class LongTaskList : List<LongTask> {
        new public void Add(LongTask task) {
            lock (this) {
                if (task.AutoSupress) {
                    foreach (var oldtask in this) {
                        if (oldtask.UniqueString == task.UniqueString && !oldtask.Terminated) {
                            oldtask.HaveToTerminate = true;
                        }
                    }
                }
                base.Add(task);
                longtasktime.Add(DateTime.Now);
            }
        }
        public LongTaskQueueController GetQueueController()
        {
            return new LongTaskQueueController(this);
        }
        IList<DateTime> longtasktime = new List<DateTime>();
        public int CountTasks(int minutes) {
            return longtasktime.Where(x => x.AddMinutes(minutes) > DateTime.Now).Count();
        }

        public int CountTasks(DateTime since)
        {
			lock(this) {
				return longtasktime.ToArray().Where(x => x > since).Count();
			}
        }

    }
}