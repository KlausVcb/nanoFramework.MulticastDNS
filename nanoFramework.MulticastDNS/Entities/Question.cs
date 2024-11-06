using nanoFramework.MulticastDNS.Enum;
using nanoFramework.MulticastDNS.Package;

namespace nanoFramework.MulticastDNS.Entities
{
    public class Question
    {
        public string Domain { get; }
        public DnsResourceType QueryType { get; }
        public ushort QueryClass { get; }

        public Question(string domain, DnsResourceType queryType, ushort queryClass)
        {
            Domain = domain;
            QueryType = queryType;
            QueryClass = queryClass;
        }

        public byte[] GetBytes()
        {
            PacketBuilder packet = new();
            packet.Add(Domain);
            packet.Add((ushort)QueryType);
            packet.Add(QueryClass);
            return packet.GetBytes();
        }
    }
}
