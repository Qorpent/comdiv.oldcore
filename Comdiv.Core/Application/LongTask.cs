using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Comdiv.Application {
    public  class LongTask {
        private static int id;
        public LongTask() {
            Counters=new Dictionary<string, decimal>();
            States = new Dictionary<string, string>();
            Phases = new Dictionary<string, TimeSpan>();
            Reports = new Dictionary<string, string>();
            this.Id = id++;
        }

        public bool WaitQueue { get; set; }
        public string Queue { get; set; }
        public decimal QueueWeight { get; set; }
        public decimal QueueLimit { get; set; }
        public int QueueWaitRate { get; set; }

        protected Dictionary<string, string> Reports { get; set; }

        public void Inc(string counter, decimal value = 1) {
            if(!Counters.ContainsKey(counter)) {
                Counters[counter] = value;
            }else {
                Counters[counter] += value;
            }
        }
        public void Set(string state, string  value) {
            States[state] = value;
        }

        public HttpContext Context { get; set; }

        public int Id { get; set; }
        public string Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public string Comment { get; set; }
        public string User { get; set; }
        public override string ToString() {
            return string.Format("{0}/{1}/{2}/{3}/{4}", User, DateTime.Now - Start, Comment,HaveToTerminate,Terminated);
        }

		public string ToFullString() {
			var sb = new StringBuilder();
			sb.AppendLine(this.ToString());
			foreach (var counter in this.Counters) {
				sb.AppendLine(string.Format("{0}: {1}", counter.Key, counter.Value.ToString("0.##")));
			}
			foreach (var state in this.States)
			{
				sb.AppendLine(string.Format("{0}: {1}", state.Key, state.Value));
			}
			foreach (var phase in this.Phases) {
				sb.AppendLine(string.Format("{0}: {1}", phase.Key, phase.Value));	
			}

			return sb.ToString();
		}

        public TimeSpan Time {
            get {
                if(Finish.Year<=1900) {
                    return DateTime.Now - Start;
                }
                return Finish - Start;
            }
        }


        public bool HaveToTerminate { get; set; }
        public bool Terminated { get; set; }
        public IDictionary<string, decimal > Counters { get; set; }
        public IDictionary<string, string> States { get; set; }
        public IDictionary<string, TimeSpan> Phases { get; set; }

        public string UniqueString { get; set; }

        public bool AutoSupress { get; set; }

        private DateTime lastphase;
        

        public void PhaseFinished(string phase) {
            if (lastphase.Year < 1900) lastphase = Start;
            var time = DateTime.Now - lastphase;
            Phases[phase] = time;
            lastphase = DateTime.Now;
        }
        public void Terminate() {
            this.Finish = DateTime.Now;
            Terminated = true;
        }

        public void Append(string state, string append) {
            if(!States.ContainsKey(state)) {
                States[state] = append;
            }else {
                States[state] += append;
            }
        }
    }
}