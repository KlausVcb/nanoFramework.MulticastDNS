using nanoFramework.MulticastDNS.Entities;
using nanoFramework.MulticastDNS.EventArgs;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace nanoFramework.MulticastDNS
{
    public class MulticastDNSService : IDisposable
    {
        private const string MulticastDnsAddress = "224.0.0.251";
        private const int MulticastDnsPort = 5353;

        bool _listening = false;

        public void Start()
        {
            if (!_listening)
            {
                _listening = true;
                new Thread(Run).Start();
            }
        }

        public void Stop() => _listening = false;

        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

        public event MessageReceivedEventHandler MessageReceived;

        private void Run()
        {
            var multicastAddress = IPAddress.Parse(MulticastDnsAddress);

            var client = new UdpClient(new IPEndPoint(IPAddress.Any, MulticastDnsPort));
            client.JoinMulticastGroup(multicastAddress);

            IPEndPoint multicastEndpoint = new(multicastAddress, MulticastDnsPort);
            IPEndPoint remoteEndpoint = new(IPAddress.Any, 0);

            var buffer = new byte[2048];

            while (_listening)
            {
                int length = client.Receive(buffer, ref remoteEndpoint);
                if (length == 0) continue;

                var msg = new Message(buffer);

                if (msg != null)
                {
                    var eventArgs = new MessageReceivedEventArgs(msg);

                    MessageReceived?.Invoke(this, eventArgs);

                    if(eventArgs.Response != null)
                        client.Send(eventArgs.Response.GetBytes(), multicastEndpoint);
                }
            }

            client.DropMulticastGroup(multicastAddress);
            client.Dispose();
        }

        public void Dispose() => Stop();
    }
}
