using System;

namespace CloudDrive.Tracing
{
    public class ConsoleTracer : ITracer
    {
        protected TraceWatcher Tracer;
        public ConsoleTracer(TraceWatcher traceWatcher)
        {
            Tracer = traceWatcher;
            Tracer.TraceReceived += Tracer_TraceReceived;
            Tracer.Start();
        }

        public void Tracer_TraceReceived(object sender, string message)
        {
			try
			{
				Console.WriteLine("[{0}] - {1}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.ff"), message);
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("Trace Exception: {0}", ex.ToString()));
			}
        }
    }
}
