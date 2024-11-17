using LayoutLibrary.Ctr;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class MaterialCtr : MaterialBase
    {
        /// <summary>
        /// The name of the material.
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// Bitflags describing the contents of the material.
        /// </summary>
        public MaterialBitfield Flags { get; set; }

        /// <summary>
        /// The color kind in bits for each count (total of 8). 
        /// If bit = 1, type is float.
        /// </summary>
        public byte ColorType { get; set; }

        public Color BlackColor { get; set; } = Color.White;
        public Color WhiteColor { get; set; } = Color.White;
        public Color TevColor1 { get; set; } = Color.White;
        public Color TevColor2 { get; set; } = Color.White;
        public Color TevColor3 { get; set; } = Color.White;
        public Color TevColor4 { get; set; } = Color.White;
        public Color TevColor5 { get; set; } = Color.White;

        public List<MaterialTextureMap> Textures { get; set; } = new List<MaterialTextureMap>();
        public List<MaterialTextureSrt> TextureSrts { get; set; } = new List<MaterialTextureSrt>();
        public List<MaterialTexCoordGen> TexCoordGens { get; set; } = new List<MaterialTexCoordGen>();
        public List<ProjectionTexGenParam> ProjectionTexGens { get; set; } = new List<ProjectionTexGenParam>();
        public List<MaterialTextureExtension> TextureExtensions { get; set; } = new List<MaterialTextureExtension>();
        public List<TevCombinerCtr> TevCombiners { get; set; } = new List<TevCombinerCtr>();
        public List<AlphaCompare> AlphaCompares { get; set; } = new List<AlphaCompare>();

        public BlendMode ColorBlend = new BlendMode();
        public BlendMode AlphaBlend = new BlendMode();

        public List<IndirectParameter> IndirectSrts = new List<IndirectParameter>();

        public FontShadowParameter FontShadowParameter = new FontShadowParameter();

        public MaterialCtr() { }

        public MaterialCtr(FileReader reader, LayoutHeader header, int size) { Read(reader, header, size); }

        internal void Read(FileReader reader, LayoutHeader header, int size)
        {
            var pos = reader.Position;

            MaterialCtr mat = this;
            mat.Name = reader.ReadFixedString(0x14);
            mat.BlackColor = new Color(reader.ReadUInt32());
            mat.WhiteColor = new Color(reader.ReadUInt32());
            //TevConstantColors 
            mat.TevColor1 = new Color(reader.ReadUInt32());
            mat.TevColor2 = new Color(reader.ReadUInt32());
            mat.TevColor3 = new Color(reader.ReadUInt32());
            mat.TevColor4 = new Color(reader.ReadUInt32());
            mat.TevColor5 = new Color(reader.ReadUInt32());

            mat.Flags = new MaterialBitfield(reader.ReadUInt32());

            for (int i = 0; i < mat.Flags.TexMapCount; i++)
            {
                mat.Textures.Add(new MaterialTextureMap()
                {
                    TextureIndex = reader.ReadUInt16(),
                    Flag1 = reader.ReadByte(),
                    Flag2 = reader.ReadByte(),
                });
            }

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

                mat.TexCoordGens.Add(texCoordGen);
            }

            for (int i = 0; i < mat.Flags.TevCombinerCount; i++)
                mat.TevCombiners.Add(new TevCombinerCtr(reader));

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

            for (int i = 0; i < mat.Flags.ProjectionTexGenCount; i++)
            {
                ProjectionTexGenParam texCoordGen = new ProjectionTexGenParam();
                texCoordGen.Position = reader.ReadVec2();
                texCoordGen.Scale = reader.ReadVec2();
                texCoordGen.Flags = reader.ReadUInt32();
                mat.ProjectionTexGens.Add(texCoordGen);
            }

            if (mat.Flags.EnableFontShadowParams)
            {
                mat.FontShadowParameter = new FontShadowParameter()
                {
                    BlackColor = new Color(reader.ReadUInt32()),
                    WhiteColor = new Color(reader.ReadUInt32()),
                };
            }

            var end = pos + size;
            //Check how much data is left
            var s = end - reader.Position;

            if (s != 0)
                Console.WriteLine($"size left over {s.ToString()}" + $" UserCombinerCount {mat.Flags.UserCombinerCount} DetailedCombinerCount {mat.Flags.UseDetailedCombiner} TevCombinerCount {mat.Flags.TevCombinerCount}");
        }

        internal override void WriteMaterial(FileWriter writer, LayoutHeader header)
        {
            MaterialCtr mat = this;

            writer.WriteFixedString(mat.Name, 0x14);

            writer.Write(mat.BlackColor.ToUInt32());
            writer.Write(mat.WhiteColor.ToUInt32());
            //TevConstantColors 
            writer.Write(mat.TevColor1.ToUInt32());
            writer.Write(mat.TevColor2.ToUInt32());
            writer.Write(mat.TevColor3.ToUInt32());
            writer.Write(mat.TevColor4.ToUInt32());
            writer.Write(mat.TevColor5.ToUInt32());
            writer.Write(mat.Flags.ToUInt32());

            mat.Flags.TexMapCount = (byte)mat.Textures.Count;
            mat.Flags.TexSrtCount = (byte)mat.TextureSrts.Count;
            mat.Flags.TexCoordGenCount = (byte)mat.TexCoordGens.Count;
            mat.Flags.TevCombinerCount = (byte)mat.TevCombiners.Count;
            mat.Flags.AlphaCompareCount = (byte)mat.AlphaCompares.Count;
            mat.Flags.ProjectionTexGenCount = (byte)mat.ProjectionTexGens.Count;
            mat.Flags.TevCombinerCount = (byte)(mat.TevCombiners.Count);
            mat.Flags.IndirectSrtCount = (byte)(mat.IndirectSrts.Count);

            for (int i = 0; i < mat.Textures.Count; i++)
            {
                writer.Write(mat.Textures[i].TextureIndex);
                writer.Write(mat.Textures[i].Flag1);
                writer.Write(mat.Textures[i].Flag2);
            }

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
            }

            for (int i = 0; i < mat.TevCombiners.Count; i++)
                mat.TevCombiners[i].Write(writer);

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

            for (int i = 0; i < mat.ProjectionTexGens.Count; i++)
            {
                writer.Write(mat.ProjectionTexGens[i].Position);
                writer.Write(mat.ProjectionTexGens[i].Scale);
                writer.Write(mat.ProjectionTexGens[i].Flags);
            }

            if (mat.Flags.EnableFontShadowParams)
            {
                writer.Write(mat.FontShadowParameter.BlackColor.ToUInt32());
                writer.Write(mat.FontShadowParameter.WhiteColor.ToUInt32());
            }
        }
    }
}
