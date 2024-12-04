using LayoutLibrary.Cafe;
using LayoutLibrary.Ctr;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class MaterialCafe : MaterialBase
    {
        /// <summary>
        /// The name of the material.
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// Bitflags describing the contents of the material.
        /// </summary>
        public MaterialBitfield Flags { get; set; } = new MaterialBitfield(0);

        /// <summary>
        /// The color kind in bits for each count (total of 8). 
        /// If bit = 1, type is float.
        /// </summary>
        public byte ColorType { get; set; }

        public List<Color> Colors { get; set; } = new List<Color>();

        public List<MaterialTextureMap> Textures { get; set; } = new List<MaterialTextureMap>();
        public List<MaterialTextureSrt> TextureSrts { get; set; } = new List<MaterialTextureSrt>();
        public List<MaterialTexCoordGen> TexCoordGens { get; set; } = new List<MaterialTexCoordGen>();
        public List<ProjectionTexGenParam> ProjectionTexGens { get; set; } = new List<ProjectionTexGenParam>();
        public List<MaterialTextureExtension> TextureExtensions { get; set; } = new List<MaterialTextureExtension>();
        public List<MaterialTevCombiner> TevCombiners { get; set; } = new List<MaterialTevCombiner>();

        public MaterialDetailedCombiner MaterialDetailedCombiner = new MaterialDetailedCombiner();
        public List<MaterialUserCombiner> UserCombiners { get; set; } = new List<MaterialUserCombiner>();
        public List<AlphaCompare> AlphaCompares { get; set; } = new List<AlphaCompare>();

        public BlendMode ColorBlend;
        public BlendMode AlphaBlend;

        public Color BlackColor => Colors.Count > 0 ? Colors[0] : Color.Black;
        public Color WhiteColor => Colors.Count > 1 ? Colors[1] : Color.White;

        public List<IndirectParameter> IndirectSrts = new List<IndirectParameter>();

        public FontShadowParameter FontShadowParameter;

        public List<BrickRepeatShaderInfo> BrickRepeatShaderInfos { get; set; } = new List<BrickRepeatShaderInfo>();

        public byte[] Raw;

        public MaterialCafe() { }
        public MaterialCafe(FileReader reader, LayoutHeader header, int size) { Read(reader, header, size); }


        internal void Read(FileReader reader, LayoutHeader header, int size)
        {
            var pos = reader.Position;

            MaterialCafe mat = this;

            mat.Name = reader.ReadFixedString(0x1C);
            if (header.VersionMajor >= 8)
            {
                mat.Flags = new MaterialBitfield(reader.ReadUInt32());

                var cpos = reader.Position;
                mat.ColorType = reader.ReadByte();
                var colorCount = reader.ReadByte();

                byte[] colorOffsets = reader.ReadBytes((int)colorCount);

                for (int i = 0; i < colorCount; i++)
                {
                    reader.SeekBegin(cpos + colorOffsets[i]);
                    var type = mat.ColorType >> i;
                    if (type == 0)
                        mat.Colors.Add(new Color(reader.ReadUInt32()));
                    else if (type == 1)
                        mat.Colors.Add(new Color(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()));

                }
            }
            else
            {
                mat.Colors.Add(new Color(reader.ReadUInt32()));
                mat.Colors.Add(new Color(reader.ReadUInt32()));
                mat.Flags = new MaterialBitfield(reader.ReadUInt32());
            }

            var start = reader.Position;
            var end = pos + size;

            var diff = start - pos;

            mat.Raw = reader.ReadBytes((int)size - (int)diff);

            reader.SeekBegin(start);
            for (int i = 0; i < mat.Flags.TexMapCount; i++)
            {
                mat.Textures.Add(new MaterialTextureMap()
                {
                    TextureIndex = reader.ReadUInt16(),
                    Flag1 = reader.ReadByte(),
                    Flag2 = reader.ReadByte(),
                });
            }

            if (mat.Flags.HasTextureExtensions)
                mat.TextureExtensions.Add(new MaterialTextureExtension(reader.ReadInt32()));

            for (int i = 0; i < mat.Flags.TexSrtCount; i++)
            {
                mat.TextureSrts.Add(new MaterialTextureSrt()
                {
                    Translate = reader.ReadVec2(),
                    Rotate = reader.ReadSingle(),
                    Scale = reader.ReadVec2(),
                });
            }

            for (int i = 0; i < mat.Flags.TexCoordGenCount; i++)
            {
                MaterialTexCoordGen texCoordGen = new MaterialTexCoordGen();
                texCoordGen.MatrixType = (TexGenMatrixType)reader.ReadByte();
                texCoordGen.Source = (TexGenType)reader.ReadByte();
                texCoordGen.Unknown = reader.ReadUInt16();
                texCoordGen.Unknown2 = reader.ReadUInt32();

                if (header.VersionMajor >= 8)
                    texCoordGen.Unknown3 = reader.ReadUInt64();

                mat.TexCoordGens.Add(texCoordGen);
            }

            for (int i = 0; i < mat.Flags.TevCombinerCount; i++)
            {
                mat.TevCombiners.Add(new MaterialTevCombiner()
                {
                    ColorFlags = reader.ReadByte(),
                    AlphaFlags = reader.ReadByte(),
                    Reserved1 = reader.ReadByte(),
                    Reserved2 = reader.ReadByte(),
                });
            }

            for (int i = 0; i < mat.Flags.AlphaCompareCount; i++)
            {
                var mode = reader.ReadByte();
                reader.ReadBytes(0x3);
                var value = reader.ReadSingle();

                mat.AlphaCompares.Add(new AlphaCompare()
                {
                    CompareMode = (GfxAlphaFunction)mode,
                    Value = value
                });
            }

            if (mat.Flags.ColorBlendMode)
                mat.ColorBlend = new BlendMode()
                {
                    BlendOp = (GfxBlendOp)reader.ReadByte(),
                    SourceFactor = (GfxBlendFactor)reader.ReadByte(),
                    DestFactor = (GfxBlendFactor)reader.ReadByte(),
                    LogicOp = (GfxLogicOp)reader.ReadByte(),
                };

            if (mat.Flags.ColorAndAlphaBlendMode)
                mat.AlphaBlend = new BlendMode()
                {
                    BlendOp = (GfxBlendOp)reader.ReadByte(),
                    SourceFactor = (GfxBlendFactor)reader.ReadByte(),
                    DestFactor = (GfxBlendFactor)reader.ReadByte(),
                    LogicOp = (GfxLogicOp)reader.ReadByte(),
                };

            for (int i = 0; i < mat.Flags.IndirectSrtCount; i++)
            {
                mat.IndirectSrts.Add(new IndirectParameter()
                {
                    Rotation = reader.ReadSingle(),
                    Scale = reader.ReadVec2(),
                });
            }

            if (mat.Flags.UseDetailedCombiner)
            {
                mat.MaterialDetailedCombiner = new MaterialDetailedCombiner()
                {
                    Value1 = reader.ReadUInt32(),
                    Color1 = new Color(reader.ReadUInt32()),
                    Color2 = new Color(reader.ReadUInt32()),
                    Color3 = new Color(reader.ReadUInt32()),
                    Color4 = new Color(reader.ReadUInt32()),
                    Color5 = new Color(reader.ReadUInt32()),
                    Color6 = new Color(reader.ReadUInt32()),
                };
                for (int i = 0; i < mat.Flags.TevCombinerCount; i++)
                    mat.MaterialDetailedCombiner.Entries.Add(new MaterialDetailedCombinerEntry(reader));
            }

            for (int i = 0; i < mat.Flags.ProjectionTexGenCount; i++)
            {
                ProjectionTexGenParam texCoordGen = new ProjectionTexGenParam();
                texCoordGen.Position = reader.ReadVec2();
                texCoordGen.Scale = reader.ReadVec2();
                texCoordGen.Flags = reader.ReadUInt32();
                mat.ProjectionTexGens.Add(texCoordGen);
            }

            for (int i = 0; i < mat.Flags.UserCombinerCount; i++)
            {
                mat.UserCombiners.Add(new MaterialUserCombiner()
                {
                    Name = reader.ReadFixedString(0x60),
                    Color1 = new Color(reader.ReadUInt32()),
                    Color2 = new Color(reader.ReadUInt32()),
                    Color3 = new Color(reader.ReadUInt32()),
                    Color4 = new Color(reader.ReadUInt32()),
                    Color5 = new Color(reader.ReadUInt32()),
                });
            }

            if (mat.Flags.EnableFontShadowParams)
            {
                mat.FontShadowParameter = new FontShadowParameter()
                {
                    BlackColor = new Color(reader.ReadUInt32()),
                    WhiteColor = new Color(reader.ReadUInt32()),
                };
            }

            for (int i = 0; i < mat.Flags.BrickRepeatShaderInfoCount; i++)
            {
                BrickRepeatShaderInfo info = new BrickRepeatShaderInfo();
                info.Scale1 = reader.ReadVec2();
                info.Offset1 = reader.ReadVec2();
                info.Scale2 = reader.ReadVec2();
                info.Offset2 = reader.ReadVec2();
                info.Unknown1 = reader.ReadVec4();
                info.Unknown2 = reader.ReadVec2(); // 0 1
                info.RotationRange = reader.ReadVec2(); // Rotation range? -180 180
                info.Unknown3 = reader.ReadVec2();
                info.Unknown4 = reader.ReadVec4();
                mat.BrickRepeatShaderInfos.Add(info);
            }

            //Check how much data is left
            var s = end - reader.Position;

            if (s != 0)
                Console.WriteLine($"size left over {s.ToString()}" + $" UserCombinerCount {mat.Flags.UserCombinerCount} DetailedCombinerCount {mat.Flags.UseDetailedCombiner} TevCombinerCount {mat.Flags.TevCombinerCount}");
        }

        internal override void WriteMaterial(FileWriter writer, LayoutHeader header)
        {
            MaterialCafe mat = this;

            mat.Flags.TexMapCount = (byte)mat.Textures.Count;
            mat.Flags.TexSrtCount = (byte)mat.TextureSrts.Count;
            mat.Flags.TexCoordGenCount = (byte)mat.TexCoordGens.Count;
            mat.Flags.TevCombinerCount = (byte)mat.TevCombiners.Count;
            mat.Flags.AlphaCompareCount = (byte)mat.AlphaCompares.Count;
            mat.Flags.ProjectionTexGenCount = (byte)mat.ProjectionTexGens.Count;
            mat.Flags.IndirectSrtCount = (byte)(mat.IndirectSrts.Count);
            mat.Flags.UserCombinerCount = (byte)mat.UserCombiners.Count;
            mat.Flags.BrickRepeatShaderInfoCount = (byte)mat.BrickRepeatShaderInfos.Count;

            mat.Flags.ColorBlendMode = mat.ColorBlend != null;
            mat.Flags.ColorAndAlphaBlendMode = mat.AlphaBlend != null;
            mat.Flags.EnableFontShadowParams = mat.FontShadowParameter != null;
            mat.Flags.UseDetailedCombiner = mat.MaterialDetailedCombiner.Entries.Count > 0;

            writer.WriteFixedString(mat.Name, 0x1C);

            if (header.VersionMajor >= 8)
            {
                writer.Write(mat.Flags.ToUInt32());

                var cpos = writer.Position;
                writer.Write((byte)mat.ColorType);
                writer.Write((byte)mat.Colors.Count);
                writer.Write(new byte[mat.Colors.Count]);

                for (int i = 0; i < mat.Colors.Count; i++)
                {
                    writer.WriteByteOffset(cpos + 2 + (i), (int)cpos);

                    var type = mat.ColorType >> i;
                    if (type == 0)
                        writer.Write(mat.Colors[i].ToUInt32());
                    else
                    {
                        writer.Write(mat.Colors[i].R);
                        writer.Write(mat.Colors[i].G);
                        writer.Write(mat.Colors[i].B);
                        writer.Write(mat.Colors[i].A);
                    }
                }
            }
            else
            {
                writer.Write(mat.BlackColor.ToUInt32());
                writer.Write(mat.WhiteColor.ToUInt32());
                writer.Write(mat.Flags.ToUInt32());
            }

            for (int i = 0; i < mat.Textures.Count; i++)
            {
                writer.Write(mat.Textures[i].TextureIndex);
                writer.Write(mat.Textures[i].Flag1);
                writer.Write(mat.Textures[i].Flag2);
            }

            if (mat.Flags.HasTextureExtensions)
                writer.Write(mat.TextureExtensions[0].Flags);

            for (int i = 0; i < mat.TextureSrts.Count; i++)
            {
                writer.Write(mat.TextureSrts[i].Translate);
                writer.Write(mat.TextureSrts[i].Rotate);
                writer.Write(mat.TextureSrts[i].Scale);
            }

            for (int i = 0; i < mat.TexCoordGens.Count; i++)
            {
                writer.Write((byte)mat.TexCoordGens[i].MatrixType);
                writer.Write((byte)mat.TexCoordGens[i].Source);
                writer.Write((ushort)mat.TexCoordGens[i].Unknown);
                writer.Write(mat.TexCoordGens[i].Unknown2);
                if (header.VersionMajor >= 8)
                    writer.Write(mat.TexCoordGens[i].Unknown3);
            }

            for (int i = 0; i < mat.TevCombiners.Count; i++)
            {
                writer.Write((byte)mat.TevCombiners[i].ColorFlags);
                writer.Write((byte)mat.TevCombiners[i].AlphaFlags);
                writer.Write((byte)mat.TevCombiners[i].Reserved1);
                writer.Write((byte)mat.TevCombiners[i].Reserved2);
            }

            for (int i = 0; i < mat.AlphaCompares.Count; i++)
            {
                writer.Write((byte)mat.AlphaCompares[i].CompareMode);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write(mat.AlphaCompares[i].Value);
            }

            if (mat.Flags.ColorBlendMode)
            {
                writer.Write((byte)mat.ColorBlend.BlendOp);
                writer.Write((byte)mat.ColorBlend.SourceFactor);
                writer.Write((byte)mat.ColorBlend.DestFactor);
                writer.Write((byte)mat.ColorBlend.LogicOp);
            }
            if (mat.Flags.ColorAndAlphaBlendMode)
            {
                writer.Write((byte)mat.AlphaBlend.BlendOp);
                writer.Write((byte)mat.AlphaBlend.SourceFactor);
                writer.Write((byte)mat.AlphaBlend.DestFactor);
                writer.Write((byte)mat.AlphaBlend.LogicOp);
            }
            for (int i = 0; i < mat.IndirectSrts.Count; i++)
            {
                writer.Write(mat.IndirectSrts[i].Rotation);
                writer.Write(mat.IndirectSrts[i].Scale);
            }

            if (mat.MaterialDetailedCombiner.Entries.Count > 0)
            {
                writer.Write(mat.MaterialDetailedCombiner.Value1);
                writer.Write(mat.MaterialDetailedCombiner.Color1.ToUInt32());
                writer.Write(mat.MaterialDetailedCombiner.Color2.ToUInt32());
                writer.Write(mat.MaterialDetailedCombiner.Color3.ToUInt32());
                writer.Write(mat.MaterialDetailedCombiner.Color4.ToUInt32());
                writer.Write(mat.MaterialDetailedCombiner.Color5.ToUInt32());
                writer.Write(mat.MaterialDetailedCombiner.Color6.ToUInt32());

                for (int i = 0; i < mat.MaterialDetailedCombiner.Entries.Count; i++)
                    mat.MaterialDetailedCombiner.Entries[i].Write(writer);
            }

            for (int i = 0; i < mat.ProjectionTexGens.Count; i++)
            {
                writer.Write(mat.ProjectionTexGens[i].Position);
                writer.Write(mat.ProjectionTexGens[i].Scale);
                writer.Write(mat.ProjectionTexGens[i].Flags);
            }

            for (int i = 0; i < mat.UserCombiners.Count; i++)
            {
                writer.WriteFixedString(mat.UserCombiners[i].Name, 0x60);
                writer.Write(mat.UserCombiners[i].Color1.ToUInt32());
                writer.Write(mat.UserCombiners[i].Color2.ToUInt32());
                writer.Write(mat.UserCombiners[i].Color3.ToUInt32());
                writer.Write(mat.UserCombiners[i].Color4.ToUInt32());
                writer.Write(mat.UserCombiners[i].Color5.ToUInt32());
            }

            if (mat.Flags.EnableFontShadowParams)
            {
                writer.Write(mat.FontShadowParameter.BlackColor.ToUInt32());
                writer.Write(mat.FontShadowParameter.WhiteColor.ToUInt32());
            }

            for (int i = 0; i < mat.BrickRepeatShaderInfos.Count; i++)
            {
                writer.Write(mat.BrickRepeatShaderInfos[i].Scale1);
                writer.Write(mat.BrickRepeatShaderInfos[i].Offset1);
                writer.Write(mat.BrickRepeatShaderInfos[i].Scale2);
                writer.Write(mat.BrickRepeatShaderInfos[i].Offset2);
                writer.Write(mat.BrickRepeatShaderInfos[i].Unknown1);
                writer.Write(mat.BrickRepeatShaderInfos[i].Unknown2);
                writer.Write(mat.BrickRepeatShaderInfos[i].RotationRange);
                writer.Write(mat.BrickRepeatShaderInfos[i].Unknown3);
                writer.Write(mat.BrickRepeatShaderInfos[i].Unknown4);
            }
        }
    }
}
