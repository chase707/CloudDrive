using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDrive.Tracing
{
    public class TraceWatcher
    {
        public TraceReader Tracer { get; set; }
        private bool Running;
        private Task RunTask;
        private object LockObj = new object();
        public TraceWatcher(TraceReader traceConsumer)
        {
            Tracer = traceConsumer;
        }

        public void Start()
        {
            lock (LockObj)
            {
                if (!Running)
                {
                    Running = true;

                    RunTask = Task.Run(() => Run());
                }
            }
        }

        public void Stop()
        {
            lock (LockObj)
            {
                Running = false;
            }
        }

        public delegate void TraceReceivedDelegate(object sender, string message);
        public TraceReceivedDelegate TraceReceived;

        private void Run()
        {
			try
			{
				while (Running)
				{
					var message = Tracer.Receive();
					if (!string.IsNullOrEmpty(message))
					{
						if (TraceReceived != null)
							TraceReceived(this, message);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(String.Format("Trace Exception: {0}", ex.ToString()));
			}
        }
    }
}
