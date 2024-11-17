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
    /// A window pane for displaying corner data.
    /// </summary>
    public class WindowPane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "wnd1";

        /// <summary>
        /// Determines if all corners share the same material.
        /// </summary>
        public bool UseOneMaterialForAll { get; set; }

        /// <summary>
        /// Determines if all corners share the same vertex color.
        /// </summary>
        public bool UseVertexColorForAll { get; set; }

        /// <summary>
        /// The type of window display.
        /// </summary>
        public WindowKind WindowKind { get; set; }

        /// <summary>
        /// Determines to ignore the middle content region and only show corners.
        /// </summary>
        public bool NotDrawnContent { get; set; }

        public ushort StretchLeft { get; set; }
        public ushort StretchRight { get; set; }
        public ushort StretchTop { get; set; }
        public ushort StretchBottm { get; set; }
        public ushort FrameElementLeft { get; set; }
        public ushort FrameElementRight { get; set; }
        public ushort FrameElementTop { get; set; }
        public ushort FrameElementBottm { get; set; }

        /// <summary>
        /// The middle window content of the window frame.
        /// </summary>
        public WindowContent Content { get; set; } = new WindowContent();

        /// <summary>
        /// Window frames.
        /// </summary>
        public List<WindowFrame> WindowFrames { get; set; } = new List<WindowFrame>();

        /// <summary>
        /// 
        /// </summary>
        public byte Flag { get; set; }

        public WindowPane() { }
        public WindowPane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal override void Read(FileReader reader, LayoutHeader header)
        {
            long pos = reader.Position - 8;

            base.Read(reader, header);

            this.StretchLeft = reader.ReadUInt16();
            this.StretchRight = reader.ReadUInt16();
            this.StretchTop = reader.ReadUInt16();
            this.StretchBottm = reader.ReadUInt16();
            this.FrameElementLeft = reader.ReadUInt16();
            this.FrameElementRight = reader.ReadUInt16();
            this.FrameElementTop = reader.ReadUInt16();
            this.FrameElementBottm = reader.ReadUInt16();
            var frameCount = reader.ReadByte();
            this.Flag = reader.ReadByte();
            reader.ReadUInt16();//padding
            uint contentOffset = reader.ReadUInt32();
            uint frameOffsetTbl = reader.ReadUInt32();

            this.WindowKind = (WindowKind)((this.Flag >> 2) & 3);

            reader.SeekBegin(pos + contentOffset);
            WindowContent cnt = new WindowContent();
            cnt.ColorTopLeft = new Color(reader.ReadUInt32());
            cnt.ColorTopRight = new Color(reader.ReadUInt32());
            cnt.ColorBottomLeft = new Color(reader.ReadUInt32());
            cnt.ColorBottomRight = new Color(reader.ReadUInt32());
            cnt.MaterialIndex = reader.ReadUInt16();
            byte UVCount = reader.ReadByte();
            reader.ReadByte(); //padding

            for (int i = 0; i < UVCount; i++)
                cnt.TexCoords.Add(new TexCoord()
                {
                    TopLeft = reader.ReadVec2(),
                    TopRight = reader.ReadVec2(),
                    BottomLeft = reader.ReadVec2(),
                    BottomRight = reader.ReadVec2(),
                });
            this.Content = cnt;

            reader.SeekBegin(pos + frameOffsetTbl);
            var offsets = reader.ReadUInt32s(frameCount);
            foreach (int offset in offsets)
            {
                reader.SeekBegin(pos + offset);
                this.WindowFrames.Add(new WindowFrame()
                {
                    MaterialIndex = reader.ReadUInt16(),
                    TextureFlip = (WindowFrameTexFlip)reader.ReadByte(),
                });
                reader.ReadByte(); //padding
            }
        }

        internal override void Write(FileWriter writer, LayoutHeader header)
        {
            this.Flag = (byte)((this.Flag & ~0x0C) | ((int)this.WindowKind << 2));
            long pos = writer.Position - 8;

            base.Write(writer, header);

            writer.Write((ushort)this.StretchLeft);
            writer.Write((ushort)this.StretchRight);
            writer.Write((ushort)this.StretchTop);
            writer.Write((ushort)this.StretchBottm);
            writer.Write((ushort)this.FrameElementLeft);
            writer.Write((ushort)this.FrameElementRight);
            writer.Write((ushort)this.FrameElementTop);
            writer.Write((ushort)this.FrameElementBottm);
            writer.Write((byte)this.WindowFrames.Count);
            writer.Write((byte)this.Flag);
            writer.Write((ushort)0); // padding

            int contentOffset = (int)writer.Position;
            writer.Write(0);

            int frameOffsetTbl = (int)writer.Position;
            writer.Write(0);

            writer.WriteUint32Offset(contentOffset, (int)pos);

            var cnt = this.Content;
            writer.Write(cnt.ColorTopLeft.ToUInt32());
            writer.Write(cnt.ColorTopRight.ToUInt32());
            writer.Write(cnt.ColorBottomLeft.ToUInt32());
            writer.Write(cnt.ColorBottomRight.ToUInt32());
            writer.Write((ushort)cnt.MaterialIndex);
            writer.Write((byte)cnt.TexCoords.Count);
            writer.Write((byte)0);

            for (int i = 0; i < cnt.TexCoords.Count; i++)
            {
                writer.Write(cnt.TexCoords[i].TopLeft);
                writer.Write(cnt.TexCoords[i].TopRight);
                writer.Write(cnt.TexCoords[i].BottomLeft);
                writer.Write(cnt.TexCoords[i].BottomRight);
            }

            writer.WriteUint32Offset(frameOffsetTbl, (int)pos);

            var frameStart = writer.Position;
            writer.Write(new uint[this.WindowFrames.Count]);

            for (int i = 0; i < this.WindowFrames.Count; i++)
            {
                writer.WriteUint32Offset(frameStart + i * 4, (int)pos);
                writer.Write((ushort)this.WindowFrames[i].MaterialIndex);
                writer.Write((byte)this.WindowFrames[i].TextureFlip);
                writer.Write((byte)0); //padding
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a window content used as the center window pane.
    /// </summary>
    public class WindowContent
    {
        /// <summary>
        /// The top left vertex color of the window content.
        /// </summary>
        public Color ColorTopLeft { get; set; } = Color.White;

        /// <summary>
        /// The top right vertex color of the window content.
        /// </summary>
        public Color ColorTopRight { get; set; } = Color.White;

        /// <summary>
        /// The bottom left vertex color of the window content.
        /// </summary>
        public Color ColorBottomLeft { get; set; } = Color.White;

        /// <summary>
        /// The bottom right vertex color of the window content.
        /// </summary>
        public Color ColorBottomRight { get; set; } = Color.White;

        /// <summary>
        /// The material index for what material is assigned to the window content.
        /// </summary>
        public ushort MaterialIndex { get; set; }

        /// <summary>
        /// Texture coordinates for the window frame.
        /// </summary>
        public List<TexCoord> TexCoords = new List<TexCoord>();
    }

    /// <summary>
    /// Represents a corner frame used in a window pane.
    /// </summary>
    public class WindowFrame
    {
        /// <summary>
        /// The material index for what material is assigned to the window frame.
        /// </summary>
        public ushort MaterialIndex;

        /// <summary>
        /// Determines what direction to flip the material frame.
        /// </summary>
        public WindowFrameTexFlip TextureFlip { get; set; } = WindowFrameTexFlip.None;
    }
}
