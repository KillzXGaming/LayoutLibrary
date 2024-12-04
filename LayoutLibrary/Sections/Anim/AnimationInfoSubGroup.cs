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
    public class AnimationInfoSubGroup
    {
        /// <summary>
        /// The magic kind that the info represents.
        /// </summary>
        public string Kind;

        /// <summary>
        /// The animation target/track which stores animation curves.
        /// </summary>
        public List<AnimationTarget> Targets = new List<AnimationTarget>();

        public string GetGroupName()
        {
            string kind = this.Kind.Remove(0, 1); //type without first char (R/F/C)

            if (TypeDefine.ContainsKey(kind))
                return TypeDefine[kind];

            return Kind;
        }

        public Dictionary<int, string> GetTargetNames()
        {
            Dictionary<int, string> target_types = new Dictionary<int, string>();

            if (TypeEnumDefine.ContainsKey(this.Kind))
            {
                var targetEnum = TypeEnumDefine[this.Kind];
                foreach (int v in Enum.GetValues(targetEnum).Cast<byte>())
                    target_types.Add(v, Enum.GetName(targetEnum, v));
            }

            return target_types;
        }

        public void Read(FileReader reader, int targetType = 0)
        {
            long pos = reader.Position;

            Kind = reader.ReadSignature();
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

        public void Write(FileWriter writer, int targetType = 0)
        {
            writer.WriteSignature(Kind);
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

        public static Dictionary<string, string> TypeDefine = new Dictionary<string, string>()
        {
            {"LPA","PaneSRT" },
            {"LVI","Visibility" },
            {"LTS","TextureSRT" },
            {"LVC","VertexColor" },
            {"LMC","MaterialColor" },
            {"LTP","TexturePattern" },
            {"LIM","IndTextureSRT" },
            {"LAC","AlphaTest" },
            {"LCT","FontShadow" },
            {"LEU","UserData" }, //Guess
        };

        public static Dictionary<string, Type> TypeEnumDefine = new Dictionary<string, Type>()
        {
            //Cafe
            {"FLPA", typeof(LPATarget) },
            {"FLVI", typeof(LVITarget) },
            {"FLTS", typeof(LTSTarget) },
            {"FLVC", typeof(LVCTarget) },
            {"FLMC", typeof(LMCTarget) },
            {"FLTP", typeof(LTPTarget) },
            {"FLIM", typeof(LIMTarget) },
            {"FLCT", typeof(LCTTarget) },

            //Ctr
            {"CLPA", typeof(LPATarget) },
            {"CLVI", typeof(LVITarget) },
            {"CLTS", typeof(LTSTarget) },
            {"CLVC", typeof(LVCTarget) },
            {"CLMC", typeof(LMCTarget) },
            {"CLTP", typeof(LTPTarget) },
            {"CLIM", typeof(LIMTarget) },
            {"CLCT", typeof(LCTTarget) },

            //Rev
            {"RLPA", typeof(LPATarget) },
            {"RLVI", typeof(LVITarget) },
            {"RLTS", typeof(LTSTarget) },
            {"RLVC", typeof(LVCTarget) },
            {"RLMC", typeof(RevLMCTarget) },
            {"RLTP", typeof(LTPTarget) },
            {"RLIM", typeof(LIMTarget) },
            {"RLCT", typeof(LCTTarget) },
        };

        public enum LPATarget : byte
        {
            TranslateX = 0x00,
            TranslateY = 0x01,
            TranslateZ = 0x02,
            RotateX = 0x03,
            RotateY = 0x04,
            RotateZ = 0x05,
            ScaleX = 0x06,
            ScaleY = 0x07,
            SizeX = 0x08,
            SizeY = 0x09,
        }

        public enum LTSTarget : byte
        {
            TranslateS = 0x00,
            TranslateT = 0x01,
            Rotate = 0x02,
            ScaleS = 0x03,
            ScaleT = 0x04,
        }

        public enum LVITarget : byte
        {
            Visibility = 0x00,
        }

        public enum LVCTarget : byte
        {
            LeftTopRed = 0x00,
            LeftTopGreen = 0x01,
            LeftTopBlue = 0x02,
            LeftTopAlpha = 0x03,

            RightTopRed = 0x04,
            RightTopGreen = 0x05,
            RightTopBlue = 0x06,
            RightTopAlpha = 0x07,

            LeftBottomRed = 0x08,
            LeftBottomGreen = 0x09,
            LeftBottomBlue = 0x0A,
            LeftBottomAlpha = 0x0B,

            RightBottomRed = 0x0C,
            RightBottomGreen = 0x0D,
            RightBottomBlue = 0x0E,
            RightBottomAlpha = 0x0F,

            PaneAlpha = 0x10,
        }

        public enum LTPTarget : byte
        {
            Image1 = 0x00,
            Image2 = 0x01, //Unsure if mutliple are used but just in case
            Image3 = 0x02,
        }

        public enum RevLMCTarget : byte
        {
            MatColorRed,
            MatColorGreen,
            MatColorBlue,
            MatColorAlpha,
            BlackColorRed,
            BlackColorGreen,
            BlackColorBlue,
            BlackColorAlpha,
            WhiteColorRed,
            WhiteColorGreen,
            WhiteColorBlue,
            WhiteColorAlpha,
            ColorReg3Red,
            ColorReg3Green,
            ColorReg3Blue,
            ColorReg3Alpha,
            TevColor1Red,
            TevColor1Green,
            TevColor1Blue,
            TevColor1Alpha,
            TevColor2Red,
            TevColor2Green,
            TevColor2Blue,
            TevColor2Alpha,
            TevColor3Red,
            TevColor3Green,
            TevColor3Blue,
            TevColor3Alpha,
            TevColor4Red,
            TevColor4Green,
            TevColor4Blue,
            TevColor4Alpha,
        }

        public enum LMCTarget : byte
        {
            BlackColorRed,
            BlackColorGreen,
            BlackColorBlue,
            BlackColorAlpha,
            WhiteColorRed,
            WhiteColorGreen,
            WhiteColorBlue,
            WhiteColorAlpha,
            TextureColorBlendRatio,
            TexColor0Red,
            TexColor0Green,
            TexColor0Blue,
            TexColor0Alpha,
            TexColor1Red,
            TexColor1Green,
            TexColor1Blue,
            TexColor1Alpha,
            TexColor2Red,
            TexColor2Green,
            TexColor2Blue,
            TexColor2Alpha,
            TevKonstantColor0Red,
            TevKonstantColor0Green,
            TevKonstantColor0Blue,
            TevKonstantColor0Alpha,
            TevKonstantColor1Red,
            TevKonstantColor1Green,
            TevKonstantColor1Blue,
            TevKonstantColor1Alpha,
            TevKonstantColor2Red,
            TevKonstantColor2Green,
            TevKonstantColor2Blue,
            TevKonstantColor2Alpha,
        }

        public enum LIMTarget : byte
        {
            Rotation,
            ScaleU,
            ScaleV,
        }

        public enum LFSTarget : byte
        {
            FontShadowBlackColorRed,
            FontShadowBlackColorGreen,
            FontShadowBlackColorBlue,
            FontShadowBlackColorAlpha,
            FontShadowWhiteColorRed,
            FontShadowWhiteColorGreen,
            FontShadowWhiteColorBlue,
            FontShadowWhiteColorAlpha,
        }

        public enum LCTTarget : byte
        {
            FontShadowBlackColorRed,
            FontShadowBlackColorGreen,
            FontShadowBlackColorBlue,
            FontShadowBlackColorAlpha,
            FontShadowWhiteColorRed,
            FontShadowWhiteColorGreen,
            FontShadowWhiteColorBlue,
            FontShadowWhiteColorAlpha,
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

        public string GetTargetName(AnimationInfoSubGroup group)
        {
            Dictionary<int, string> target_types = group.GetTargetNames();
            if (target_types.ContainsKey(this.Target))
                return target_types[this.Target];

            return this.Target.ToString();
        }
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
