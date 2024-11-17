using LayoutLibrary.Cafe;
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
    public class XmlPane
    {
        [XmlAttribute]
        public string Type;
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string UserDataInfo;

        public XmlVector3 Translate;
        public XmlVector3 Rotate;
        public XmlVector2 Scale;
        public Size Size;
        public OriginPlacement Origin;
        public OriginPlacement ParentOrigin;

        [XmlAttribute()]
        public bool Visible;

        [XmlAttribute()]
        public bool InfluenceAlpha;
        
        [XmlAttribute()]
        public byte Alpha;

        [XmlAttribute()]
        public byte PaneMagFlags;

        [XmlAttribute()]
        public byte Flags1;

        [XmlElement("Picture", typeof(XmlPicturePane))]
        [XmlElement("Window", typeof(XmlWindowPane))]
        [XmlElement("Text", typeof(XmlTextPane))]
        [XmlElement("Part", typeof(XmlPartPane))]
        [XmlElement("Null", typeof(XmlNullPane))]
        [XmlElement("Alignment", typeof(XmlAlignmentPane))]
        [XmlElement("Scissor", typeof(XmlScissorPane))]
        [XmlElement("Bounds", typeof(XmlBoundsPane))]
        public XmlPaneContent Content;

        public XmlUserData UserData;

        [XmlArray("Children")]
        [XmlArrayItem("Pane")]
        public XmlPane[] Children;

        public XmlPane() { }

        public XmlPane(Pane pane, BflytFile bflyt)
        {
            this.Size = new Size() { Width = pane.Width, Height = pane.Height };
            this.Scale = new XmlVector2(pane.Scale.X, pane.Scale.Y);
            this.Translate = new XmlVector3(pane.Translate.X, pane.Translate.Y, pane.Translate.Z);
            this.Rotate = new XmlVector3(pane.Rotate.X, pane.Rotate.Y, pane.Rotate.Z);
            this.Alpha = pane.Alpha;
            this.ParentOrigin = new OriginPlacement(pane.ParentOriginX, pane.ParentOriginY);
            this.Origin = new OriginPlacement(pane.OriginX, pane.OriginY);
            this.Name = pane.Name;
            this.PaneMagFlags = pane.PaneMagFlags;
            this.UserDataInfo = pane.UserDataInfo;
            this.Flags1 = pane.Flags1;
            this.Visible = pane.Visible;
            this.InfluenceAlpha = pane.InfluenceAlpha;

            this.Children = new XmlPane[pane.Children.Count];
            for (int i = 0; i < pane.Children.Count; i++)
                this.Children[i] = new XmlPane(pane.Children[i], bflyt);

            if (pane.UserData != null)
                this.UserData = new XmlUserData(pane.UserData);

            if (pane is PicturePane) this.Content = new XmlPicturePane((PicturePane)pane, bflyt);
            else if (pane is WindowPane) this.Content = new XmlWindowPane((WindowPane)pane, bflyt);
            else if (pane is TextPane) this.Content = new XmlTextPane((TextPane)pane, bflyt);
            else if (pane is PartsPane) this.Content = new XmlPartPane((PartsPane)pane, bflyt);
            else if (pane is AlignmentPane) this.Content = new XmlAlignmentPane((AlignmentPane)pane);
            else if (pane is ScissorPane) this.Content = new XmlScissorPane();
            else if (pane is BoundsPane) this.Content = new XmlBoundsPane();

            this.Type = "Null";
            if (pane is PicturePane) this.Type = "Picture";
            else if (pane is WindowPane) this.Type = "Window";
            else if (pane is TextPane) this.Type = "Text";
            else if (pane is PartsPane) this.Type = "Part";
            else if (pane is AlignmentPane) this.Type = "Alignment";
            else if (pane is ScissorPane) this.Type = "Scissor";
            else if (pane is BoundsPane) this.Type = "Bounds";
        }

        public Pane Create(BflytFile bflyt)
        {
            Pane pane = new Pane();

            switch (this.Type)
            {
                case "Picture":
                    pane = ((XmlPicturePane)this.Content).Create(bflyt);
                    break;
                case "Window":
                    pane = ((XmlWindowPane)this.Content).Create(bflyt);
                    break;
                case "Text":
                    pane = ((XmlTextPane)this.Content).Create(bflyt);
                    break;
                case "Part":
                    pane = ((XmlPartPane)this.Content).Create(bflyt);
                    break;
                case "Alignment":
                    pane = ((XmlAlignmentPane)this.Content).Create();
                    break;
                case "Scissor":
                    pane = new ScissorPane();
                    break;
                case "Bounds":
                    pane = new BoundsPane();
                    break;
                default:
                    pane = new Pane();
                    break;
            }

            pane.Width = this.Size.Width;
            pane.Height = this.Size.Height;
            pane.Scale = new Vector2(this.Scale.X, this.Scale.Y);
            pane.Translate = new Vector3(this.Translate.X, this.Translate.Y, this.Translate.Z);
            pane.Rotate = new Vector3(this.Rotate.X, this.Rotate.Y, this.Rotate.Z);
            pane.Alpha = this.Alpha;
            pane.Flags1 = this.Flags1;
            pane.ParentOriginX = this.ParentOrigin.X;
            pane.ParentOriginY = this.ParentOrigin.Y;
            pane.OriginX = this.Origin.X;
            pane.OriginY = this.Origin.Y;
            pane.Name = this.Name;
            pane.PaneMagFlags = this.PaneMagFlags;
            pane.UserDataInfo = this.UserDataInfo;
            pane.Visible = this.Visible;
            pane.InfluenceAlpha = this.InfluenceAlpha;

            if (this.UserData != null)
                pane.UserData = this.UserData.Create();

            for (int i = 0; i < this.Children.Length; i++)
                pane.Children.Add(this.Children[i].Create(bflyt));

            return pane;
        }
    }

    public class XmlPaneContent
    {

    }
    public class XmlNullPane : XmlPaneContent
    {

    }
    public class XmlScissorPane : XmlPaneContent
    {

    }
    public class XmlBoundsPane : XmlPaneContent
    {

    }
    public class XmlAlignmentPane : XmlPaneContent
    {
        public XmlVector3 Alignment;

        public XmlAlignmentPane() { }
        public XmlAlignmentPane(AlignmentPane v) { this.Alignment = new XmlVector3(v.AlignmentValue); }

        public AlignmentPane Create()
        {
            return new AlignmentPane()
            {
                AlignmentValue = Alignment.ToVector3(),
            };
        }
    }
}
