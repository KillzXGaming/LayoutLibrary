using LayoutLibrary.Cafe;
using LayoutLibrary.Sections.Rev;
using LayoutLibrary.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace LayoutLibrary.XmlConverter
{
    public class XmlWindowPane : XmlPaneContent
    {
        public bool UseOneMaterialForAll;
        public bool UseVertexColorForAll;
        public WindowKind WindowKind;
        public bool NotDrawnContent;
        public ushort StretchLeft;
        public ushort StretchRight;
        public ushort StretchTop;
        public ushort StretchBottm;
        public ushort FrameElementLeft;
        public ushort FrameElementRight;
        public ushort FrameElementTop;
        public ushort FrameElementBottm;
        public byte Flag;

        public XmlWindowContent Content;

        [XmlArrayItem("WindowFrame")]
        public XmlWindowFrame[] Frames = new XmlWindowFrame[0];

        public XmlWindowPane() { }

        public XmlWindowPane(WindowPane pane, BflytFile bflyt)
        {
            this.Frames = new XmlWindowFrame[pane.WindowFrames.Count];
            for (int i = 0; i < pane.WindowFrames.Count; i++)
                this.Frames[i] = new XmlWindowFrame()
                {
                    TextureFlip = pane.WindowFrames[i].TextureFlip,
                    Material = XmlMaterialBase.Create(bflyt, pane.WindowFrames[i].MaterialIndex),
                };

            this.UseOneMaterialForAll = pane.UseOneMaterialForAll;
            this.UseVertexColorForAll = pane.UseVertexColorForAll;
            this.WindowKind = pane.WindowKind;
            this.NotDrawnContent = pane.NotDrawnContent;

            this.StretchLeft = pane.StretchLeft;
            this.StretchRight = pane.StretchRight;
            this.StretchTop = pane.StretchTop;
            this.StretchBottm = pane.StretchBottm;

            this.FrameElementLeft = pane.FrameElementLeft;
            this.FrameElementRight = pane.FrameElementRight;
            this.FrameElementTop = pane.FrameElementTop;
            this.FrameElementBottm = pane.FrameElementBottm;
            this.Flag = pane.Flag;

            this.Content = new XmlWindowContent()
            {
                ColorTopLeft = new XmlColor(pane.Content.ColorTopLeft),
                ColorTopRight = new XmlColor(pane.Content.ColorTopRight),
                ColorBottomLeft = new XmlColor(pane.Content.ColorBottomLeft),
                ColorBottomRight = new XmlColor(pane.Content.ColorBottomRight),
                Material = XmlMaterialBase.Create(bflyt, pane.Content.MaterialIndex),
            };

            this.Content.TexCoords = new XmlTexCoord[pane.Content.TexCoords.Count];
            for (int i = 0; i < pane.Content.TexCoords.Count; i++)
                this.Content.TexCoords[i] = new XmlTexCoord()
                {
                    TopLeft = new XmlVector2(pane.Content.TexCoords[i].TopLeft),
                    TopRight = new XmlVector2(pane.Content.TexCoords[i].TopRight),
                    BottomLeft = new XmlVector2(pane.Content.TexCoords[i].BottomLeft),
                    BottomRight = new XmlVector2(pane.Content.TexCoords[i].BottomRight),
                };
        }

        public WindowPane Create(BflytFile bflyt)
        {
            var contentMaterial = XmlMaterialBase.ConvertBack(bflyt, this.Content.Material);

            var pane = new WindowPane
            {
                UseOneMaterialForAll = this.UseOneMaterialForAll,
                UseVertexColorForAll = this.UseVertexColorForAll,
                WindowKind = this.WindowKind,
                NotDrawnContent = this.NotDrawnContent,
                StretchLeft = this.StretchLeft,
                StretchRight = this.StretchRight,
                StretchTop = this.StretchTop,
                StretchBottm = this.StretchBottm,
                FrameElementLeft = this.FrameElementLeft,
                FrameElementRight = this.FrameElementRight,
                FrameElementTop = this.FrameElementTop,
                FrameElementBottm = this.FrameElementBottm,
                Flag = this.Flag,
                Content = new WindowContent
                {
                    ColorTopLeft = this.Content.ColorTopLeft.ToColor(),
                    ColorTopRight = this.Content.ColorTopRight.ToColor(),
                    ColorBottomLeft = this.Content.ColorBottomLeft.ToColor(),
                    ColorBottomRight = this.Content.ColorBottomRight.ToColor(),
                    MaterialIndex = bflyt.MaterialTable.GetMaterialIndex(contentMaterial),
                    TexCoords = this.Content.TexCoords.Select(tc => new TexCoord
                    {
                        TopLeft = new Vector2(tc.TopLeft.X, tc.TopLeft.Y),
                        TopRight = new Vector2(tc.TopRight.X, tc.TopRight.Y),
                        BottomLeft = new Vector2(tc.BottomLeft.X, tc.BottomLeft.Y),
                        BottomRight = new Vector2(tc.BottomRight.X, tc.BottomRight.Y)
                    }).ToList()
                }
            };


            pane.WindowFrames = this.Frames.Select(f => new WindowFrame
            {
                TextureFlip = f.TextureFlip,
                MaterialIndex = bflyt.MaterialTable.GetMaterialIndex(
                    XmlMaterialBase.ConvertBack(bflyt, f.Material)),
            }).ToList();

            return pane;
        }
    }

    public class XmlWindowContent
    {
        [XmlArrayItem("TexCoord")]
        public XmlTexCoord[] TexCoords = new XmlTexCoord[0];

        public XmlColor ColorTopLeft;
        public XmlColor ColorTopRight;
        public XmlColor ColorBottomLeft;
        public XmlColor ColorBottomRight;

        [XmlElement("Material", typeof(XmlMaterialCafe))]
        [XmlElement("MaterialCtr", typeof(XmlMaterialCtr))]
        [XmlElement("MaterialRev", typeof(XmlMaterialRev))]
        public XmlMaterialBase Material;
    }

    public class XmlWindowFrame
    {
        public WindowFrameTexFlip TextureFlip;

        [XmlElement("Material", typeof(XmlMaterialCafe))]
        [XmlElement("MaterialCtr", typeof(XmlMaterialCtr))]
        [XmlElement("MaterialRev", typeof(XmlMaterialRev))]
        public XmlMaterialBase Material;
    }
}
