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
    public class Color16
    {
        /// <summary>
        /// The red channel value.
        /// </summary>
        public ushort R;

        /// <summary>
        /// The green channel value.
        /// </summary>
        public ushort G;

        /// <summary>
        /// The blue channel value.
        /// </summary>
        public ushort B;

        /// <summary>
        /// The alpha channel value.
        /// </summary>
        public ushort A;

        /// <summary>
        /// A white color (0xFFFFFFFF).
        /// </summary>
        public static Color16 White = new Color16(255, 255, 255, 255);

        /// <summary>
        /// A black color with 255 alpha (0x000000FF).
        /// </summary>
        public static Color16 Black = new Color16(0, 0, 0, 255);

        public Color16(ushort r, ushort g, ushort b, ushort a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color16(ushort[] rgba)
        {
            R = rgba[0];
            G = rgba[1];
            B = rgba[2];
            A = rgba[3];
        }

        public ushort[] ToUInt16s()
        {
            return new ushort[] { R, G, B, A };
        }
    }
}
