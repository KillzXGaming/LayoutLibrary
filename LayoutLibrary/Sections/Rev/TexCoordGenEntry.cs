using LayoutLibrary.Revolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary.Sections.Rev
{
    public class TexCoordGenEntry
    {
        public TexCoordGenTypes Type { get; set; }
        public TexCoordGenSource Source { get; set; }
        public TexCoordGenMatrixSource MatrixSource { get; set; }
        public byte Unknown { get; set; }

        public TexCoordGenEntry()
        {
            Type = TexCoordGenTypes.GX_TG_MTX2x4;
            Source = TexCoordGenSource.GX_TG_TEX0;
            MatrixSource = TexCoordGenMatrixSource.GX_TEXMTX0;
            Unknown = 0;
        }

        public TexCoordGenEntry(FileReader reader)
        {
            Type = (TexCoordGenTypes)reader.ReadByte();
            Source = (TexCoordGenSource)reader.ReadByte();
            MatrixSource = (TexCoordGenMatrixSource)reader.ReadByte();
            Unknown = reader.ReadByte();
        }

        public void Write(FileWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write((byte)Source);
            writer.Write((byte)MatrixSource);
            writer.Write(Unknown);
        }
    }
}
