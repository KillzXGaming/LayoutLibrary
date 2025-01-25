using LayoutLibrary.Cafe;
using LayoutLibrary.Sections.Rev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlTextPane : XmlPaneContent
    {
        public string Text;

        public ushort TextLength;
        public ushort MaxTextLength;

        public string Font;

        public byte TextAlignment;
        public LineAlign LineAlignment;
        public float ItalicTilt;
        public XmlColor FontTopColor;
        public XmlColor FontBottomColor;
        public XmlVector2 FontSize;
        public float CharacterSpace;
        public float LineSpace;
        public XmlVector2 ShadowXY;
        public XmlVector2 ShadowXYSize;
        public XmlColor ShadowForeColor;
        public XmlColor ShadowBackColor;
        public float ShadowItalic;
        public string TextBoxName;

        public byte _flags;
        public byte Unknown3;

        public float Unknown1;
        public float Unknown2;


        public bool ShadowEnabled;
        public bool RestrictedTextLengthEnabled;
        public bool PerCharTransformEnabled;

        public OriginX HorizontalAlignment;
        public OriginY VerticalAlignment;

        public XmlPerCharacterTransform PerCharacterTransform;

        [XmlElement("Material", typeof(XmlMaterialCafe))]
        [XmlElement("MaterialCtr", typeof(XmlMaterialCtr))]
        [XmlElement("MaterialRev", typeof(XmlMaterialRev))]
        public XmlMaterialBase Material;

        public XmlTextPane() { }
        public XmlTextPane(TextPane pane, BflytFile bflyt)
        {
            // XML adjustments
            if (pane.Text == null)
                pane.Text = "";

            this.Text = pane.Text.Replace("\r\n", "{CRLF}")
                                 .Replace("\r", "{CR}")
                                 .Replace("\n", "{LF}");

            this.TextLength = pane.TextLength;
            this.MaxTextLength = pane.MaxTextLength;
            this.Material = XmlMaterialBase.Create(bflyt, pane.MaterialIndex);
            this.Font = bflyt.FontList.Count > pane.FontIndex ? bflyt.FontList[pane.FontIndex] : "";
            this.TextAlignment = pane.TextAlignment;
            this.LineAlignment = pane.LineAlignment;
            this.ItalicTilt = pane.ItalicTilt;
            this.FontTopColor = new XmlColor(pane.FontTopColor);
            this.FontBottomColor = new XmlColor(pane.FontBottomColor);
            this.FontSize = new XmlVector2(pane.FontSize);
            this.CharacterSpace = pane.CharacterSpace;
            this.LineSpace = pane.LineSpace;
            this.ShadowXY = new XmlVector2(pane.ShadowXY);
            this.ShadowXYSize = new XmlVector2(pane.ShadowXYSize);

            if (pane.ShadowForeColor  != null) 
                this.ShadowForeColor = new XmlColor(pane.ShadowForeColor);
            if (pane.ShadowBackColor != null)
                this.ShadowBackColor = new XmlColor(pane.ShadowBackColor);

            this.ShadowItalic = pane.ShadowItalic;
            this.TextBoxName = pane.TextBoxName;

            this._flags = pane._flags;
            this.Unknown3 = pane.Unknown3;
            this.Unknown1 = pane.Unknown1;
            this.Unknown2 = pane.Unknown2;

            this.ShadowEnabled = pane.ShadowEnabled;
            this.RestrictedTextLengthEnabled = pane.RestrictedTextLengthEnabled;
            this.PerCharTransformEnabled = pane.PerCharTransformEnabled;

            this.HorizontalAlignment = pane.HorizontalAlignment;
            this.VerticalAlignment = pane.VerticalAlignment;

            if (pane.PerCharacterTransform != null )
                this.PerCharacterTransform = new XmlPerCharacterTransform(pane.PerCharacterTransform);
        }

        public TextPane Create(BflytFile bflyt)
        {
            var material = XmlMaterialBase.ConvertBack(bflyt, this.Material);

            if (!string.IsNullOrEmpty(this.Font) && !bflyt.FontList.Contains(this.Font))
                bflyt.FontList.Add(this.Font);

            string text_content = this.Text.Replace("{CRLF}", "\r\n")
                                            .Replace("{CR}", "\r")
                                            .Replace("{LF}", "\n");

            return new TextPane
            {
                Text = text_content,
                TextLength = this.TextLength,
                MaxTextLength = this.MaxTextLength,
                MaterialIndex =  bflyt.MaterialTable.GetMaterialIndex(material),
                FontIndex = (ushort)bflyt.FontList.IndexOf(this.Font),
                TextAlignment = this.TextAlignment,
                LineAlignment = this.LineAlignment,
                ItalicTilt = this.ItalicTilt,
                FontTopColor = this.FontTopColor.ToColor(),
                FontBottomColor = this.FontBottomColor.ToColor(),
                FontSize = this.FontSize.ToVector2(),
                CharacterSpace = this.CharacterSpace,
                LineSpace = this.LineSpace,
                ShadowXY = this.ShadowXY.ToVector2(),
                ShadowXYSize = this.ShadowXYSize.ToVector2(),
                ShadowForeColor = this.ShadowForeColor == null ? null : this.ShadowForeColor.ToColor(),
                ShadowBackColor = this.ShadowBackColor == null ? null : this.ShadowBackColor.ToColor(),
                ShadowItalic = this.ShadowItalic,
                TextBoxName = this.TextBoxName,
                _flags = this._flags,
                Unknown3 = this.Unknown3,
                Unknown1 = this.Unknown1,
                Unknown2 = this.Unknown2,
                ShadowEnabled = this.ShadowEnabled,
                RestrictedTextLengthEnabled = this.RestrictedTextLengthEnabled,
                PerCharTransformEnabled = this.PerCharTransformEnabled,
                HorizontalAlignment = this.HorizontalAlignment,
                VerticalAlignment = this.VerticalAlignment,
                PerCharacterTransform = this.PerCharacterTransform == null ? null : this.PerCharacterTransform.Create(),
            };
        }
    }

    public class XmlPerCharacterTransform
    {
        public float CurveTimeOffset;
        public float CurveWidth;
        public byte LoopType;
        public byte VerticalOrigin;
        public byte HasAnimInfo;
        public byte padding;

        public XMLAnimationConverter.XmlAnimationSubGroup AnimationInfo;

        public byte[] CharList = new byte[20];

        public XmlPerCharacterTransform() { }
        public XmlPerCharacterTransform(PerCharacterTransform perCharacter)
        {
            this.CurveTimeOffset = perCharacter.CurveTimeOffset;
            this.CurveWidth = perCharacter.CurveWidth;
            this.LoopType = perCharacter.LoopType;
            this.VerticalOrigin = perCharacter.VerticalOrigin;
            this.HasAnimInfo = perCharacter.HasAnimInfo;
            this.padding = perCharacter.padding;
            this.CharList = perCharacter.CharList;
            if (perCharacter.AnimationInfo != null)
                this.AnimationInfo = XMLAnimationConverter.ConvertSubGroup(perCharacter.AnimationInfo);
        }

        public PerCharacterTransform Create()
        {
            return new PerCharacterTransform()
            {
                CurveWidth = this.CurveWidth, 
                HasAnimInfo = this.HasAnimInfo,
                CharList = this.CharList,
                CurveTimeOffset = this.CurveTimeOffset,
                LoopType = this.LoopType,
                padding = this.padding,
                VerticalOrigin = this.VerticalOrigin,
                AnimationInfo = AnimationInfo == null ? null : XMLAnimationConverter.ConvertXmlSubGroup(this.AnimationInfo)
            };
        }
    }
}
