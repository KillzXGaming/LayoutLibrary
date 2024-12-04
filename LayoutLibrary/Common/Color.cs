using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// Represents color data that stores RGBA values.
    /// </summary>
    public class Color
    {
        /// <summary>
        /// The red channel value.
        /// </summary>
        public float R;

        /// <summary>
        /// The green channel value.
        /// </summary>
        public float G;

        /// <summary>
        /// The blue channel value.
        /// </summary>
        public float B;

        /// <summary>
        /// The alpha channel value.
        /// </summary>
        public float A;

        /// <summary>
        /// A white color (0xFFFFFFFF).
        /// </summary>
        public static Color White = new Color(0xFFFFFFFF);

        /// <summary>
        /// A black color with 255 alpha (0x000000FF).
        /// </summary>
        public static Color Black = new Color(0x000000FF);

        public Color(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }

        public Color(uint rgba)
        {
            A = ((byte)((rgba >> 24) & 0xFF)) / 255f;
            B = ((byte)((rgba >> 16) & 0xFF)) / 255f; 
            G = ((byte)((rgba >> 8) & 0xFF)) / 255f; 
            R = ((byte)(rgba & 0xFF)) / 255f; 
        }

        public uint ToUInt32()
        {
            byte r = (byte)(R * 255);
            byte g = (byte)(G * 255);
            byte b = (byte)(B * 255);
            byte a = (byte)(A * 255);

            return ((uint)a << 24) | ((uint)b << 16) | ((uint)g << 8) | r;
        }
    }
}
