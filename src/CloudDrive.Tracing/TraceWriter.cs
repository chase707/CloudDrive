using ZeroMQ;

namespace CloudDrive.Tracing
{
    public class TraceWriter
    {
        ZmqSocket Socket;

        public TraceWriter(ZmqContext zmqContext, string tracerAddress)
        {
            Socket = zmqContext.CreateSocket(SocketType.PUSH);
            Socket.Bind(tracerAddress);
        }
        
        public void Trace(string format, params object[] parameters)
        {
            Socket.Send(string.Format(format, parameters), System.Text.Encoding.ASCII);
        }
    }
}
