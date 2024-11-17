using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LayoutLibrary
{
    public class PartsPane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "prt1";

        public float MagnifyX;
        public float MagnifyY;

        public List<PartsProperty> Properties = new List<PartsProperty>();

        public string LayoutFileName;

        public PartsPane() { }
        public PartsPane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal override void Read(FileReader reader, LayoutHeader header)
        {
            long headerStart = reader.Position - 8;

            base.Read(reader, header);

            uint num_properties = reader.ReadUInt32();
            this.MagnifyX = reader.ReadSingle();
            this.MagnifyY = reader.ReadSingle();
            for (int i = 0; i < num_properties; i++)
                this.Properties.Add(new PartsProperty(reader, header, headerStart));
            this.LayoutFileName = reader.ReadZeroTerminatedString();
        }

        internal override void Write(FileWriter writer, LayoutHeader header)
        {
            long pos = writer.Position - 8;

            base.Write(writer, header);

            writer.Write(this.Properties.Count);
            writer.Write(this.MagnifyX);
            writer.Write(this.MagnifyY);

            long[] offsets_start = new long[this.Properties.Count];
            for (int i = 0; i < this.Properties.Count; i++)
            {
                writer.WriteFixedString(this.Properties[i].Name, 0x18);
                writer.Write(this.Properties[i].UsageFlag);
                writer.Write(this.Properties[i].BasicUsageFlag);
                writer.Write(this.Properties[i].MaterialUsageFlag);
                writer.Write(this.Properties[i].Reserved0);

                offsets_start[i] = writer.Position;

                writer.Write(0); //pane_offset
                writer.Write(this.Properties[i].Flag1); //user_data__offset or flag by usage
                writer.Write(0); //pane_info_offset
            }
            writer.Align(4);

            writer.WriteStringZeroTerminated(this.LayoutFileName);
            writer.Align(4);

            // Write sections next
            for (int i = 0; i < this.Properties.Count; i++)
            {
                if (this.Properties[i].Property != null)
                {
                    writer.WriteUint32Offset(offsets_start[i], (int)pos);

                    int item = 0;
                    header.WriteSection(writer, this.Properties[i].Property.Magic,
                        () => header.WritePaneKind(this.Properties[i].Property, writer), ref item);
                }
                if (this.Properties[i].UserData != null)
                {
                    writer.WriteUint32Offset(offsets_start[i] + 4, (int)pos);

                    int item = 0;
                    header.WriteSection(writer, "usd1",
                        () => this.Properties[i].UserData.Write(writer, header), ref item);
                }
                if (this.Properties[i].BasicInfo != null)
                {
                    writer.WriteUint32Offset(offsets_start[i] + 8, (int)pos);
                    writer.WriteFixedString(this.Properties[i].BasicInfo.UserName, 0x8);
                    writer.Write(this.Properties[i].BasicInfo.Translate);
                    writer.Write(this.Properties[i].BasicInfo.Rotate);
                    writer.Write(this.Properties[i].BasicInfo.Scale);
                    writer.Write((byte)this.Properties[i].BasicInfo.Alpha);
                    writer.Write((byte)this.Properties[i].BasicInfo.Reserved0);
                    writer.Write((byte)this.Properties[i].BasicInfo.Reserved1);
                    writer.Write((byte)this.Properties[i].BasicInfo.Reserved2);
                    writer.Write(this.Properties[i].BasicInfo.Reserved3);
                    writer.Write(this.Properties[i].BasicInfo.Reserved4);
                }
            }
        }

        #endregion
    }

    public class PartsProperty
    {
        public string Name;

        public byte UsageFlag;
        public byte BasicUsageFlag;
        public byte MaterialUsageFlag;
        public byte Reserved0;

        public uint Flag1;

        public PartsPaneBasicInfo BasicInfo;

        public UserData UserData;

        public Pane Property;

        public PartsProperty() { }

        public PartsProperty(FileReader reader, LayoutHeader header, long headerStart)
        {
            PartsProperty prop = this;

            prop.Name = reader.ReadFixedString(0x18);
            prop.UsageFlag = reader.ReadByte();
            prop.BasicUsageFlag = reader.ReadByte();
            prop.MaterialUsageFlag = reader.ReadByte();
            prop.Reserved0 = reader.ReadByte();

            uint pane_offset = reader.ReadUInt32();
            uint user_data__offset = reader.ReadUInt32();
            uint pane_info_offset = reader.ReadUInt32();

            // todo usage flag
            prop.Flag1 = user_data__offset;

            var origin = reader.BaseStream.Position;

            if (pane_offset != 0)
            {
                reader.SeekBegin(headerStart + pane_offset);
                prop.Property = header.ReadPaneKind(reader);
            }
            if (user_data__offset != 0 && user_data__offset > 10)
            {
                reader.SeekBegin(headerStart + user_data__offset);
                //magic + size
                reader.ReadSignature("usd1");
                reader.ReadUInt32();

                prop.UserData = new UserData(reader, header);
            }

            if (pane_info_offset != 0)
            {
                reader.SeekBegin(headerStart + pane_info_offset);

                prop.BasicInfo = new PartsPaneBasicInfo();
                prop.BasicInfo.UserName = reader.ReadFixedString(0x8);
                prop.BasicInfo.Translate = reader.ReadVec3();
                prop.BasicInfo.Rotate = reader.ReadVec3();
                prop.BasicInfo.Scale = reader.ReadVec2();
                prop.BasicInfo.Alpha = reader.ReadByte();
                prop.BasicInfo.Reserved0 = reader.ReadByte();
                prop.BasicInfo.Reserved1 = reader.ReadByte();
                prop.BasicInfo.Reserved2 = reader.ReadByte();

                prop.BasicInfo.Reserved3 = reader.ReadUInt32();
                prop.BasicInfo.Reserved4 = reader.ReadUInt32();
            }

            reader.SeekBegin(origin);
        }
    }

    public class PartsPaneBasicInfo
    {   
        public string UserName = "";
        public Vector3 Translate;
        public Vector3 Rotate;
        public Vector2 Scale;
        public byte Alpha;
        public byte Reserved0;
        public byte Reserved1;
        public byte Reserved2;

        public uint Reserved3;
        public uint Reserved4;
    }
}
