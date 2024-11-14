using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// Represents an animation group that animates target data.
    /// </summary>
    public class AnimationInfo
    {
        /// <summary>
        /// The magic kind that the info represents.
        /// </summary>
        public string Kind;

        /// <summary>
        /// The animation target/track which stores animation curves.
        /// </summary>
        public List<AnimationTarget> Targets = new List<AnimationTarget>();

        public void Read(FileReader reader)
        {
            long pos = reader.Position;

            Kind = Encoding.ASCII.GetString(reader.ReadBytes(4));
            byte numTargets = reader.ReadByte();
            reader.ReadByte(); // 0
            reader.ReadUInt16(); // 0

            uint[] offsets = reader.ReadUInt32s(numTargets);
            for (int i = 0; i < offsets.Length; i++)
            {
                long target_pos = reader.Position;

                reader.SeekBegin(pos + offsets[i]);

                AnimationTarget target = new AnimationTarget();
                target.Index = reader.ReadByte();
                target.Target = reader.ReadByte();
                target.CurveType = (AnimCurveType)reader.ReadByte();
                reader.ReadByte(); // padding

                ushort numKeyFrames = reader.ReadUInt16();
                reader.ReadUInt16(); // padding

                uint keyFrameOffset = reader.ReadUInt32();

                reader.SeekBegin(target_pos + keyFrameOffset);
                for (int j = 0; j < numKeyFrames; j++)
                {
                    switch (target.CurveType)
                    {
                        case AnimCurveType.Step:
                            target.KeyFrames.Add(new KeyFrame()
                            {
                                Frame = reader.ReadSingle(),
                                Value = reader.ReadUInt16(),
                            });
                            reader.ReadUInt16(); // padding
                            break;
                        case AnimCurveType.Hermite:
                            target.KeyFrames.Add(new KeyFrame()
                            {
                                Frame = reader.ReadSingle(),
                                Value = reader.ReadSingle(),
                                Slope = reader.ReadSingle(),
                            });
                            break;
                        case AnimCurveType.Constant: // Todo check constant type
                            target.KeyFrames.Add(new KeyFrame()
                            {
                                Frame = reader.ReadSingle(),
                                Value = reader.ReadSingle(),
                            });
                            break;
                    }
                }

                Targets.Add(target);
            }
        }

        public void Write(FileWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes(Kind));
            writer.Write((byte)Targets.Count);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);

            long pos = writer.Position;
            writer.Write(new uint[Targets.Count]);

            for (int i = 0; i < Targets.Count; i++)
            {
                var targetStart = writer.Position;

                writer.WriteUint32Offset(pos + i * 4, (int)pos - 8);
                writer.Write((byte)Targets[i].Index);
                writer.Write((byte)Targets[i].Target);
                writer.Write((byte)Targets[i].CurveType);
                writer.Write((byte)0); // padding

                writer.Write((ushort)Targets[i].KeyFrames.Count);
                writer.Write((ushort)0); // padding
                writer.Write(0); // Key frame offset

                writer.WriteUint32Offset(targetStart + 8, (int)targetStart);

                foreach (var keyFrame in Targets[i].KeyFrames)
                {
                    switch (Targets[i].CurveType)
                    {
                        case AnimCurveType.Step:
                            writer.Write(keyFrame.Frame);
                            writer.Write((ushort)keyFrame.Value);
                            writer.Write((ushort)0); // padding
                            break;
                        case AnimCurveType.Hermite:
                            writer.Write(keyFrame.Frame);
                            writer.Write(keyFrame.Value);
                            writer.Write(keyFrame.Slope);
                            break;
                        case AnimCurveType.Constant: // Todo check constant type
                            writer.Write(keyFrame.Frame);
                            writer.Write(keyFrame.Value);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Animation target that stores curve data and what type to target.
    /// </summary>
    public class AnimationTarget
    {
        /// <summary>
        /// The index of the item to target.
        /// </summary>
        public byte Index;

        /// <summary>
        /// The target type.
        /// </summary>
        public byte Target;

        /// <summary>
        /// The interpolation type for handling animation curves.
        /// </summary>
        public AnimCurveType CurveType;

        /// <summary>
        /// The key frame data.
        /// </summary>
        public List<KeyFrame> KeyFrames = new List<KeyFrame>();
    }

    /// <summary>
    /// A key frame for animating data.
    /// The way these are used depends on the AnimCurveType.
    /// </summary>
    public class KeyFrame
    {
        /// <summary>
        /// The frame this key is displayed at.
        /// </summary>
        public float Frame;

        /// <summary>
        /// The value.
        /// </summary>
        public float Value;

        /// <summary>
        /// The Hermite in/out slope if AnimCurveType is Hermite.
        /// </summary>
        public float Slope;
    }
}
