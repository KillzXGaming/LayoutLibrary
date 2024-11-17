using LayoutBXLYT.Revolution;
using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using LayoutLibrary.Revolution;
using LayoutLibrary.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LayoutLibrary.Sections.Rev
{
    public class XmlMaterialRev : XmlMaterialBase
    {
        public XmlColor BlackColor;
        public XmlColor WhiteColor;
        public XmlColor ColorRegister3;
        public XmlColor MatColor;
        public XmlColor TevColor1;
        public XmlColor TevColor2;
        public XmlColor TevColor3;
        public XmlColor TevColor4;

        public List<XmlMaterialTextureMap> Textures = new List<XmlMaterialTextureMap>();
        public List<XmlMaterialTextureSrt> TextureSrts = new List<XmlMaterialTextureSrt>();
        public List<TexCoordGenEntry> TexCoordGens = new List<TexCoordGenEntry>();
        public List<XmlMaterialTextureSrt> IndirectSrts = new List<XmlMaterialTextureSrt>();

        public List<IndirectStage> IndirectStages = new List<IndirectStage>();
        public List<TevStage> TevStages = new List<TevStage>();

        public ChanCtrl ChanControl;
        public TevSwapModeTable TevSwapModeTable;
        public AlphaCompareRev AlphaCompare;
        public BlendMode BlendMode;

        public bool HasMaterialColor;

        public XmlMaterialRev() { }
        public XmlMaterialRev(MaterialRev material, BflytFile bflyt, ushort index) : base()
        {
            Name = material.Name;
            this.Index = index;

            this.BlackColor = new XmlColor(
                                (byte)material.BlackColor.R,
                                (byte)material.BlackColor.G,
                                (byte)material.BlackColor.B,
                                (byte)material.BlackColor.A);
            this.WhiteColor = new XmlColor(
                                (byte)material.WhiteColor.R,
                                (byte)material.WhiteColor.G,
                                (byte)material.WhiteColor.B,
                                (byte)material.WhiteColor.A);
            this.ColorRegister3 = new XmlColor(
                                (byte)material.ColorRegister3.R,
                                (byte)material.ColorRegister3.G,
                                (byte)material.ColorRegister3.B,
                                (byte)material.ColorRegister3.A);

            this.TevColor1 = new XmlColor(material.TevColor1);
            this.TevColor2 = new XmlColor(material.TevColor2);
            this.TevColor3 = new XmlColor(material.TevColor3);
            this.TevColor4 = new XmlColor(material.TevColor4);
            this.MatColor = new XmlColor(material.MatColor);

            this.HasMaterialColor = material.Flags.HasMaterialColor;

            foreach (var tex in material.TextureMaps)
                this.Textures.Add(new XmlMaterialTextureMap(tex, bflyt));
            foreach (var texGen in material.TexCoordGens)
                this.TexCoordGens.Add(texGen);
            foreach (var tex in material.TextureSrts)
                this.TextureSrts.Add(new XmlMaterialTextureSrt(tex));
            foreach (var srt in material.IndirectTexTransforms)
                this.IndirectSrts.Add(new XmlMaterialTextureSrt(srt));
            foreach (var stage in material.IndirectStages)
                this.IndirectStages.Add(stage);
            foreach (var stage in material.TevStages)
                this.TevStages.Add(stage);

            if (material.Flags.HasChannelControl)
                this.ChanControl = material.ChanControl;
            if (material.Flags.HasTevSwapTable)
                this.TevSwapModeTable = material.TevSwapModeTable;
            if (material.Flags.HasAlphaCompare)
                this.AlphaCompare = material.AlphaCompare;
            if (material.Flags.HasBlendMode)
                this.BlendMode = material.BlendMode;

            //Null unused lists to hide in xml
            if (this.Textures.Count == 0) this.Textures = null;
            if (this.TevStages.Count == 0) this.TevStages = null;
            if (this.IndirectStages.Count == 0) this.IndirectStages = null;
            if (this.TexCoordGens.Count == 0) this.TexCoordGens = null;
            if (this.TextureSrts.Count == 0) this.TextureSrts = null;
            if (this.IndirectSrts.Count == 0) this.IndirectSrts = null;
        }

        public MaterialRev Create(BflytFile bflyt)
        {
            MaterialRev mat = new MaterialRev()
            {
                Name = this.Name,
                Index = this.Index,
                BlackColor = new Color16(this.BlackColor.R, this.BlackColor.G, this.BlackColor.B, this.BlackColor.A),
                WhiteColor = new Color16(this.WhiteColor.R, this.WhiteColor.G, this.WhiteColor.B, this.WhiteColor.A),
                ColorRegister3 = new Color16(this.ColorRegister3.R, this.ColorRegister3.G, this.ColorRegister3.B, this.ColorRegister3.A),
                TevColor1 = this.TevColor1.ToColor(),
                TevColor2 = this.TevColor2.ToColor(),
                TevColor3 = this.TevColor3.ToColor(),
                TevColor4 = this.TevColor4.ToColor(),
                MatColor = this.MatColor.ToColor(),
                AlphaCompare = this.AlphaCompare,
                BlendMode = this.BlendMode,
                ChanControl = this.ChanControl,
                TevSwapModeTable = this.TevSwapModeTable,
                TevStages = this.TevStages,
                TexCoordGens = this.TexCoordGens,
                IndirectStages = this.IndirectStages,
            };

            mat.Flags.HasMaterialColor = this.HasMaterialColor;
            mat.Flags.HasAlphaCompare = this.AlphaCompare != null;
            mat.Flags.HasBlendMode = this.BlendMode != null;
            mat.Flags.HasChannelControl = this.ChanControl != null;
            mat.Flags.HasTevSwapTable = this.TevSwapModeTable != null;

            foreach (var tex in this.Textures)
            {
                if (!bflyt.TextureList.Contains(tex.Texture))
                    bflyt.TextureList.Add(tex.Texture);

                mat.TextureMaps.Add(tex.Create(bflyt));
            }

            foreach (var srt in this.TextureSrts)
                mat.TextureSrts.Add(srt.Create());
            foreach (var srt in this.IndirectSrts)
                mat.IndirectTexTransforms.Add(srt.Create());

            return mat;
        }
    }
}
