using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
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
        public virtual OriginX OriginX { get; set; } = OriginX.Center;

        /// <summary>
        /// The origin placement on the Y axis.
        /// </summary>
        public virtual OriginY OriginY { get; set; } = OriginY.Center;

        /// <summary>
        /// The parent origin placement on the X axis.
        /// </summary>
        public virtual OriginX ParentOriginX { get; set; } = OriginX.Center;

        /// <summary>
        /// The parent origin placement on the Y axis.
        /// </summary>
        public virtual OriginY ParentOriginY { get; set; } = OriginY.Center;

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
        public string UserDataInfo { get; set; } = "";

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

        /// <summary>
        /// Calculates the world position of the pane including the origin and parent origin alignment
        /// </summary>
        public Vector3 GetLocalPosition()
        {
            var base_pos_x = this.GetBasePositionX(OriginX);
            var base_pos_y = this.GetBasePositionY(OriginY);

            if (this.Parent != null)
            {
                base_pos_x += this.Parent.GetBasePositionX(ParentOriginX);
                base_pos_y += this.Parent.GetBasePositionY(ParentOriginY);
            }

            return new Vector3(
                    this.Translate.X + base_pos_x,
                    this.Translate.Y + base_pos_y,
                    this.Translate.Z);
        }

        public float GetBasePositionX(OriginX originX)
        {
            switch (originX)
            {
                case OriginX.Center: return 0;
                case OriginX.Left: return -this.Width / 2;
                case OriginX.Right: return this.Width / 2;
                default:
                    throw new Exception($"Unknown origin x type {this.OriginX}");
            }
        }

        public float GetBasePositionY(OriginY originY)
        {
            switch (originY)
            {
                case OriginY.Center: return 0;
                case OriginY.Bottom: return -this.Height / 2;
                case OriginY.Top: return this.Height / 2;
                default:
                    throw new Exception($"Unknown origin x type {this.OriginX}");
            }
        }

        public Pane() { }

        public Pane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal virtual void Read(FileReader reader, LayoutHeader header)
        {
            if (header.IsCTR || header.IsRev)
                ReaCtrRev(reader);
            else
                ReadCafe(reader);
        }

        internal virtual void Write(FileWriter writer, LayoutHeader header)
        {
            if (header.IsCTR || header.IsRev)
                WriteCtrRev(writer);
            else
                WriteCafe(writer);
        }

        internal void ReaCtrRev(FileReader reader)
        {
            this.Flags1 = reader.ReadByte();
            byte origin = reader.ReadByte();
            this.Alpha = reader.ReadByte();
            this.PaneMagFlags = reader.ReadByte();
            this.Name = reader.ReadFixedString(0x10);
            this.UserDataInfo = reader.ReadFixedString(0x8);
            this.Translate = reader.ReadVec3();
            this.Rotate = reader.ReadVec3();
            this.Scale = reader.ReadVec2();
            this.Width = reader.ReadSingle();
            this.Height = reader.ReadSingle();

            switch ((origin % 3))
            {
                case 0: this.OriginX = OriginX.Left; break;
                case 1: this.OriginX = OriginX.Center; break;
                case 2: this.OriginX = OriginX.Right; break;
            }

            switch ((origin / 3))
            {
                case 0: this.OriginY = OriginY.Top; break;
                case 1: this.OriginY = OriginY.Center; break;
                case 2: this.OriginY = OriginY.Bottom; break;
            }
        }

        internal void ReadCafe(FileReader reader)
        {
            this.Flags1 = reader.ReadByte();
            byte origin = reader.ReadByte();
            this.Alpha = reader.ReadByte();
            this.PaneMagFlags = reader.ReadByte();
            this.Name = reader.ReadFixedString(0x18);
            this.UserDataInfo = reader.ReadFixedString(0x8);
            this.Translate = reader.ReadVec3();
            this.Rotate = reader.ReadVec3();
            this.Scale = reader.ReadVec2();
            this.Width = reader.ReadSingle();
            this.Height = reader.ReadSingle();

            int mainorigin = origin % 16;
            int parentorigin = origin / 16;

            this.OriginX = (OriginX)(mainorigin % 4);
            this.OriginY = (OriginY)(mainorigin / 4);
            this.ParentOriginX = (OriginX)(parentorigin % 4);
            this.ParentOriginY = (OriginY)(parentorigin / 4);
        }

        internal void WriteCtrRev(FileWriter writer)
        {
            byte originL = 0;
            byte originH = 0;

            switch (this.OriginX)
            {
                case OriginX.Left: originL = 0; break;
                case OriginX.Center: originL = 1; break;
                case OriginX.Right: originL = 2; break;
            }
            switch (this.OriginY)
            {
                case OriginY.Top: originH = 0; break;
                case OriginY.Center: originH = 1; break;
                case OriginY.Bottom: originH = 2; break;
            }

            writer.Write(this.Flags1);
            writer.Write((byte)(((int)originL) + ((int)originH * 3)));
            writer.Write(this.Alpha);
            writer.Write(this.PaneMagFlags);
            writer.WriteFixedString(this.Name, 0x10);
            writer.WriteFixedString(this.UserDataInfo, 0x8);
            writer.Write(this.Translate);
            writer.Write(this.Rotate);
            writer.Write(this.Scale);
            writer.Write(this.Width);
            writer.Write(this.Height);
        }

        internal void WriteCafe(FileWriter writer)
        {
            int originL = (int)this.OriginX;
            int originH = (int)this.OriginY * 4;
            int originPL = (int)this.ParentOriginX;
            int originPH = (int)this.ParentOriginY * 4;
            byte parentOrigin = (byte)((originPL + originPH) * 16);
            byte origin = (byte)(originL + originH + parentOrigin);

            writer.Write(this.Flags1);
            writer.Write(origin);
            writer.Write(this.Alpha);
            writer.Write(this.PaneMagFlags);
            writer.WriteFixedString(this.Name, 0x18);
            writer.WriteFixedString(this.UserDataInfo, 0x8);
            writer.Write(this.Translate);
            writer.Write(this.Rotate);
            writer.Write(this.Scale);
            writer.Write(this.Width);
            writer.Write(this.Height);
        }

        #endregion
    }
}
