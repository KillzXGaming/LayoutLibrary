using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class FileWriter : BinaryDataWriter
    {
        internal bool ReverseMagic = false;

        public FileWriter(Stream stream) : base(stream)
        {

        }

        public FileWriter(string path)
            : base(new FileStream(path, FileMode.Create, FileAccess.Write))
        {

        }

        public void WriteStringZeroTerminated(string value)
        {
            Write(this.Encoding.GetBytes(value));
            Write((byte)0);
        }

        public void WriteFixedString(string value, int count)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            //clamp string
            if (buffer.Length > count)
            {
                buffer = buffer.AsSpan().Slice(0, count).ToArray();
                Console.WriteLine($"Warning! String {value} too long!");
            }

            Write(buffer);
            Write(new byte[count - buffer.Length]);
        }

        public void SetByteOrder(bool IsBigEndian)
        {
            if (IsBigEndian)
                ByteOrder = ByteOrder.BigEndian;
            else
                ByteOrder = ByteOrder.LittleEndian;
        }

        public void WriteSignature(string value)
        {
            if (ReverseMagic)
                Write(Encoding.ASCII.GetBytes(new string(value.Reverse().ToArray())));
            else
                Write(Encoding.ASCII.GetBytes(value));
        }

        public void WriteUint32Offset(long target, int relativePosition = 0)
        {
            long pos = Position;
            using (TemporarySeek(target, SeekOrigin.Begin)) {
                Write((uint)(pos - relativePosition));
            }
        }

        public void WriteByteOffset(long target, int relativePosition = 0)
        {
            long pos = Position;
            using (TemporarySeek(target, SeekOrigin.Begin))
            {
                Write((byte)(pos - relativePosition));
            }
        }

        public void WriteSectionSizeU32(long position, long startPosition, long endPosition) {
            WriteSectionSizeU32(position, endPosition - startPosition);
        }

        public void WriteSectionSizeU32(long position, long size)
        {
            using (TemporarySeek(position, System.IO.SeekOrigin.Begin))
            {
                Write((uint)(size));
            }
        }


        public void AlignBytes(int alignment, byte value = 0)
        {
            var startPos = Position;
            long position = Seek((-Position % alignment + alignment) % alignment, SeekOrigin.Current);

            Seek(startPos, System.IO.SeekOrigin.Begin);
            while (Position != position)
            {
                Write(value);
            }
        }

        public void AlignBytes(long origin_start, int alignment, byte value = 0)
        {
            var startPos = Position;

            //create a relative pos to align
            var aligned = (-(Position - origin_start) % alignment + alignment) % alignment;

            if (aligned > 0)
            {
                while (Position != startPos + aligned)
                {
                    Write(value);
                }
            }
        }

        public void AlignBytesU16(int alignment, ushort value = 0)
        {
            var startPos = Position;
            long position = Seek((-Position % alignment + alignment) % alignment, SeekOrigin.Current);

            Seek(startPos, System.IO.SeekOrigin.Begin);
            while (Position != position)
            {
                Write(value);
            }
        }

        public void CheckByteOrderMark(ushort byteOrderMark)
        {
            if (byteOrderMark == 0xFEFF)
                ByteOrder = ByteOrder.BigEndian;
            else
                ByteOrder = ByteOrder.LittleEndian;
        }

        public void SeekBegin(long pos) => this.Seek(pos, SeekOrigin.Begin);

        public void WriteStringOffsets(List<string> values)
        {
            //Fill empty spaces for offsets later
            long pos = this.Position;
            this.Write(new uint[values.Count]);

            //Save offsets and strings
            for (int i = 0; i < values.Count; i++)
            {
                this.WriteUint32Offset(pos + (i * 4), (int)pos);
                this.WriteStringZeroTerminated(values[i]);
            }
            this.AlignBytes(4);
        }
    }
}
