using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
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

        public Layout() { }

        public Layout(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal void Read(FileReader reader, LayoutHeader header)
        {
            this.DrawFromCenter = reader.ReadBoolean();
            reader.Seek(3); //padding
            this.Width = reader.ReadSingle();
            this.Height = reader.ReadSingle();

            if (header.VersionMajor >= 3)
            {
                this.MaxPartsWidth = reader.ReadSingle();
                this.MaxPartsHeight = reader.ReadSingle();
                this.Name = reader.ReadZeroTerminatedString();
            }
        }

        internal void Write(FileWriter writer, LayoutHeader header)
        {
            writer.Write(this.DrawFromCenter);
            writer.Seek(3);
            writer.Write(this.Width);
            writer.Write(this.Height);

            if (header.VersionMajor >= 3)
            {
                writer.Write(this.MaxPartsWidth);
                writer.Write(this.MaxPartsHeight);
                writer.WriteStringZeroTerminated(this.Name);
                writer.AlignBytes(4);
            }
        }

        #endregion
    }
}
