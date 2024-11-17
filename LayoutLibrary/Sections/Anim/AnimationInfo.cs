using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// A section from FLAN/CLAN/RLAN file binaries that store animation groups and tracks.
    /// </summary>
    public class AnimationInfo
    {
        /// <summary>
        /// Texture names to be used with pattern type groups that use ushort key types.
        /// </summary>
        public List<string> Textures = new List<string>();

        /// <summary>
        /// The total frame count of the animation
        /// </summary>
        public ushort FrameSize;

        /// <summary>
        /// To loop the animation or not
        /// </summary>
        public bool Loop;

        /// <summary>
        /// Groups of animation for what to animate.
        /// </summary>
        public List<AnimationInfoGroup> Entries = new List<AnimationInfoGroup>();

        public AnimationInfo() { }

        public AnimationInfo(FileReader reader, LayoutHeader header)
        {
            long startPos = reader.Position - 8;

            FrameSize = reader.ReadUInt16();
            Loop = reader.ReadBoolean();
            reader.ReadByte(); //padding
            var numTextures = reader.ReadUInt16();
            var numEntries = reader.ReadUInt16();
            var entryOffsetTbl = reader.ReadUInt32();

            long texStart = reader.Position;
            var texOffsets = reader.ReadUInt32s(numTextures);
            for (int i = 0; i < numTextures; i++)
            {
                reader.SeekBegin(texStart + texOffsets[i]);
                Textures.Add(reader.ReadZeroTerminatedString());
            }

            reader.SeekBegin(startPos + entryOffsetTbl);
            var entryOffsets = reader.ReadUInt32s(numEntries);
            for (int i = 0; i < numEntries; i++)
            {
                reader.SeekBegin(startPos + entryOffsets[i]);

                AnimationInfoGroup info = new AnimationInfoGroup();
                info.Read(reader, header);
                Entries.Add(info);
            }
        }

        internal void Write(FileWriter writer, LayoutHeader header)
        {
            long startPos = writer.Position - 8;

            writer.Write(FrameSize);
            writer.Write(Loop);
            writer.Write((byte)0);
            writer.Write((ushort)Textures.Count);
            writer.Write((ushort)Entries.Count);
            long entryOfsTblPos = writer.Position;
            writer.Write(0);

            if (Textures.Count > 0)
            {
                long startOfsPos = writer.Position;
                writer.Write(new uint[Textures.Count]);
                for (int i = 0; i < Textures.Count; i++)
                {
                    writer.WriteUint32Offset(startOfsPos + (i * 4), (int)startOfsPos);
                    writer.WriteStringZeroTerminated(Textures[i]);
                }
                writer.Align(4);
            }
            if (Entries.Count > 0)
            {
                writer.WriteUint32Offset(entryOfsTblPos, (int)startPos);

                long startOfsPos = writer.Position;
                writer.Write(new uint[Entries.Count]);
                for (int i = 0; i < Entries.Count; i++)
                {
                    writer.WriteUint32Offset(startOfsPos + (i * 4), (int)startPos);
                    Entries[i].Write(writer, header);
                }
            }
        }
    }
}
