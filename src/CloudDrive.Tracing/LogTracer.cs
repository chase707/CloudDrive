using System;
using System.IO;

namespace CloudDrive.Tracing
{
    public class LogTracer : ITracer, IDisposable
    {
        protected TraceWatcher Tracer;
        protected StreamWriter FileStream;
        public LogTracer(TraceWatcher traceWatcher, string logFilename)
        {
            Tracer = traceWatcher;
            Tracer.TraceReceived += Tracer_TraceReceived;

            FileStream = new StreamWriter(File.Open(logFilename, FileMode.Append, FileAccess.Write, FileShare.Read));
            FileStream.AutoFlush = true;
            
            Tracer.Start();
        }

        public void Tracer_TraceReceived(object sender, string message)
        {
			try
			{
				FileStream.WriteLine("[{0}] - {1}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.ff"), message);            
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("Trace Exception: {0}", ex.ToString()));
			}
        }

        public void Dispose()
        {
            FileStream.Flush();
            FileStream.Dispose();
        }
    }
}
