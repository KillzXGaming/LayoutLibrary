using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LayoutLibrary
{
    /// <summary>
    /// A group for storing pane references.
    /// These can be in a hierachy and hardcoded to do multiple things.
    /// </summary>
    public class Group 
    {
        /// <summary>
        /// The group name.
        /// </summary>
        public string Name { get; set; } = "A_Group";

        /// <summary>
        /// A list of pane references.
        /// </summary>
        public List<string> Panes { get; set; } = new List<string>();

        private Group parent = null;

        /// <summary>
        /// The parent of this group.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Group Parent
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
        /// Children connected to this group.
        /// </summary>
        public List<Group> Children { get; set; } = new List<Group>();

        public Group() { }
        public Group(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        public void Read(FileReader reader, LayoutHeader header)
        {
            if (header.IsCTR || header.IsRev)
                ReadGroupV1(reader, header);
            else
                ReadGroupV2(reader, header);
        }

        internal void Write(FileWriter writer, LayoutHeader header)
        {
            if (header.IsCTR || header.IsRev)
                WriteGroupV1(writer, header);
            else
                WriteGroupV2( writer,  header);
        }

        private void ReadGroupV1(FileReader reader, LayoutHeader header)
        {
            this.Name = reader.ReadFixedString(0x10);
            ushort num_children = reader.ReadUInt16();
            reader.ReadUInt16();
            for (int i = 0; i < num_children; i++)
                this.Panes.Add(reader.ReadFixedString(0x10));
        }

        private void ReadGroupV2(FileReader reader, LayoutHeader header)
        {
            if (header.VersionMajor >= 5)
            {
                this.Name = reader.ReadFixedString(0x21);
                reader.ReadByte();
                ushort num_children = reader.ReadUInt16();
                for (int i = 0; i < num_children; i++)
                    this.Panes.Add(reader.ReadFixedString(0x18));
            }
            else
            {
                this.Name = reader.ReadFixedString(0x18);
                ushort num_children = reader.ReadUInt16();
                reader.ReadUInt16();
                for (int i = 0; i < num_children; i++)
                    this.Panes.Add(reader.ReadFixedString(0x18));
            }
        }

        private void WriteGroupV1(FileWriter writer, LayoutHeader header)
        {
            writer.WriteFixedString(this.Name, 0x10);
            writer.Write((ushort)this.Panes.Count);
            writer.Write((ushort)0);

            for (int i = 0; i < this.Panes.Count; i++)
                writer.WriteFixedString(this.Panes[i], 0x10);
        }

        private void WriteGroupV2(FileWriter writer, LayoutHeader header)
        {
            if (header.VersionMajor >= 5)
            {
                writer.WriteFixedString(this.Name, 0x21);
                writer.Write((byte)0);
                writer.Write((ushort)this.Panes.Count);
            }
            else
            {
                writer.WriteFixedString(this.Name, 0x18);
                writer.Write((ushort)this.Panes.Count);
                writer.Write((ushort)0);
            }
            for (int i = 0; i < this.Panes.Count; i++)
                writer.WriteFixedString(this.Panes[i], 0x18);
        }

        #endregion
    }
}
