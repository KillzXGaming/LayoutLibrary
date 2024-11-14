using System;
using System.Collections.Generic;
using System.Linq;
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
