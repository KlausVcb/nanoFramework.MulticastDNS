using nanoFramework.MulticastDNS.Enum;
using nanoFramework.MulticastDNS.Package;
using System.Collections;
using System.Diagnostics;

namespace nanoFramework.MulticastDNS.Entities
{
    public class Message
    {
        private static System.Random generator = new();

        protected ushort id;
        protected ushort flags = 0;

        protected ArrayList questions = new();
        protected ArrayList answers = new();
        protected ArrayList servers = new();
        protected ArrayList additionals = new();

        public Message() => id = (ushort)generator.Next(1 << 16);

        public Message(byte[] data) => ParseData(data);

        public Question[] GetQuestions() => (Question[])questions.ToArray(typeof(Question));

        public Resource[] GetResources()
        {
            ArrayList resources = new();
            resources.AddRange(answers);
            resources.AddRange(servers);
            resources.AddRange(additionals);
            return (Resource[])resources.ToArray(typeof(Resource));
        }

        public byte[] GetBytes()
        {
            PacketBuilder packet = new();
            packet.Add(id);
            packet.Add(flags);
            packet.Add((ushort)questions.Count);
            packet.Add((ushort)answers.Count);
            packet.Add((ushort)servers.Count);
            packet.Add((ushort)additionals.Count);

            foreach (Question query in questions)
                packet.Add(query.GetBytes());

            foreach (Resource resource in answers)
                packet.Add(resource.GetBytes());

            foreach (Resource resource in servers)
                packet.Add(resource.GetBytes());

            foreach (Resource resource in additionals)
                packet.Add(resource.GetBytes());

            return packet.GetBytes();
        }

        private void ParseData(byte[] data)
        {
            PacketParser packet = new(data);

            id = packet.ReadUShort();
            flags = packet.ReadUShort();
            ushort question_count = packet.ReadUShort();
            ushort answer_count = packet.ReadUShort();
            ushort server_count = packet.ReadUShort();
            ushort additional_count = packet.ReadUShort();

            for (int i = 0; i < question_count; ++i)
            {
                string domain = packet.ReadDomain();
                DnsResourceType rr_type = GetResourType(packet.ReadUShort());
                ushort rr_class = packet.ReadUShort();
                Debug.WriteLine($"{domain} - {rr_type} - {rr_class}");
                if (rr_type > 0) questions.Add(new Question(domain, rr_type, rr_class));
            }

            for (int i = 0; i < answer_count; ++i)
                answers.Add(ParseResource(packet));

            for (int i = 0; i < server_count; ++i)
                servers.Add(ParseResource(packet));

            for (int i = 0; i < additional_count; ++i)
                additionals.Add(ParseResource(packet));
        }

        private Resource ParseResource(PacketParser packet)
        {
            string domain = packet.ReadDomain();
            ushort rr_type = packet.ReadUShort();
            ushort rr_class = packet.ReadUShort();
            int ttl = packet.ReadInt();
            ushort length = packet.ReadUShort();

            switch (rr_type)
            {
                case 1: return new A(packet, domain, ttl, length);
                case 5: return new CNAME(packet, domain, ttl);
                case 12: return new PTR(packet, domain, ttl);
                case 16: return new TXT(packet, domain, ttl);
                case 28: return new AAAA(packet, domain, ttl, length);
                case 33: return new SRV(packet, domain, ttl);
                default:
                    Debug.WriteLine($"Unknown Resource ({rr_type}/{rr_class})");
                    packet.ReadBytes(length);
                    return new Resource(domain, ttl);
            }
        }

        private DnsResourceType GetResourType(ushort rr_type) => rr_type switch
        {
            1 => DnsResourceType.A,
            5 => DnsResourceType.CNAME,
            12 => DnsResourceType.PTR,
            16 => DnsResourceType.TXT,
            28 => DnsResourceType.AAAA,
            33 => DnsResourceType.SRV,
            _ => 0,
        };
    }
}
