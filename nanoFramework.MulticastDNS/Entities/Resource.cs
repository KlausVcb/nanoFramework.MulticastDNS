using nanoFramework.MulticastDNS.Enum;
using nanoFramework.MulticastDNS.Package;
using System.Net;
using System.Text;

namespace nanoFramework.MulticastDNS.Entities
{
    public class Resource
    {
        public Resource(string domain, DnsResourceType type, int ttl) : this(domain, ttl) => ResourceType = type;

        public Resource(string domain, int ttl)
        {
            Domain = domain;
            Ttl = ttl;
        }

        public string Domain { get; }
        public DnsResourceType ResourceType { get; }
        public ushort ResourceClass { get; } = 1; // IN
        public int Ttl { get; set; } = 2000;

        public byte[] GetBytes()
        {
            PacketBuilder packet = new();
            packet.Add(Domain);
            packet.Add((ushort)ResourceType);
            packet.Add(ResourceClass);
            packet.Add(Ttl);

            var data = GetBytesInternal();

            packet.Add((ushort)data.Length);
            packet.Add(data);

            return packet.GetBytes();
        }

        protected virtual byte[] GetBytesInternal() => new byte[0]; 
    }

    public class TargetResource : Resource
    {
        public TargetResource(string domain, DnsResourceType type, int ttl) : base(domain, type, ttl) { }
        public string Target { get; set; }

        protected override byte[] GetBytesInternal()
        {
            var packetBuilder = new PacketBuilder();
            packetBuilder.Add(Target);
            return packetBuilder.GetBytes();
        }
    }

    public class AddressResource : Resource
    {
        public AddressResource(string domain, DnsResourceType type, int ttl) : base(domain, type, ttl) { }
        public IPAddress Address { get; set; }

        protected override byte[] GetBytesInternal() => Address.GetAddressBytes();
    }

    public class A : AddressResource
    { // 1
        public A(string domain, IPAddress address, int ttl = 2000) : base(domain, DnsResourceType.A, ttl) => Address = address;
        internal A(PacketParser packet, string domain, int ttl, int length) : base(domain, DnsResourceType.A, ttl) => Address = new IPAddress(packet.ReadBytes(length));
    }

    public class CNAME : TargetResource
    { // 5
        public CNAME(string domain, string targetDomain, int ttl = 2000) : base(domain, DnsResourceType.CNAME, ttl) => Target = targetDomain;
        internal CNAME(PacketParser packet, string domain, int ttl) : base(domain, DnsResourceType.CNAME, ttl) => Target = packet.ReadDomain();
    }

    public class PTR : TargetResource
    { // 12
        public PTR(string domain, string targetDomain, int ttl = 2000) : base(domain, DnsResourceType.PTR, ttl) => Target = targetDomain;
        internal PTR(PacketParser packet, string domain, int ttl) : base(domain, DnsResourceType.PTR, ttl) => Target = packet.ReadDomain();
    }

    public class TXT : Resource
    { // 16
        public TXT(string domain, string txt, int ttl = 2000) : base(domain, DnsResourceType.TXT, ttl) => Txt = txt;
        internal TXT(PacketParser packet, string domain, int ttl) : base(domain, DnsResourceType.TXT, ttl) => Txt = packet.ReadString();

        public string Txt { get; }

        protected override byte[] GetBytesInternal() => Encoding.UTF8.GetBytes(Txt);
    }

    public class AAAA : AddressResource
    { // 28
        public AAAA(string domain, IPAddress address, int ttl = 2000) : base(domain, DnsResourceType.AAAA, ttl) => Address = address;
        internal AAAA(PacketParser packet, string domain, int ttl, int length) : base(domain, DnsResourceType.AAAA, ttl) => Address = new IPAddress(packet.ReadBytes(length));
    }

    public class SRV : TargetResource
    { // 33
        public SRV(string domain, ushort priority, ushort weight, ushort port, string targetDomain, int ttl = 2000) : base(domain, DnsResourceType.SRV, ttl)
        {
            Priority = priority;
            Weight = weight;
            Port = port;
            Target = targetDomain;
        }

        internal SRV(PacketParser packet, string domain, int ttl) : base(domain, DnsResourceType.SRV, ttl)
        {
            Priority = packet.ReadUShort();
            Weight = packet.ReadUShort();
            Port = packet.ReadUShort();
            Target = packet.ReadDomain();
        }

        public ushort Port { get; set; }
        public ushort Priority { get; set; }
        public ushort Weight { get; set; }

        protected override byte[] GetBytesInternal()
        {
            var packetBuilder = new PacketBuilder();
            packetBuilder.Add(Priority);
            packetBuilder.Add(Weight);
            packetBuilder.Add(Port);
            packetBuilder.Add(Target);
            return packetBuilder.GetBytes();
        }
    }
}
