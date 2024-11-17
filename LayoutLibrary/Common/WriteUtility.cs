using LayoutLibrary.Cafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public static class WriteUtility
    {
        public static void WriteStringSection(FileWriter writer, List<string> values, BflytFile header)
        {
            if (header.IsRev)
            {
                WriteStringSectionRev(writer, values, header);
                return;
            }

            writer.Write((ushort)values.Count);
            writer.Write((ushort)0);

            //Fill empty spaces for offsets later
            long pos = writer.Position;
            writer.Write(new uint[values.Count]);

            //Save offsets and strings
            for (int i = 0; i < values.Count; i++)
            {
                writer.WriteUint32Offset(pos + (i * 4), (int)pos);
                writer.WriteStringZeroTerminated(values[i]);
            }
            writer.AlignBytes(4);
        }

        static void WriteStringSectionRev(FileWriter writer, List<string> values, BflytFile header)
        {
            writer.Write((ushort)values.Count);
            writer.Write((ushort)0);

            //Fill empty spaces for offsets later
            long pos = writer.Position;
            writer.Write(new uint[values.Count * 2]);

            //Save offsets and strings
            for (int i = 0; i < values.Count; i++)
            {
                writer.WriteUint32Offset(pos + (i * 8), (int)pos);
                writer.WriteStringZeroTerminated(values[i]);
            }
            writer.AlignBytes(4);
        }

        public static void Write(this FileWriter writer, Vector2 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
        }

        public static void Write(this FileWriter writer, Vector3 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
        }

        public static void Write(this FileWriter writer, Vector4 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
            writer.Write(v.W);
        }
    }
}
