using System.Collections;
using System.Text;

namespace nanoFramework.MulticastDNS.Package
{
    internal class PacketBuilder
    {
        IBitConverter _converter = EndianBitConverter.Big;
        ArrayList list = new ArrayList();

        public void Add(byte value) => list.Add(value);
        public void Add(ushort value) => list.AddRange(_converter.GetBytes(value));
        public void Add(int value) => list.AddRange(_converter.GetBytes(value));
        public void Add(byte[] bytes) => list.AddRange(bytes);

        public void Add(string domain)
        {
            char[] dots = { '.' };
            string[] labels = domain.Trim(dots).Split(dots);
            foreach (string label in labels)
            {
                Add(new Label(label));
            }
            Add((byte)0);  // end the name
        }

        private void Add(Label label)
        {
            Add(label.Length);
            Add(label.GetBytes());
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[list.Count];
            list.CopyTo(bytes);
            return bytes;
        }

        private class Label
        {
            string _label;

            public Label(string label) => _label = label;

            public byte Length => (byte)_label.Length;

            public byte[] GetBytes() => Encoding.UTF8.GetBytes(_label);
        }
    }
}
