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
    }

    public class PerCharacterTransform
    {
        public float CurveTimeOffset { get; set; }
        public float CurveWidth { get; set; }
        public byte LoopType { get; set; }
        public byte VerticalOrigin { get; set; }
        public byte HasAnimInfo { get; set; }
        public byte padding { get; set; }

        public AnimationInfo AnimationInfo;

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
