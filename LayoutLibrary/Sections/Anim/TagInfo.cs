using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace LayoutLibrary
{
    /// <summary>
    /// Tag information for the FLAN/CLAN/RLAN file binaries
    /// </summary>
    public class TagInfo
    {
        /// <summary>
        /// The order in which animations are played in.
        /// </summary>
        public ushort AnimationOrder = 2;

        /// <summary>
        /// The animation name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// User data (only in v8 =>)
        /// </summary>
        public UserData UserData;

        /// <summary>
        /// The start frame to play animations.
        /// </summary>
        public short StartFrame;

        /// <summary>
        /// The end frame top end animations.
        /// </summary>
        public short EndFrame;

        /// <summary>
        /// 
        /// </summary>
        public bool ChildBinding;

        /// <summary>
        /// Unknown, possibly padding values
        /// </summary>
        public byte[] UnknownData = new byte[3];

        /// <summary>
        /// Groups
        /// </summary>
        public List<string> Groups = new List<string>();

        public TagInfo() { }

        public TagInfo(FileReader reader, LayoutHeader header)
        {
            long startPos = reader.Position - 8;

            AnimationOrder = reader.ReadUInt16();
            ushort groupCount = reader.ReadUInt16();
            uint animNameOffset = reader.ReadUInt32();
            uint groupNamesOffset = reader.ReadUInt32();

            uint userDataOffset = 0;
            if (header.VersionMajor >= 8)
                 userDataOffset = reader.ReadUInt32();

            StartFrame = reader.ReadInt16();
            EndFrame = reader.ReadInt16();

            ChildBinding = reader.ReadBoolean();
            UnknownData = reader.ReadBytes(3);

            reader.SeekBegin(startPos + animNameOffset);
            Name = reader.ReadZeroTerminatedString();

            int str_length = header.VersionMajor >= 8 ? 36 : 28;
            if (header.VersionMajor == 1)
                str_length = 20;

            reader.SeekBegin(startPos + groupNamesOffset);
            for (int i = 0; i < groupCount; i++)
                Groups.Add(reader.ReadFixedString(str_length));

            if (userDataOffset != 0)
            {
                reader.SeekBegin(startPos + userDataOffset);
                reader.ReadSignature("usd1");
                reader.ReadUInt32(); //size

                UserData = new UserData(reader, header);
            }
        }

        internal void Write(FileWriter writer, LayoutHeader header)
        {
            long startPos = writer.Position - 8;

            writer.Write(AnimationOrder);
            writer.Write((ushort)Groups.Count);
            writer.Write(uint.MaxValue); //animNameOffset
            writer.Write(uint.MaxValue); //groupNamesOffset

            long userDataOfsPos = writer.Position;
            if (header.VersionMajor >= 8)
                writer.Write(0); //UserData

            writer.Write((ushort)StartFrame);
            writer.Write((ushort)EndFrame);

            writer.Write(ChildBinding);
            writer.Write(UnknownData);

            writer.WriteUint32Offset(startPos + 12, (int)startPos);
            writer.WriteStringZeroTerminated(Name);
            writer.Align(4);

            int str_length = header.VersionMajor >= 8 ? 36 : 28;
            if (header.VersionMajor == 1)
                str_length = 20;

            writer.WriteUint32Offset(startPos + 16, (int)startPos);
            for (int i = 0; i < Groups.Count; i++)
                writer.WriteFixedString(Groups[i], str_length);

            writer.AlignBytes(4);

            if (header.VersionMajor >= 8 && UserData != null)
            {
                writer.WriteUint32Offset(userDataOfsPos, (int)startPos);
                long usdPos = writer.Position ;

                writer.WriteSignature("usd1");
                writer.Write(0); //size later
                UserData.Write(writer, header);
                writer.Align(4);

                writer.WriteSectionSizeU32(usdPos + 4, writer.Position - usdPos);
            }
        }
    }
}
