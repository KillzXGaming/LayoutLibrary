using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// A picture pane that is capable of displaying material textures and vertex colors.
    /// </summary>
    public class PicturePane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "pic1";

        /// <summary>
        /// Texture coordinates for assigning the texture space.
        /// Which coordinates are used depends on the texture map uv source.
        /// </summary>
        public TexCoord[] TexCoords { get; set; } = new TexCoord[0];

        /// <summary>
        /// The top left vertex color.
        /// </summary>
        public Color ColorTopLeft { get; set; } = Color.White;

        /// <summary>
        /// The top right vertex color.
        /// </summary>
        public Color ColorTopRight { get; set; } = Color.White;

        /// <summary>
        /// The bottom left vertex color.
        /// </summary>
        public Color ColorBottomLeft { get; set; } = Color.White;

        /// <summary>
        /// The bottom right vertex color.
        /// </summary>
        public Color ColorBottomRight { get; set; } = Color.White;

        /// <summary>
        /// The material index for what material is assigned to.
        /// </summary>
        public ushort MaterialIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsShape { get; set; }

        public PicturePane() { }
        public PicturePane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal override void Read(FileReader reader, LayoutHeader header)
        {
            base.Read(reader, header);

            this.ColorTopLeft = new Color(reader.ReadUInt32());
            this.ColorTopRight = new Color(reader.ReadUInt32());
            this.ColorBottomLeft = new Color(reader.ReadUInt32());
            this.ColorBottomRight = new Color(reader.ReadUInt32());
            this.MaterialIndex = reader.ReadUInt16();
            byte numUVs = reader.ReadByte();
            this.IsShape = reader.ReadBoolean();

            this.TexCoords = new TexCoord[numUVs];
            for (int i = 0; i < numUVs; i++)
            {
                this.TexCoords[i] = new TexCoord()
                {
                    TopLeft = reader.ReadVec2(),
                    TopRight = reader.ReadVec2(),
                    BottomLeft = reader.ReadVec2(),
                    BottomRight = reader.ReadVec2(),
                };
            }
        }

        internal override void Write(FileWriter writer, LayoutHeader header)
        {
            base.Write(writer, header);
            writer.Write(this.ColorTopLeft.ToUInt32());
            writer.Write(this.ColorTopRight.ToUInt32());
            writer.Write(this.ColorBottomLeft.ToUInt32());
            writer.Write(this.ColorBottomRight.ToUInt32());
            writer.Write((ushort)this.MaterialIndex);
            writer.Write((byte)this.TexCoords.Length);
            writer.Write((byte)(this.IsShape ? 1 : 0));
            for (int i = 0; i < this.TexCoords.Length; i++)
            {
                writer.Write(this.TexCoords[i].TopLeft);
                writer.Write(this.TexCoords[i].TopRight);
                writer.Write(this.TexCoords[i].BottomLeft);
                writer.Write(this.TexCoords[i].BottomRight);
            }
        }

        #endregion
    }
}
