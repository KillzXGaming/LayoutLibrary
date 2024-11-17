using LayoutLibrary.Cafe;
using LayoutLibrary.Sections.Rev;
using LayoutLibrary.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlPicturePane : XmlPaneContent
    {
        [XmlArrayItem("TexCoord")]
        public XmlTexCoord[] TexCoords = new XmlTexCoord[0];

        public XmlColor ColorTopLeft;
        public XmlColor ColorTopRight;
        public XmlColor ColorBottomLeft;
        public XmlColor ColorBottomRight;

        public bool IsShape;

        [XmlElement("Material", typeof(XmlMaterialCafe))]
        [XmlElement("MaterialCtr", typeof(XmlMaterialCtr))]
        [XmlElement("MaterialRev", typeof(XmlMaterialRev))]
        public XmlMaterialBase Material;

        public XmlPicturePane() { }

        public XmlPicturePane(PicturePane pane, BflytFile bflyt)
        {
            this.TexCoords = new XmlTexCoord[pane.TexCoords.Length];
            for (int i = 0; i < pane.TexCoords.Length; i++)
                this.TexCoords[i] = new XmlTexCoord()
                {
                    TopLeft = new XmlVector2(pane.TexCoords[i].TopLeft),
                    TopRight = new XmlVector2(pane.TexCoords[i].TopRight),
                    BottomLeft = new XmlVector2(pane.TexCoords[i].BottomLeft),
                    BottomRight = new XmlVector2(pane.TexCoords[i].BottomRight),
                };

            this.ColorTopLeft = new XmlColor(pane.ColorTopLeft);
            this.ColorTopRight = new XmlColor(pane.ColorTopRight);
            this.ColorBottomLeft = new XmlColor(pane.ColorBottomLeft);
            this.ColorBottomRight = new XmlColor(pane.ColorBottomRight);
            this.IsShape = pane.IsShape;
            this.Material = XmlMaterialBase.Create(bflyt, pane.MaterialIndex);
        }

        public PicturePane Create(BflytFile bflyt)
        {
            var material = XmlMaterialBase.ConvertBack(bflyt, this.Material);

            var pane = new PicturePane
            {
                TexCoords = this.TexCoords.Select(tc => new TexCoord
                {
                    TopLeft = new Vector2(tc.TopLeft.X, tc.TopLeft.Y),
                    TopRight = new Vector2(tc.TopRight.X, tc.TopRight.Y),
                    BottomLeft = new Vector2(tc.BottomLeft.X, tc.BottomLeft.Y),
                    BottomRight = new Vector2(tc.BottomRight.X, tc.BottomRight.Y)
                }).ToArray(),
                ColorTopLeft = this.ColorTopLeft.ToColor(),
                ColorTopRight = this.ColorTopRight.ToColor(),
                ColorBottomLeft = this.ColorBottomLeft.ToColor(),
                ColorBottomRight = this.ColorBottomRight.ToColor(),
                IsShape = this.IsShape,
                MaterialIndex = bflyt.MaterialTable.GetMaterialIndex(material)
            };
            return pane;
        }
    }
}
