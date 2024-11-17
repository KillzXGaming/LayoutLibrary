using LayoutLibrary.Files;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// Represents an animation group that animates target data.
    /// </summary>
    public class AnimationInfoGroup
    {
        /// <summary>
        /// The name based on the group type (pane or material).
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The type of object to target (pane or material)
        /// </summary>
        public AnimationTargetType Type;

        /// <summary>
        /// The animation target/track which stores animation curves.
        /// </summary>
        public List<AnimationInfoSubGroup> Tags = new List<AnimationInfoSubGroup>();

        /// <summary>
        /// Unknown user value used in v8 >=
        /// </summary>
        public uint UserValue = 4;

        /// <summary>
        /// Unknown user name value used in v8 >=
        /// </summary>
        public string UserName = "";

        public void Read(FileReader reader, LayoutHeader layout)
        {
            long startPos = reader.Position;

            Name = reader.ReadFixedString(layout.VersionMajor == 1 ? 20 : 28);
            var numTags = reader.ReadByte();
            Type = (AnimationTargetType)reader.ReadByte();
            reader.ReadUInt16(); //padding

            var offsets = reader.ReadUInt32s(numTags);

            if (Type == AnimationTargetType.User)
            {
                uint userNameOffset = reader.ReadUInt32();
                reader.SeekBegin(startPos + userNameOffset);
                UserValue = reader.ReadUInt32();
                UserName = reader.ReadFixedString(16);
            }

            for (int i = 0; i < numTags; i++)
            {
                reader.SeekBegin(startPos + offsets[i]);

                if (Type == AnimationTargetType.User)
                {
                    uint tagOffset = reader.ReadUInt32();
                    reader.SeekBegin(startPos + tagOffset);
                }

                AnimationInfoSubGroup info = new AnimationInfoSubGroup();
                info.Read(reader, (int)Type);
                Tags.Add(info);
            }
        }

        public void Write(FileWriter writer, LayoutHeader layout)
        {
            long startPos = writer.Position;

            writer.WriteFixedString(Name, layout.VersionMajor == 1 ? 20 : 28);
            writer.Write((byte)Tags.Count);
            writer.Write((byte)Type);
            writer.Write((ushort)0);

            long ofsPos = writer.Position;

            writer.Write(new uint[Tags.Count]);

            var ofsUserNameOfs = writer.Position;
            if (Type == AnimationTargetType.User)
            {
                writer.Write(0); // userNameOffset later
            }

            if (Tags.Count > 0)
            {
                for (int i = 0; i < Tags.Count; i++)
                {
                    writer.WriteUint32Offset(ofsPos + (i * 4), (int)startPos);

                    if (Type == AnimationTargetType.User)
                    {
                        var ofs = writer.Position - startPos + 4;
                        writer.Write((uint)ofs); // tag offset
                    }

                    Tags[i].Write(writer, (int)Type);
                }
            }

            if (Type == AnimationTargetType.User)
            {
                writer.WriteUint32Offset(ofsUserNameOfs, (int)startPos);
                writer.Write(UserValue);
                writer.WriteFixedString(UserName, 16);
            }
        }
    }
}
