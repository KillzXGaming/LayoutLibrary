using LayoutLibrary.Cafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary.Files
{
    public class LayoutHeader
    {
        //The full version number
        internal uint Version;

        /// <summary>
        /// Version Major.
        /// </summary>
        public byte VersionMajor;

        /// <summary>
        /// Version Minor.
        /// </summary>
        public byte VersionMinor;

        /// <summary>
        /// Version Micro.
        /// </summary>
        public ushort VersionMicro;

        /// <summary>
        /// The byte order.
        /// </summary>
        public ushort ByteOrderMark;

        // header size
        internal ushort HeaderSize;

        /// <summary>
        /// The magic identifier
        /// </summary>
        internal string Magic;

        public bool IsCTR => Magic == "CLYT" || Magic == "CLAN";
        public bool IsRev => Magic == "RLYT" || Magic == "RLAN" || Magic == "TYLR";

        /// <summary>
        /// A list of sections skipped by the tool and are read/written as raw blobs
        /// </summary>
        internal List<UnsupportedSection> UnsupportedSections = new List<UnsupportedSection>();

        public LayoutHeader() { }

        public LayoutHeader(string path) {
            Read(new FileReader(File.OpenRead(path)));
        }

        public LayoutHeader(Stream stream) {
            Read(new FileReader(stream));
        }

        public void Save(string path)
        {
            using (var writer = new FileWriter(path)) {
                Write(writer);
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                Write(writer);
            }
        }

        internal virtual void CheckMagic(string magic)
        {

        }

        internal virtual void ReadSections(FileReader reader, int sectionCount)
        {
            for (int i = 0; i < sectionCount; i++)
            {
                long pos = reader.Position;
                string signature = reader.ReadString(4, Encoding.ASCII);
                uint sectionSize = reader.ReadUInt32();

                switch (signature)
                {
                    default:
                        UnsupportedSections.Add(new UnsupportedSection()
                        {
                            Magic = signature,
                            Data = reader.ReadBytes((int)sectionSize - 8)
                        });
                        break;
                }
                reader.SeekBegin(pos + sectionSize);
            }
        }

        internal virtual void WriteSections(FileWriter writer, ref int numSections)
        {
            foreach (var section in UnsupportedSections)
                WriteSection(writer, section.Magic, () => writer.Write(section.Data), ref numSections);
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);

            Magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            CheckMagic(Magic);

            //Magic reversed in SM3DAS
            if (Magic == "NLAR" || Magic == "TYLR")
            {
                reader.ReverseMagic = true;
                Magic = new string(Magic.Reverse().ToArray());
            }

            ByteOrderMark = reader.ReadUInt16();
            reader.CheckByteOrderMark(ByteOrderMark);

            ushort sectionCount = 0;

            if (Magic == "RLAN" || Magic == "RLYT")
            {
                Version = reader.ReadUInt16();
                uint FileSize = reader.ReadUInt32();
                HeaderSize = reader.ReadUInt16();
                sectionCount = reader.ReadUInt16();

                // Version == 10 / 1.0
                VersionMajor = (byte)(Version / 10); 
                VersionMinor = (byte)(Version % 10);
            }
            else
            {
                HeaderSize = reader.ReadUInt16();
                Version = reader.ReadUInt32();
                uint FileSize = reader.ReadUInt32();
                sectionCount = reader.ReadUInt16();
                ushort Padding = reader.ReadUInt16();

                //Version set
                VersionMajor = (byte)(Version >> 24);
                VersionMinor = (byte)(Version >> 16 & 0xFF);
                VersionMicro = (ushort)(Version & 0xFFFF);
            }

            reader.SeekBegin(HeaderSize);
            ReadSections(reader, sectionCount);
        }

        internal virtual Pane ReadPaneKind(FileReader reader)
        {
            //start of section
            var pos = reader.BaseStream.Position;

            //section magic and size
            string signature = reader.ReadString(4, Encoding.ASCII);
            uint sectionSize = reader.ReadUInt32();

            switch (signature)
            {
                case "pan1": return new Pane(reader, this);
                case "pic1": return new PicturePane(reader, this);
                case "wnd1": return new WindowPane(reader, this);
                case "txt1": return new TextPane(reader, this);
                case "bnd1": return new BoundsPane(reader, this);
                case "scr1": return new ScissorPane(reader, this);
                case "ali1": return new AlignmentPane(reader, this);
                case "prt1": return new PartsPane(reader, this);
                default:
                    throw new Exception($"Unsupported pane kind for part pane {signature}!");
            }
        }


        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(true);

            // Little endian rev types (SM3DAS
            if ((Magic == "RLAN" || Magic == "RLYT") && ByteOrderMark != 0xFEFF)
                writer.ReverseMagic = true;

            writer.WriteSignature(Magic);
            writer.Write(ByteOrderMark);
            writer.CheckByteOrderMark(ByteOrderMark);

            var _ofsFileSize = writer.Position;
            var _ofsBlockNum = writer.Position;

            if (Magic == "RLAN" || Magic == "RLYT")
            {
                HeaderSize = 16;
                Version = (ushort)(VersionMajor * 10 + VersionMinor);

                writer.Write((ushort)Version);
                _ofsFileSize = writer.Position;
                writer.Write(uint.MaxValue);
                writer.Write(HeaderSize);
                _ofsBlockNum = writer.Position;
                writer.Write((ushort)0); //BlockCount
            }
            else
            {
                uint Version =
                    ((uint)VersionMajor << 24) |
                    ((uint)VersionMinor << 16) |
                    (uint)VersionMicro;

                HeaderSize = 20;

                writer.Write(HeaderSize);
                writer.Write(Version);
                _ofsFileSize = writer.Position;
                writer.Write(uint.MaxValue);
                _ofsBlockNum = writer.Position;
                writer.Write((ushort)0); //BlockCount
                writer.Write((ushort)0);
            }

            writer.SeekBegin(HeaderSize);
            var numSections = 0;

            WriteSections(writer, ref numSections);

            //Save Block Count
            using (writer.TemporarySeek(_ofsBlockNum, SeekOrigin.Begin))
            {
                writer.Write((ushort)numSections);
            }

            //Save File size
            using (writer.TemporarySeek(_ofsFileSize, SeekOrigin.Begin))
            {
                writer.Write((uint)writer.BaseStream.Length);
            }
        }

        public void WriteSection(FileWriter writer, string magic, Action writeSection, ref int numSections)
        {
            var pos = writer.Position;
            writer.WriteSignature(magic);
            writer.Write(uint.MaxValue); //section size written later

            writeSection?.Invoke();

            writer.AlignBytes(4);

            var length = writer.Position - pos;
            using (writer.TemporarySeek(pos + 4, SeekOrigin.Begin))
            {
                writer.Write((uint)length);
            }
            numSections++;
        }

        internal void WritePaneKind(Pane pane, FileWriter writer)
        {
            if (pane is PicturePane)
                ((PicturePane)pane).Write(writer, this);
            else if (pane is TextPane) 
                ((TextPane)pane).Write(writer, this);
            else if (pane is PartsPane)
                ((PartsPane)pane).Write(writer, this);
            else if (pane is WindowPane)
                ((WindowPane)pane).Write(writer, this);
            else if (pane is ScissorPane)
                ((ScissorPane)pane).Write(writer, this);
            else if (pane is AlignmentPane) 
                ((AlignmentPane)pane).Write(writer, this);
            else if (pane is BoundsPane)
                ((BoundsPane)pane).Write(writer, this);
            else
                pane.Write(writer, this);
        }
    }
    public class UnsupportedSection
    {
        public string Magic;
        public byte[] Data;

        public void Write(FileWriter writer, BflytFile header)
        {
            writer.Write(Data);
        }
    }
}
