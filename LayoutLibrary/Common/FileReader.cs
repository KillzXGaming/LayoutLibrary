using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class FileReader : BinaryDataReader
    {
        internal bool ReverseMagic = false;

        public FileReader(Stream stream) : base(stream) { 

        }

        public string ReadFixedString(int count)
        {
            var buffer = ReadBytes(count);
            return this.Encoding.GetString(buffer).Replace("\0", "");
        }

        public string ReadZeroTerminatedString() => ReadString(BinaryStringFormat.ZeroTerminated);

        public void SetByteOrder(bool isBigEndian)
        {
            if (isBigEndian)
                ByteOrder = ByteOrder.BigEndian;
            else
                ByteOrder = ByteOrder.LittleEndian;
        }

        public string ReadSignature()
        {
            string signature = ReadString(4, Encoding.ASCII);
            if (ReverseMagic)
                signature = new string(signature.Reverse().ToArray());

            return signature;
        }

        public string ReadSignature(string expected_magic)
        {
            string signature = ReadString(expected_magic.Length, Encoding.ASCII);
            if (ReverseMagic)
                signature = new string(signature.Reverse().ToArray());

            if (signature != expected_magic)
                throw new Exception($"Invalid signature {signature}! Expected {expected_magic}.");

            return signature;
        }

        public void SeekBegin(long pos) => this.Seek(pos, SeekOrigin.Begin);

        public void CheckByteOrderMark(ushort byteOrderMark)
        {
            if (byteOrderMark == 0xFEFF)
                ByteOrder = ByteOrder.BigEndian;
            else
                ByteOrder = ByteOrder.LittleEndian;
        }

        public List<string> ReadStringOffsets(int count)
        {
            List<string> values = new List<string>();

            long pos = this.Position;
            uint[] offsets = this.ReadUInt32s(count);
            for (int i = 0; i < offsets.Length; i++)
            {
                this.SeekBegin(offsets[i] + pos);
                values.Add(this.ReadZeroTerminatedString());
            }
            return values;
        }
    }
}
