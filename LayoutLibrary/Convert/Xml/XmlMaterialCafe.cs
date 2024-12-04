using LayoutLibrary.Cafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace LayoutLibrary.XmlConverter
{
    public class XmlMaterialCafe : XmlMaterialBase
    {
        public byte ColorType;

        [XmlArrayItem("Color")]
        public List<XmlColor> Colors = new List<XmlColor>();
        [XmlArrayItem("TextureMap")]
        public List<XmlMaterialTextureMap> Textures = new List<XmlMaterialTextureMap>();
        [XmlArrayItem("TextureSrt")]
        public List<XmlMaterialTextureSrt> TextureSrts = new List<XmlMaterialTextureSrt>();
        [XmlArrayItem("TexCoordGen")]
        public List<XmlMaterialTexCoordGen> TexCoordGens = new List<XmlMaterialTexCoordGen>();
        [XmlArrayItem("ProjTexGenParam")]
        public List<XmlProjectionTexGenParam> ProjectionTexGens = new List<XmlProjectionTexGenParam>();
        [XmlArrayItem("IndirectParameter")]
        public List<XmlIndirectParameter> IndirectSrts = new List<XmlIndirectParameter>();
        [XmlArrayItem("TevCombiner")]
        public List<XmlMaterialTevCombiner> TevCombiners = new List<XmlMaterialTevCombiner>();

        public XmlAlphaCompare AlphaCompare;
        public XmlBlendMode ColorBlend;
        public XmlBlendMode AlphaBlend;
        public XmlFontShadowParameter FontShadowParameter;
        public XmlMaterialTextureExtension TextureExtension;

        public XmlMaterialDetailedCombiner DetailedCombiner;

        [XmlArrayItem("UserCombiner")]
        public List<XmlMaterialUserCombiner> UserCombiners = new List<XmlMaterialUserCombiner>();
        [XmlArrayItem("BrickRepeat")]
        public List<BrickRepeatShaderInfo> BrickRepeatShaderInfos = new List<BrickRepeatShaderInfo>();

        public bool UseTextureOnly = false;
        public bool AlphaInterpolation = false;

        public XmlMaterialCafe() { }
        public XmlMaterialCafe(MaterialCafe material, BflytFile bflyt, ushort index)
        {
            this.Name = material.Name;
            this.Index = index;

            this.ColorType = material.ColorType;
            foreach (var clr in material.Colors)
                this.Colors.Add(new XmlColor(clr));

            this.UseTextureOnly = material.Flags.UseTextureOnly;
            this.AlphaInterpolation = material.Flags.AlphaInterpolation != 0;

            foreach (var tex in material.Textures)
                this.Textures.Add(new XmlMaterialTextureMap(tex, bflyt));
            foreach (var tex in material.TexCoordGens)
                this.TexCoordGens.Add(new XmlMaterialTexCoordGen(tex));
            foreach (var tex in material.TextureSrts)
                this.TextureSrts.Add(new XmlMaterialTextureSrt(tex));
            foreach (var tex in material.ProjectionTexGens)
                this.ProjectionTexGens.Add(new XmlProjectionTexGenParam(tex));
            foreach (var srt in material.IndirectSrts)
                this.IndirectSrts.Add(new XmlIndirectParameter(srt));

            if (material.ColorBlend != null)
                this.ColorBlend = new XmlBlendMode(material.ColorBlend);
            if (material.AlphaBlend != null)
                this.AlphaBlend = new XmlBlendMode(material.AlphaBlend);
            if (material.AlphaCompares.Count > 0)
                this.AlphaCompare = new XmlAlphaCompare(material.AlphaCompares[0]);
            if (material.Flags.EnableFontShadowParams)
                this.FontShadowParameter = new XmlFontShadowParameter(material.FontShadowParameter);

            if (material.TextureExtensions.Count > 0)
                TextureExtension = new XmlMaterialTextureExtension() { Flags = material.TextureExtensions[0].Flags };

            if (material.MaterialDetailedCombiner.Entries.Count > 0)
            {
                DetailedCombiner = new XmlMaterialDetailedCombiner()
                {
                    Value1 = material.MaterialDetailedCombiner.Value1,
                    Color1 = new XmlColor(material.MaterialDetailedCombiner.Color1),
                    Color2 = new XmlColor(material.MaterialDetailedCombiner.Color2),
                    Color3 = new XmlColor(material.MaterialDetailedCombiner.Color3),
                    Color4 = new XmlColor(material.MaterialDetailedCombiner.Color4),
                    Color5 = new XmlColor(material.MaterialDetailedCombiner.Color5),
                    Color6 = new XmlColor(material.MaterialDetailedCombiner.Color6),
                };
                foreach (var entry in material.MaterialDetailedCombiner.Entries)
                    DetailedCombiner.Entries.Add(new XmlMaterialDetailedCombinerEntry()
                    {
                        ColorFlags = entry.ColorFlags,
                        AlphaFlags = entry.AlphaFlags,
                        Unknown1 = entry.Unknown1,
                        Unknown2 = entry.Unknown2,
                    });
            }

            foreach (var cmb in material.TevCombiners)
            {
                this.TevCombiners.Add(new XmlMaterialTevCombiner()
                {
                    ColorFlags = cmb.ColorFlags,
                    AlphaFlags = cmb.AlphaFlags,
                    Reserved1 = cmb.Reserved1,
                    Reserved2 = cmb.Reserved2,
                });
            }

            foreach (var cmb in material.UserCombiners)
                this.UserCombiners.Add(new XmlMaterialUserCombiner()
                {
                    Name = cmb.Name,
                    Color1 = new XmlColor(cmb.Color1),
                    Color2 = new XmlColor(cmb.Color2),
                    Color3 = new XmlColor(cmb.Color3),
                    Color4 = new XmlColor(cmb.Color4),
                    Color5 = new XmlColor(cmb.Color5),
                });

            this.BrickRepeatShaderInfos = material.BrickRepeatShaderInfos;

            //Null unused lists to hide in xml
            if (this.Textures.Count == 0) this.Textures = null;
            if (this.TevCombiners.Count == 0) this.TevCombiners = null;
            if (this.ProjectionTexGens.Count == 0) this.ProjectionTexGens = null;
            if (this.TexCoordGens.Count == 0) this.TexCoordGens = null;
            if (this.TextureSrts.Count == 0) this.TextureSrts = null;
            if (this.UserCombiners.Count == 0) this.UserCombiners = null;
            if (this.BrickRepeatShaderInfos.Count == 0) this.BrickRepeatShaderInfos = null;

        }

        public MaterialCafe Create(BflytFile bflyt)
        {
            MaterialCafe mat = new MaterialCafe();
            mat.Name = this.Name;
            mat.Index = this.Index;
            mat.ColorType = this.ColorType;
            foreach (var clr in this.Colors)
                mat.Colors.Add(clr.ToColor());
            mat.Flags.UseTextureOnly = this.UseTextureOnly;
            mat.Flags.AlphaInterpolation = (byte)(this.AlphaInterpolation ? 1 : 0);

            foreach (var tex in this.Textures)
            {
                if (!bflyt.TextureList.Contains(tex.Texture))
                    bflyt.TextureList.Add(tex.Texture);

                mat.Textures.Add(tex.Create(bflyt));
            }

            foreach (var texCoord in this.TexCoordGens)
                mat.TexCoordGens.Add(texCoord.Create());
            foreach (var texSrt in this.TextureSrts)
                mat.TextureSrts.Add(texSrt.Create());
            foreach (var srt in this.IndirectSrts)
                mat.IndirectSrts.Add(srt.Create());
            foreach (var srt in this.ProjectionTexGens)
                mat.ProjectionTexGens.Add(srt.Create());

            if (this.ColorBlend != null)
                mat.ColorBlend = ColorBlend.Create();
            if (this.AlphaBlend != null)
                mat.AlphaBlend = AlphaBlend.Create();
            if (this.AlphaCompare != null)
                mat.AlphaCompares.Add(AlphaCompare.Create());

            if (this.FontShadowParameter != null)
                mat.FontShadowParameter = FontShadowParameter.Create();

            if (TextureExtension != null)
                mat.TextureExtensions.Add(new MaterialTextureExtension(TextureExtension.Flags));

            foreach (var cmb in this.TevCombiners)
            {
                mat.TevCombiners.Add(new MaterialTevCombiner()
                {
                    ColorFlags = cmb.ColorFlags,
                    AlphaFlags = cmb.AlphaFlags,
                    Reserved1 = cmb.Reserved1,
                    Reserved2 = cmb.Reserved2,
                });
            }

            foreach (var cmb in this.UserCombiners)
                mat.UserCombiners.Add(new MaterialUserCombiner()
                {
                    Name = cmb.Name,
                    Color1 = cmb.Color1.ToColor(),
                    Color2 = cmb.Color2.ToColor(),
                    Color3 = cmb.Color3.ToColor(),
                    Color4 = cmb.Color4.ToColor(),
                    Color5 = cmb.Color5.ToColor(),
                });

            if (this.DetailedCombiner != null)
            {
                mat.MaterialDetailedCombiner = new MaterialDetailedCombiner()
                {
                    Value1 = DetailedCombiner.Value1,
                    Color1 = DetailedCombiner.Color1.ToColor(),
                    Color2 = DetailedCombiner.Color2.ToColor(),
                    Color3 = DetailedCombiner.Color3.ToColor(),
                    Color4 = DetailedCombiner.Color4.ToColor(),
                    Color5 = DetailedCombiner.Color5.ToColor(),
                    Color6 = DetailedCombiner.Color6.ToColor(),
                };
                foreach (var entry in DetailedCombiner.Entries)
                    mat.MaterialDetailedCombiner.Entries.Add(new MaterialDetailedCombinerEntry()
                    {
                        ColorFlags = entry.ColorFlags,
                        AlphaFlags = entry.AlphaFlags,
                        Unknown1 = entry.Unknown1,
                        Unknown2 = entry.Unknown2,
                    });
            }

            mat.BrickRepeatShaderInfos = this.BrickRepeatShaderInfos;

            return mat;
        }
    }

    public class XmlMaterialTevCombiner
    {
        [XmlAttribute]
        public byte ColorFlags;
        [XmlAttribute]
        public byte AlphaFlags;
        [XmlAttribute]
        public byte Reserved1;
        [XmlAttribute]
        public byte Reserved2;
    }

    public class XmlMaterialUserCombiner
    {
        [XmlAttribute]
        public string Name; //fixed string 0x60

        public XmlColor Color1;
        public XmlColor Color2;
        public XmlColor Color3;
        public XmlColor Color4;
        public XmlColor Color5;
    }

    public class XmlMaterialTextureExtension
    {
        public int Flags;
    }

    public class XmlFontShadowParameter
    {
        public XmlColor BlackColor;
        public XmlColor WhiteColor;

        public XmlFontShadowParameter() { }
        public XmlFontShadowParameter(FontShadowParameter parameter)
        {
            this.BlackColor = new XmlColor(parameter.BlackColor);
            this.WhiteColor = new XmlColor(parameter.WhiteColor);
        }

        public FontShadowParameter Create() => new FontShadowParameter()
        {
            WhiteColor = this.WhiteColor.ToColor(),
            BlackColor = this.BlackColor.ToColor(),
        };
    }

    public class XmlMaterialTextureMap
    {
        [XmlAttribute]
        public string Texture;
        [XmlAttribute]
        public WrapMode WrapModeX;
        [XmlAttribute]
        public WrapMode WrapModeY;
        [XmlAttribute]
        public FilterMode MinFilterMode;
        [XmlAttribute]
        public FilterMode MagFilterMode;

        public XmlMaterialTextureMap() { }
        public XmlMaterialTextureMap(MaterialTextureMap tex, BflytFile bflyt)
        {
            this.Texture = bflyt.TextureList[tex.TextureIndex];
            this.WrapModeX = tex.WrapModeU;
            this.WrapModeY = tex.WrapModeV;
            this.MagFilterMode = tex.MagFilterMode;
            this.MinFilterMode = tex.MinFilterMode;
        }

        public MaterialTextureMap Create(BflytFile bflyt) => new MaterialTextureMap()
        {
            WrapModeU = this.WrapModeX,
            WrapModeV = this.WrapModeY,
            MagFilterMode = this.MagFilterMode,
            MinFilterMode = this.MinFilterMode,
            TextureIndex = (ushort)bflyt.TextureList.IndexOf(this.Texture),
        };
    }

    public class XmlMaterialTextureSrt
    {
        public XmlVector2 Translate;
        public XmlValue Rotate;
        public XmlVector2 Scale;

        public XmlMaterialTextureSrt() { }
        public XmlMaterialTextureSrt(MaterialTextureSrt srt)
        {
            this.Translate = new XmlVector2(srt.Translate);
            this.Rotate = new XmlValue(srt.Rotate);
            this.Scale = new XmlVector2(srt.Scale);
        }

        public MaterialTextureSrt Create() => new MaterialTextureSrt()
        {
            Translate = new Vector2(Translate.X, Translate.Y),
            Scale = new Vector2(Scale.X, Scale.Y),
            Rotate = Rotate.Value,
        };
    }

    public class XmlProjectionTexGenParam
    {
        public XmlVector2 Position;
        public XmlVector2 Scale;
        public uint Flags;

        public XmlProjectionTexGenParam() { }
        public XmlProjectionTexGenParam(ProjectionTexGenParam srt)
        {
            this.Position = new XmlVector2(srt.Position);
            this.Scale = new XmlVector2(srt.Scale);
            this.Flags = srt.Flags;
        }

        public ProjectionTexGenParam Create() => new ProjectionTexGenParam()
        {
            Position = new Vector2(Position.X, Position.Y),
            Scale = new Vector2(Scale.X, Scale.Y),
            Flags = Flags,
        };
    }

    public class XmlIndirectParameter
    {
        public XmlValue Rotation;
        public XmlVector2 Scale;

        public XmlIndirectParameter() { }
        public XmlIndirectParameter(IndirectParameter srt)
        {
            this.Scale = new XmlVector2(srt.Scale);
            this.Rotation = new XmlValue(srt.Rotation);
        }

        public IndirectParameter Create() => new IndirectParameter()
        {
            Scale = new Vector2(Scale.X, Scale.Y),
            Rotation = Rotation.Value,
        };
    }

    public class XmlMaterialTexCoordGen
    {
        [XmlAttribute]
        public TexGenMatrixType MatrixType;
        [XmlAttribute]
        public TexGenType Source;

        [XmlAttribute]
        public ushort Unknown;
        [XmlAttribute]
        public uint Unknown2;
        //Version >= 8
        [XmlAttribute]
        public ulong Unknown3;

        public XmlMaterialTexCoordGen() { }
        public XmlMaterialTexCoordGen(MaterialTexCoordGen texCoordGen)
        {
            this.MatrixType = texCoordGen.MatrixType;
            this.Source = texCoordGen.Source;
            this.Unknown = texCoordGen.Unknown;
            this.Unknown2 = texCoordGen.Unknown2;
            this.Unknown3 = texCoordGen.Unknown3;
        }

        public MaterialTexCoordGen Create() => new MaterialTexCoordGen()
        {
            MatrixType = this.MatrixType,
            Source = this.Source,
            Unknown = this.Unknown,
            Unknown2 = this.Unknown2,
            Unknown3 = this.Unknown3,
        };
    }

    public class XmlAlphaCompare
    {
        [XmlAttribute]
        public GfxAlphaFunction CompareMode { get; set; }
        [XmlAttribute]
        public float Value { get; set; }

        public XmlAlphaCompare() { }
        public XmlAlphaCompare(AlphaCompare cmp)
        {
            this.CompareMode = cmp.CompareMode;
            this.Value = cmp.Value;
        }

        public AlphaCompare Create() => new AlphaCompare()
        {
            CompareMode = this.CompareMode,
            Value = this.Value,
        };
    }

    public class XmlBlendMode
    {
        [XmlAttribute]
        public GfxBlendOp BlendOp { get; set; }
        [XmlAttribute]
        public GfxBlendFactor SourceFactor { get; set; }
        [XmlAttribute]
        public GfxBlendFactor DestFactor { get; set; }
        [XmlAttribute]
        public GfxLogicOp LogicOp { get; set; }

        public XmlBlendMode() { }
        public XmlBlendMode(BlendMode cmp)
        {
            this.BlendOp = cmp.BlendOp;
            this.SourceFactor = cmp.SourceFactor;
            this.DestFactor = cmp.DestFactor;
            this.LogicOp = cmp.LogicOp;
        }

        public BlendMode Create() => new BlendMode()
        {
            BlendOp = this.BlendOp,
            SourceFactor = this.SourceFactor,
            DestFactor = this.DestFactor,
            LogicOp = this.LogicOp,
        };
    }

    public class XmlMaterialDetailedCombiner
    {
        public uint Value1;

        public XmlColor Color1;
        public XmlColor Color2;
        public XmlColor Color3;
        public XmlColor Color4;
        public XmlColor Color5;
        public XmlColor Color6;

        public List<XmlMaterialDetailedCombinerEntry> Entries = new List<XmlMaterialDetailedCombinerEntry>();
    }

    public class XmlMaterialDetailedCombinerEntry
    {
        public uint Unknown1;
        public int ColorFlags;
        public int AlphaFlags;
        public uint Unknown2;
    }

}
