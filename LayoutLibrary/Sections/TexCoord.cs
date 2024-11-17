using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// Represents texture coordinates for a 4 point quad.
    /// </summary>
    public class TexCoord
    {
        /// <summary>
        /// The top left point of the coordinate.
        /// </summary>
        public Vector2 TopLeft { get; set; }

        /// <summary>
        /// The top right point of the coordinate.
        /// </summary>
        public Vector2 TopRight { get; set; }

        /// <summary>
        /// The bottom left point of the coordinate.
        /// </summary>
        public Vector2 BottomLeft { get; set; }

        /// <summary>
        /// The bottom right point of the coordinate.
        /// </summary>
        public Vector2 BottomRight { get; set; }
    }
}
