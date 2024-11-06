using nanoFramework.MulticastDNS.Entities;
using nanoFramework.MulticastDNS.Enum;
using nanoFramework.MulticastDNS.EventArgs;
using nanoFramework.Networking;
using System;
using System.Net;
using System.Threading;

namespace nanoFramework.MulticastDNS.Sample
{
    public class Program
    {
        public static void Main()
        {
            //replace with your wifi ssid/pwd
            const string ssid = "...";
            const string pwd = "...";

            WifiNetworkHelper.ConnectDhcp(ssid, pwd);

            MulticastDNSService multicastDNSService = new();
            multicastDNSService.MessageReceived += MulticastDNSService_MessageReceived;

            multicastDNSService.Start();

            Thread.Sleep((int)TimeSpan.FromSeconds(30).TotalMilliseconds);

            multicastDNSService.Stop();
        }

        private static void MulticastDNSService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            const string deviceName = "nanodevice.local";
            const string ipaddress = "192.168.1.1";

            if (e.Message != null)
                foreach (var question in e.Message.GetQuestions())
                {
                    if (question.QueryType == DnsResourceType.A && question.Domain == deviceName)
                    {
                        var response = new Response();
                        response.AddAnswer(new A(question.Domain, IPAddress.Parse(ipaddress)));
                        e.Response = response;
                    }
                }
        }
    }
}
