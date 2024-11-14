using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// Layout data that describes the layout config.
    /// Includes the render width/height and if origin is centered.
    /// </summary>
    public class Layout 
    {
        /// <summary>
        /// Determines to draw the layout from the center.
        /// </summary>
        public bool DrawFromCenter;

        /// <summary>
        /// The width of the entire layout.
        /// </summary>
        public float Width;

        /// <summary>
        /// The height of the entire layout.
        /// </summary>
        public float Height;

        /// <summary>
        /// 
        /// </summary>
        public float MaxPartsWidth;

        /// <summary>
        /// 
        /// </summary>
        public float MaxPartsHeight;

        /// <summary>
        /// 
        /// </summary>
        public string Name = "";
    }
}
