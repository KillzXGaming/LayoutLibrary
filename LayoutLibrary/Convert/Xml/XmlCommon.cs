
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlColor
    {
        [XmlAttribute("r")]
        public byte R;
        [XmlAttribute("g")]
        public byte G;
        [XmlAttribute("b")]
        public byte B;
        [XmlAttribute("a")]
        public byte A;

        public XmlColor() { }
        public XmlColor(byte r, byte g, byte b, byte a)
        {
            R = r; G = g; B = b; A = a;
        }
        public XmlColor(Color color)
        {
            R = (byte)(color.R * 255);
            G = (byte)(color.G * 255);
            B = (byte)(color.B * 255);
            A = (byte)(color.A * 255);
        }

        public Color ToColor() => new Color(R, G, B, A);
    }

    public class XmlVector3
    {
        [XmlAttribute("x")]
        public float X;
        [XmlAttribute("y")]
        public float Y;
        [XmlAttribute("z")]
        public float Z;

        public XmlVector3() { }
        public XmlVector3(Vector3 v) { X = v.X; Y = v.Y; Z = v.Z; }
        public XmlVector3(float x, float y, float z) { X = x; Y = y; Z = z; }

        public Vector3 ToVector3() => new Vector3(X, Y, Z);
    }

    public class XmlVector2
    {
        [XmlAttribute("x")]
        public float X;
        [XmlAttribute("y")]
        public float Y;

        public XmlVector2() { }
        public XmlVector2(Vector2 v) { X = v.X; Y = v.Y; }
        public XmlVector2(float x, float y) { X = x; Y = y; }

        public Vector2 ToVector2() => new Vector2(X, Y);
    }

    public class XmlValue
    {
        [XmlAttribute("x")]
        public float Value;

        public XmlValue() { }
        public XmlValue(float v) { Value = v; }
    }


    public class Size
    {
        [XmlAttribute()]
        public float Width;
        [XmlAttribute()]
        public float Height;

        public Size() { }
        public Size(float w, float h) { Width = w; Height = h; }
    }

    public class OriginPlacement
    {
        [XmlAttribute("x")]
        public OriginX X;
        [XmlAttribute("y")]
        public OriginY Y;

        public OriginPlacement() { }
        public OriginPlacement(OriginX x, OriginY y) { X = x; Y = y; }
    }

    public class XmlTexCoord
    {
        public XmlVector2 TopLeft;
        public XmlVector2 TopRight;
        public XmlVector2 BottomLeft;
        public XmlVector2 BottomRight;
    }
}
