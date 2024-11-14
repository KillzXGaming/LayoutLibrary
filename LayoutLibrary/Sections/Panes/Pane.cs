using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// Represents a pane, a rectangular space for displaying and handling layout data.
    /// </summary>
    public class Pane 
    {
        [Newtonsoft.Json.JsonIgnore]
        public virtual string Magic => "pan1";

        /// <summary>
        /// The pane name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The translation of the pane in local space.
        /// </summary>
        public Vector3 Translate { get; set; }

        /// <summary>
        /// The rotation of the pane in local space.
        /// </summary>
        public Vector3 Rotate { get; set; }

        /// <summary>
        /// The scale of the pane in local space.
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// The width of the pane.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The height of the pane.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// The origin placement on the X axis.
        /// </summary>
        public virtual OriginX OriginX { get; set; }

        /// <summary>
        /// The origin placement on the Y axis.
        /// </summary>
        public virtual OriginY OriginY { get; set; }

        /// <summary>
        /// The parent origin placement on the X axis.
        /// </summary>
        public virtual OriginX ParentOriginX { get; set; }

        /// <summary>
        /// The parent origin placement on the Y axis.
        /// </summary>
        public virtual OriginY ParentOriginY { get; set; }

        /// <summary>
        /// The alpha of the pane.
        /// </summary>
        public byte Alpha { get; set; }

        private Pane parent;

        /// <summary>
        /// The parent that this pane connects to.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Pane Parent
        {
            get => parent;
            set
            {
                if (value != null)
                {
                    if (!value.Children.Contains(this))
                        value.Children.Add(this);
                    this.parent = value;
                }
            }
        }

        /// <summary>
        /// Children connected to this pane.
        /// </summary>
        public List<Pane> Children { get; set; } = new List<Pane>();

        /// <summary>
        /// Pane mag flags used for parts.
        /// </summary>
        public byte PaneMagFlags { get; set; }

        /// <summary>
        /// User data string.
        /// </summary>
        public string UserDataInfo { get; set; }

        /// <summary>
        /// Pane flags.
        /// </summary>
        public byte Flags1 { get; set; }

        /// <summary>
        /// Toggles visibility of the pane.
        /// </summary>
        public bool Visible
        {
            get { return (Flags1 & 0x1) == 0x1; }
            set
            {
                if (value)
                    Flags1 |= 0x1;
                else
                    Flags1 &= 0xFE;
            }
        }

        /// <summary>
        /// Determines if the current alpha influences the children.
        /// </summary>
        public bool InfluenceAlpha
        {
            get { return (Flags1 & 0x2) == 0x2; }
            set
            {
                if (value)
                    Flags1 |= 0x2;
                else
                    Flags1 &= 0xFD;
            }
        }

        /// <summary>
        /// Pane user data.
        /// </summary>
        public UserData UserData { get; set; }
    }
}
