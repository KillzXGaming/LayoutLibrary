using LayoutLibrary.Cafe;
using LayoutLibrary.Ctr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlMaterialCtr : XmlMaterialBase
    {
        public XmlColor BlackColor;
        public XmlColor WhiteColor;
        public XmlColor TevColor1;
        public XmlColor TevColor2;
        public XmlColor TevColor3;
        public XmlColor TevColor4;
        public XmlColor TevColor5;

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
        [XmlArrayItem("TevCombinerCtr")]
        public List<TevCombinerCtr> TevCombiners = new List<TevCombinerCtr>();

        public XmlAlphaCompare AlphaCompare;
        public XmlBlendMode ColorBlend;
        public XmlBlendMode AlphaBlend;
        public XmlFontShadowParameter FontShadowParameter;


        public bool UseTextureOnly = false;
        public bool AlphaInterpolation = false;

        public XmlMaterialCtr() { }
        public XmlMaterialCtr(MaterialCtr material, BflytFile bflyt, ushort index)
        {
            this.Name = material.Name;
            this.Index = index;

            this.BlackColor = new XmlColor(material.BlackColor);
            this.WhiteColor = new XmlColor(material.WhiteColor);
            this.TevColor1 = new XmlColor(material.TevColor1);
            this.TevColor2 = new XmlColor(material.TevColor2);
            this.TevColor3 = new XmlColor(material.TevColor3);
            this.TevColor4 = new XmlColor(material.TevColor4);
            this.TevColor5 = new XmlColor(material.TevColor5);

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

            foreach (var cmb in material.TevCombiners)
            {
                this.TevCombiners.Add(cmb);
            }

            //Null unused lists to hide in xml
            if (this.Textures.Count == 0) this.Textures = null;
            if (this.TevCombiners.Count == 0) this.TevCombiners = null;
            if (this.ProjectionTexGens.Count == 0) this.ProjectionTexGens = null;
            if (this.TexCoordGens.Count == 0) this.TexCoordGens = null;
            if (this.TextureSrts.Count == 0) this.TextureSrts = null;
        }

        public MaterialCtr Create(BflytFile bflyt)
        {
            MaterialCtr mat = new MaterialCtr();
            mat.Name = this.Name;
            mat.Index = this.Index;
            mat.BlackColor = this.BlackColor.ToColor();
            mat.WhiteColor = this.WhiteColor.ToColor();
            mat.TevColor1 = this.TevColor1.ToColor();
            mat.TevColor2 = this.TevColor2.ToColor();
            mat.TevColor3 = this.TevColor3.ToColor();
            mat.TevColor4 = this.TevColor4.ToColor();
            mat.TevColor5 = this.TevColor5.ToColor();

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

            foreach (var cmb in this.TevCombiners)
                mat.TevCombiners.Add(cmb);

            return mat;
        }
    }
}
