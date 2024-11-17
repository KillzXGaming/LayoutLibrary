using LayoutLibrary.Cafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public static class ReadUtility
    {
        public static List<string> ReadStringSection(FileReader reader, BflytFile header)
        {
            if (header.IsRev)
                return ReadStringSectionRev(reader, header);

            List<string> values = new List<string>();

            ushort count = reader.ReadUInt16();
            reader.Seek(2); //padding

            long pos = reader.Position;
            uint[] offsets = reader.ReadUInt32s(count);
            for (int i = 0; i < offsets.Length; i++)
            {
                reader.SeekBegin(offsets[i] + pos);
                values.Add(reader.ReadZeroTerminatedString());
            }
            return values;
        }

        static List<string> ReadStringSectionRev(FileReader reader, BflytFile header)
        {
            List<string> values = new List<string>();

            ushort count = reader.ReadUInt16();
            reader.Seek(2); //padding

            long pos = reader.Position;
            for (int i = 0; i < count; i++)
            {
                uint offset = reader.ReadUInt32();
                reader.ReadUInt32(); //padding
                using (reader.TemporarySeek(offset + pos, SeekOrigin.Begin)) {
                    values.Add(reader.ReadZeroTerminatedString());
                }
            }
            return values;
        }

        public static Vector2 ReadVec2(this FileReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVec3(this FileReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVec4(this FileReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(),
                               reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
