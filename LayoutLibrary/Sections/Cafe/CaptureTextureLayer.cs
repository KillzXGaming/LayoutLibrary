using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class CaptureTextureLayer
    {
        public byte[] Raw;

        public CaptureTextureLayer() { }
        public CaptureTextureLayer(FileReader reader, LayoutHeader header) { Read(reader, header); }

        internal void Read(FileReader reader, LayoutHeader header)
        {
            reader.Seek(-4, SeekOrigin.Current);
            uint size = reader.ReadUInt32();

            Raw = reader.ReadBytes((int)size - 8);
        }

        internal void Write(FileWriter writer, LayoutHeader header)
        {
            writer.Write(Raw);
        }
    }
}
