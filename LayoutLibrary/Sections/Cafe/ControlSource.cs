using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class ControlSource
    {
        public string Name { get; set; } = "";
        public string ControlName { get; set; } = "";
        public List<string> Panes { get; set; } = new List<string>();
        public List<string> Animations { get; set; } = new List<string>();
        public List<string> PaneStates { get; set; } = new List<string>();
        public List<string> AnimationStates { get; set; } = new List<string>();

        public UserData UserData { get; set; }

        public ControlSource() { }
        public ControlSource(FileReader reader, LayoutHeader header) { Read(reader, header); }

        internal void Read(FileReader reader, LayoutHeader header)
        {
            long pos = reader.Position - 8;

            uint controlNameOffset = reader.ReadUInt32();
            uint paneNameOffset = reader.ReadUInt32();
            ushort paneCount = reader.ReadUInt16();
            ushort animCount = reader.ReadUInt16();
            uint paneArrayOffset = reader.ReadUInt32();
            uint animArrayOffset = reader.ReadUInt32();

            reader.SeekBegin(pos + 28);
            this.Name = reader.ReadZeroTerminatedString();

            reader.SeekBegin(pos + controlNameOffset);
            this.ControlName = reader.ReadZeroTerminatedString();

            reader.SeekBegin(pos + paneNameOffset);
            for (int i = 0; i < paneCount; i++)
                this.Panes.Add(reader.ReadFixedString(24));

            this.AnimationStates = reader.ReadStringOffsets((int)animCount);

            reader.SeekBegin(pos + paneArrayOffset);
            this.PaneStates = reader.ReadStringOffsets((int)paneCount);

            reader.SeekBegin(pos + animArrayOffset);
            this.Animations = reader.ReadStringOffsets((int)animCount);
        }

        internal void Write(FileWriter writer, LayoutHeader header)
        {
            long pos = writer.Position - 8;

            writer.Write(40);
            writer.Write(0); //pane name offset later
            writer.Write((ushort)this.Panes.Count);
            writer.Write((ushort)this.Animations.Count);
            writer.Write(0); //pane array offset later
            writer.Write(0); //anim array offset later

            writer.WriteStringZeroTerminated(this.Name);
            writer.AlignBytes(4);

            writer.WriteUint32Offset(pos + 8, (int)pos);
            writer.WriteStringZeroTerminated(this.ControlName);
            writer.AlignBytes(4);

            writer.WriteUint32Offset(pos + 8 + 4, (int)pos);
            foreach (var pane in this.Panes)
                writer.WriteFixedString(pane, 24);

            writer.WriteStringOffsets(this.AnimationStates);

            writer.WriteUint32Offset(pos + 8 + 12, (int)pos);
            writer.WriteStringOffsets(this.PaneStates);

            writer.WriteUint32Offset(pos + 8 + 16, (int)pos);
            writer.WriteStringOffsets(this.Animations);

            writer.AlignBytes(4);
        }
    }
}
