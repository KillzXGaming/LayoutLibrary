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
    public class TextPane : Pane
    {
        public override string Magic => "txt1";

        public OriginX HorizontalAlignment
        {
            get { return (OriginX)((TextAlignment) & 0x3); }
            set
            {
                TextAlignment &= unchecked((byte)(~0x3));
                TextAlignment |= (byte)(value);
            }
        }

        public OriginY VerticalAlignment
        {
            get { return (OriginY)((TextAlignment >> 2) & 0x3); }
            set
            {
                TextAlignment &= unchecked((byte)(~0xC));
                TextAlignment |= (byte)((byte)(value) << 2);
            }
        }

        public ushort RestrictedLength
        {
            get
                { //Divide by 2 due to 2 characters taking up 2 bytes
                  //Subtract 1 due to padding
                return (ushort)((TextLength / 2) - 1);
            }
            set
            {
                TextLength = (ushort)((value * 2) + 1);
            }
        }

        public string Text { get; set; }

        public ushort TextLength { get; set; }
        public ushort MaxTextLength { get; set; }
        public ushort MaterialIndex { get; set; }
        public ushort FontIndex { get; set; }

        public byte TextAlignment { get; set; }
        public LineAlign LineAlignment { get; set; }
        public float ItalicTilt { get; set; }
        public Color FontTopColor { get; set; }
        public Color FontBottomColor { get; set; }
        public Vector2 FontSize { get; set; }
        public float CharacterSpace { get; set; }
        public float LineSpace { get; set; }
        public Vector2 ShadowXY { get; set; }
        public Vector2 ShadowXYSize { get; set; }
        public Color ShadowForeColor { get; set; }
        public Color ShadowBackColor { get; set; }
        public float ShadowItalic { get; set; }
        public string TextBoxName { get; set; }

        public byte _flags;
        public byte Unknown3;

        public float Unknown1 { get; set; }
        public float Unknown2 { get; set; }


        public bool ShadowEnabled
        {
            get { return (_flags & 1) != 0; }
            set { _flags = value ? (byte)(_flags | 1) : unchecked((byte)(_flags & (~1))); }
        }
        public bool RestrictedTextLengthEnabled
        {
            get { return (_flags & 0x2) != 0; }
            set { _flags = value ? (byte)(_flags | 0x2) : unchecked((byte)(_flags & (~0x2))); }
        }

        public bool PerCharTransformEnabled
        {
            get { return (_flags & 0x10) != 0; }
            set { _flags = value ? (byte)(_flags | 0x10) : unchecked((byte)(_flags & (~0x10))); }
        }

        public PerCharacterTransform PerCharacterTransform;

        public TextPane() { }
        public TextPane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal override void Read(FileReader reader, LayoutHeader header)
        {
            if (header.IsRev || header.IsCTR)
                ReadCtrRev(reader, header);
            else
                ReadCafe(reader, header);
        }

        internal void ReadCtrRev(FileReader reader, LayoutHeader header)
        {
            long pos = reader.Position - 8;

            reader.SeekBegin(pos + 4);
            uint sectionSize = reader.ReadUInt32();

            var pane = this;

            base.Read(reader, header);

            pane.TextLength = reader.ReadUInt16();
            pane.MaxTextLength = reader.ReadUInt16();
            pane.MaterialIndex = reader.ReadUInt16();
            pane.FontIndex = reader.ReadUInt16();
            pane.TextAlignment = reader.ReadByte();
            pane.LineAlignment = (LineAlign)reader.ReadByte();
            pane._flags = reader.ReadByte();
            pane.Unknown3 = reader.ReadByte();
            uint textOffset = reader.ReadUInt32();
            pane.FontTopColor = new Color(reader.ReadUInt32());
            pane.FontBottomColor = new Color(reader.ReadUInt32());
            pane.FontSize = reader.ReadVec2();
            pane.CharacterSpace = reader.ReadSingle();
            pane.LineSpace = reader.ReadSingle();

            if (textOffset != sectionSize && pane.TextLength > 0)
            {
                reader.SeekBegin(pos + textOffset);
                pane.Text = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated,
                       Encoding.Unicode);
            }
        }

        internal void ReadCafe(FileReader reader, LayoutHeader header)
        {
            long pos = reader.Position - 8;

            base.Read(reader, header);

            this.TextLength = reader.ReadUInt16();
            this.MaxTextLength = reader.ReadUInt16();
            this.MaterialIndex = reader.ReadUInt16();
            this.FontIndex = reader.ReadUInt16();
            this.TextAlignment = reader.ReadByte();
            this.LineAlignment = (LineAlign)reader.ReadByte();
            this._flags = reader.ReadByte();
            this.Unknown3 = reader.ReadByte();
            this.ItalicTilt = reader.ReadSingle();
            uint textOffset = reader.ReadUInt32();
            this.FontTopColor = new Color(reader.ReadUInt32());
            this.FontBottomColor = new Color(reader.ReadUInt32());
            this.FontSize = reader.ReadVec2();
            this.CharacterSpace = reader.ReadSingle();
            this.LineSpace = reader.ReadSingle();
            uint nameOffset = reader.ReadUInt32();
            this.ShadowXY = reader.ReadVec2();
            this.ShadowXYSize = reader.ReadVec2();
            this.ShadowForeColor = new Color(reader.ReadUInt32());
            this.ShadowBackColor = new Color(reader.ReadUInt32());
            this.ShadowItalic = reader.ReadSingle();

            uint lineTransformOffset = 0;
            if (header.VersionMajor >= 8)
                lineTransformOffset = reader.ReadUInt32();

            uint perCharTransformOffset = 0;
            if (header.VersionMajor > 3)
                perCharTransformOffset = reader.ReadUInt32();

            if (textOffset != 0 && this.TextLength > 0)
            {
                reader.SeekBegin(pos + textOffset);
                this.Text = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated,
                       Encoding.Unicode);
            }

            if (nameOffset != 0)
            {
                reader.SeekBegin(pos + nameOffset);
                this.TextBoxName = reader.ReadZeroTerminatedString();
            }

            if (this.PerCharTransformEnabled && perCharTransformOffset != 0)
            {
                reader.SeekBegin(pos + perCharTransformOffset);
                var transform = new PerCharacterTransform();
                transform.CurveTimeOffset = reader.ReadSingle();
                transform.CurveWidth = reader.ReadSingle();
                transform.LoopType = reader.ReadByte();
                transform.VerticalOrigin = reader.ReadByte();
                transform.HasAnimInfo = reader.ReadByte();
                transform.padding = reader.ReadByte();
                this.PerCharacterTransform = transform;

                transform.CharList = reader.ReadBytes(20);

                if (transform.HasAnimInfo != 0)
                {
                    transform.AnimationInfo = new AnimationInfoSubGroup();
                    transform.AnimationInfo.Read(reader);
                }
            }
        }

        internal override void Write(FileWriter writer, LayoutHeader header)
        {
            if (header.IsRev || header.IsCTR)
                WriteCtrRev(writer, header);
            else
                WriteCafe(writer, header);
        }

        private void WriteCtrRev(FileWriter writer, LayoutHeader header)
        {
            long pos = writer.Position - 8;

            var pane = this;

            base.Write(writer, header);

            writer.Write((ushort)pane.TextLength);
            writer.Write((ushort)pane.MaxTextLength);
            writer.Write((ushort)pane.MaterialIndex);
            writer.Write((ushort)pane.FontIndex);
            writer.Write((byte)pane.TextAlignment);
            writer.Write((byte)pane.LineAlignment);
            writer.Write((byte)pane._flags);
            writer.Write((byte)pane.Unknown3);

            var textOffset = (int)writer.Position;
            writer.Write(0);
            writer.Write(pane.FontTopColor.ToUInt32());
            writer.Write(pane.FontBottomColor.ToUInt32());
            writer.Write(pane.FontSize);
            writer.Write(pane.CharacterSpace);
            writer.Write(pane.LineSpace);

            writer.WriteUint32Offset(textOffset, (int)pos);
            if (!string.IsNullOrEmpty(pane.Text))
            {
                if (writer.ByteOrder == Syroot.BinaryData.ByteOrder.BigEndian)
                    writer.Write(Encoding.BigEndianUnicode.GetBytes(pane.Text));
                else
                    writer.Write(Encoding.Unicode.GetBytes(pane.Text));
                writer.Write((byte)0);
                writer.Align(4);
            }
        }

        private void WriteCafe(FileWriter writer, LayoutHeader header)
        {
            long pos = writer.Position - 8;

            base.Write(writer, header);

            writer.Write((ushort)this.TextLength);
            writer.Write((ushort)this.MaxTextLength);
            writer.Write((ushort)this.MaterialIndex);
            writer.Write((ushort)this.FontIndex);
            writer.Write((byte)this.TextAlignment);
            writer.Write((byte)this.LineAlignment);
            writer.Write((byte)this._flags);
            writer.Write((byte)this.Unknown3);
            writer.Write(this.ItalicTilt);

            var textOffset = (int)writer.Position;
            writer.Write(0);
            writer.Write(this.FontTopColor.ToUInt32());
            writer.Write(this.FontBottomColor.ToUInt32());
            writer.Write(this.FontSize);
            writer.Write(this.CharacterSpace);
            writer.Write(this.LineSpace);

            var nameOffset = (int)writer.Position;
            writer.Write(0);

            writer.Write(this.ShadowXY);
            writer.Write(this.ShadowXYSize);
            writer.Write(this.ShadowForeColor.ToUInt32());
            writer.Write(this.ShadowBackColor.ToUInt32());
            writer.Write(this.ShadowItalic);

            var lineTransformOffset = (int)writer.Position;
            if (header.VersionMajor >= 8)
                writer.Write(0);

            var perCharTransformOffset = (int)writer.Position;
            if (header.VersionMajor > 3)
                writer.Write(0);

            writer.Align(4);

            writer.WriteUint32Offset(textOffset, (int)pos);
            if (!string.IsNullOrEmpty(this.Text))
            {
                if (writer.ByteOrder == Syroot.BinaryData.ByteOrder.BigEndian)
                    writer.Write(Encoding.BigEndianUnicode.GetBytes(this.Text));
                else
                    writer.Write(Encoding.Unicode.GetBytes(this.Text));
                writer.Write((byte)0);
                writer.AlignBytes(4);
            }
            else if (this.TextLength > 0)
            {
                writer.Write(new byte[this.TextLength]);
                writer.AlignBytes(4);
            }

            if (!string.IsNullOrEmpty(this.TextBoxName))
            {
                writer.WriteUint32Offset(nameOffset, (int)pos);
                writer.WriteStringZeroTerminated(this.TextBoxName);
                writer.Align(4);
            }

            if (this.PerCharacterTransform != null)
            {
                writer.WriteUint32Offset(perCharTransformOffset, (int)pos);
                var val = this.PerCharacterTransform;

                writer.Write(val.CurveTimeOffset);
                writer.Write(val.CurveWidth);
                writer.Write((byte)val.LoopType);
                writer.Write((byte)val.VerticalOrigin);
                writer.Write((byte)val.HasAnimInfo);
                writer.Write((byte)val.padding);

                writer.Write(val.CharList);

                writer.Align(4);

                if (val.HasAnimInfo != 0)
                    val.AnimationInfo.Write(writer);
            }
        }

        #endregion
    }

    public class PerCharacterTransform
    {
        public float CurveTimeOffset { get; set; }
        public float CurveWidth { get; set; }
        public byte LoopType { get; set; }
        public byte VerticalOrigin { get; set; }
        public byte HasAnimInfo { get; set; }
        public byte padding { get; set; }

        public AnimationInfoSubGroup AnimationInfo;

        public byte[] CharList = new byte[20];
    }

    public class PerCharacterTransformChars
    {
        public float Start;
        public float End;

        public List<PerCharacterTransformKey> Keys = new List<PerCharacterTransformKey>();
    }

    public class PerCharacterTransformKey
    {
        public float Value1;
        public float Value2;
    }
}
