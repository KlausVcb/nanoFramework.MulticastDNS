﻿using System;
using System.Diagnostics;
using System.Text;

namespace nanoFramework.MulticastDNS.Package
{
    internal class PacketParser
    {
        private readonly IBitConverter _converter = EndianBitConverter.Big;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly byte[] _data;
        private int _position = 0;

        public PacketParser(byte[] bytes)
        {
            _data = bytes;
        }

        private void MovePosition(int n)
        {
            _position += n;

            if (_position > _data.Length)
                throw new IndexOutOfRangeException("No more data in packet");
        }

        public byte ReadByte() => _data[_position++];

        public ushort ReadUShort()
        {
            ushort value = _converter.ToUInt16(_data, _position);
            MovePosition(2);
            return value;
        }

        public int ReadInt()
        {
            int value = _converter.ToInt32(_data, _position);
            MovePosition(4);
            return value;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] value = new byte[count];
            System.Array.Copy(_data, _position, value, 0, count);
            MovePosition(count);
            return value;
        }

        public string ReadString()
        {
            int length = ReadByte();
            var remaining = _data.Length - _position;

            if (length > remaining)
            {
                Debug.WriteLine($"{nameof(ReadString)}: requested length {length} would surpass available data, truncating to {remaining} bytes.");
                length = remaining;
            }
            string value = _encoding.GetString(_data, _position, length);
            MovePosition(length);
            return value;
        }

        public string ReadDomain()
        {
            int dot = 0;
            string domain = "";

            while (true)
            {
                string label;
                try
                {
                    label = PopLabel(ref dot);
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.WriteLine("error processing " + domain);
                    throw;
                }
                if (label == null) break;
                domain += label + ".";
            }

            return domain.Trim('.');
        }

        private string PopLabel(ref int dot)
        {
            int length = PopLabelLength(ref dot);
            if (length == 0) return null;

            if (dot != 0) return GetPointer(length, ref dot);

            string label = _encoding.GetString(_data, _position, length);
            MovePosition(length);

            return label;
        }

        private int PopLabelLength(ref int dot)
        {
            if (dot != 0) return GetPointerLength(ref dot);
            int length = _data[_position++];
            if ((length & 0xc0) != 0xc0) return length;
            dot = ((length & 0x3f) << 8) + _data[_position++];
            return GetPointerLength(ref dot);
        }

        private string GetPointer(int length, ref int dot)
        {
            if (length == 0) return null;

            string label = _encoding.GetString(_data, dot, length);

            dot += length;
            return label;
        }

        private int GetPointerLength(ref int dot)
        {
            if (dot > _data.Length)
                throw new IndexOutOfRangeException($"Read past end of packet {dot}/{_data.Length}");

            int length = _data[dot++];
            if ((length & 0xc0) != 0xc0) return length;
            dot = ((length & 0x3f) << 8) + _data[dot];
            return GetPointerLength(ref dot);
        }
    }
}
