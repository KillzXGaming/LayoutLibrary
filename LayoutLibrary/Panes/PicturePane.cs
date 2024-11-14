using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
