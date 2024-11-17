using LayoutLibrary.Cafe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary.Files
{
    /// <summary>
    /// 
    /// </summary>
    public class BflanFile : LayoutHeader
    {
        /// <summary>
        /// 
        /// </summary>
        public TagInfo TagInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AnimationInfo AnimationInfo { get; set; }

        public static bool Identity(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                reader.BaseStream.Position = 0;

                return magic == "FLAN" || magic == "RLAN" || magic == "CLAN";
            }
        }

        public BflanFile() { }
        public BflanFile(string path) : base(path) { }

        public BflanFile(Stream stream) : base(stream) { }

        internal override void CheckMagic(string magic)
        {
             switch (magic)
            {
                case "FLAN": break;
                case "CLAN": break;
                case "RLAN": break;
                case "NALR": break; //RLAN reversed in SM3DAS
                default:
                    throw new Exception($"Unexpected magic {magic}!");
            }
        }

        internal override void ReadSections(FileReader reader, int sectionCount)
        {
            for (int i = 0; i < sectionCount; i++)
            {
                long pos = reader.Position;
                string signature = reader.ReadString(4, Encoding.ASCII);
                uint sectionSize = reader.ReadUInt32();

                switch (signature)
                {
                    case "pat1": //Tag Info
                        TagInfo = new TagInfo(reader, this);
                        break;
                    case "pai1": //Animation Info
                        AnimationInfo = new AnimationInfo(reader, this);
                        break;
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

        internal override void WriteSections(FileWriter writer, ref int numSections)
        {
            WriteSection(writer, "pat1", () => TagInfo.Write(writer, this), ref numSections);
            WriteSection(writer, "pai1", () => AnimationInfo.Write(writer, this), ref numSections);

            foreach (var section in UnsupportedSections)
                WriteSection(writer, section.Magic, () => writer.Write(section.Data), ref numSections);
        }
    }
}
