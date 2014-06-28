using System;
using System.IO;
using ZeroMQ;

namespace CloudDrive.Tracing
{
    public class TraceReader
    {
        ZmqSocket Socket;

        public TraceReader(ZmqContext zmqContext, string tracerAddress)
        {
            Socket = zmqContext.CreateSocket(SocketType.PULL);
            Socket.Connect(tracerAddress);
        }

        public string Receive()
        {
            var traceMessage = Socket.ReceiveMessage();

            try
            {
                using (var messageStream = new MemoryStream(traceMessage[0]))
                using (var textReader = new StreamReader(messageStream))
                    return textReader.ReadToEnd();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
