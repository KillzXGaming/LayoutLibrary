using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using LayoutLibrary.XmlConverter;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    /// <summary>
    /// Converts BFLYT/BRLYT/BCLYT to and from the XML file format.
    /// </summary>
    public partial class XMLayoutConverter
    {
        public static string ToXml(BflytFile bflyt)
        {
            XmlHeader header = new XmlHeader();
            header.Header = new XmlLayoutHeaderInfo()
            {
                Magic = bflyt.Magic,
                ByteOrderMark = ((ByteOrder)bflyt.ByteOrderMark).ToString(),
                VersionMajor = bflyt.VersionMajor,
                VersionMinor = bflyt.VersionMinor,
                VersionMicro = bflyt.VersionMicro,
            };

            if (bflyt.Layout != null)
                header.Layout = new XmlLayout()
                {
                    DrawFromCenter = bflyt.Layout.DrawFromCenter,
                    Size = new Size(bflyt.Layout.Width, bflyt.Layout.Height),
                    //Version specific features
                    MaxPartsSize = new Size(bflyt.Layout.MaxPartsWidth, bflyt.Layout.MaxPartsHeight),
                    Name            =  bflyt.Layout.Name,
                };

            header.TextureList = bflyt.TextureList;
            header.FontList = bflyt.FontList;
            header.Root = new XmlPane(bflyt.Root, bflyt);
            header.RootGroup = new XmlGroup(bflyt.RootGroup);
            header.CaptureTextureLayer = bflyt.CaptureTextureLayer;
            if (bflyt.UnsupportedSections?.Count > 0)
                header.UnsupportedSections = bflyt.UnsupportedSections;

            if (bflyt.ControlSource != null)
                header.ControlSource = new XmlControlSource(bflyt.ControlSource);

            foreach (var usd in bflyt.UserData)
                header.UserData.Add(new XmlUserData(usd));

            using (var writer = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(typeof(XmlHeader));
                serializer.Serialize(writer, header);
                writer.Flush();
                return writer.ToString();
            }
        }

        public static BflytFile FromXml(string xmlString)
        {
            XmlHeader header = Deserialize<XmlHeader>(xmlString);

            BflytFile bflyt = new BflytFile();
            bflyt.Magic = header.Header.Magic;
            bflyt.ByteOrderMark = (ushort)Enum.Parse(typeof(ByteOrder), header.Header.ByteOrderMark);
            bflyt.VersionMajor = header.Header.VersionMajor;
            bflyt.VersionMinor = header.Header.VersionMinor;
            bflyt.VersionMicro = header.Header.VersionMicro;
            bflyt.TextureList = header.TextureList;
            bflyt.FontList = header.FontList;
            bflyt.CaptureTextureLayer = header.CaptureTextureLayer;
            if (header.UnsupportedSections != null)
                bflyt.UnsupportedSections = header.UnsupportedSections;

            if (header.ControlSource != null)
                bflyt.ControlSource = header.ControlSource.Create();

            foreach (var usd in header.UserData)
                bflyt.UserData.Add(usd.Create());

            foreach (var xmlMat in GetMaterialList(header.Root).OrderBy(x => x.Index))
                bflyt.MaterialTable.Materials.Add(XmlMaterialBase.ConvertBack(bflyt, xmlMat));

            if (header.Layout != null)
                bflyt.Layout = new Layout()
                {
                    DrawFromCenter = header.Layout.DrawFromCenter,
                    Width = header.Layout.Size.Width,
                    Height = header.Layout.Size.Height,
                    //Version specific features
                    MaxPartsWidth = header.Layout.MaxPartsSize.Width,
                    MaxPartsHeight = header.Layout.MaxPartsSize.Height,
                    Name = header.Layout.Name,
                };

            bflyt.Root = header.Root.Create(bflyt);
            bflyt.RootGroup = header.RootGroup.Create();

            return bflyt;
        }

        static List<XmlMaterialBase> GetMaterialList(XmlPane pane)
        {
            List<XmlMaterialBase> materials = new List<XmlMaterialBase>();

            if (pane.Content is XmlPartPane partPane)
            {
                foreach (var p in partPane.Properties)
                {
                    if (p.Property != null)
                        materials.AddRange(GetMaterialList(p.Property));
                }
            }
            if (pane.Content is XmlPicturePane picPane)
                materials.Add(picPane.Material);
            if (pane.Content is XmlTextPane textPane)
                materials.Add(textPane.Material);
            if (pane.Content is XmlWindowPane wndPane)
            {
                if (wndPane.Content != null)
                    materials.Add(wndPane.Content.Material);
                foreach (var f in wndPane.Frames)
                    materials.Add(f.Material);
            }

            foreach (var child in pane.Children)
                materials.AddRange(GetMaterialList(child));

            return materials;
        }

        private static T Deserialize<T>(string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public class XmlHeader
        {
            [XmlElement]
            public XmlLayoutHeaderInfo Header;

            [XmlArray("UserData")]
            [XmlArrayItem("UserData")]
            public List<XmlUserData> UserData = new List<XmlUserData>();

            public XmlLayout Layout;

            public List<string> TextureList;
            public List<string> FontList;

            [XmlElement]
            public XmlPane Root;

            [XmlElement]
            public XmlGroup RootGroup;

            [XmlElement]
            public XmlControlSource ControlSource;

            [XmlElement]
            public CaptureTextureLayer CaptureTextureLayer;

            public List<UnsupportedSection> UnsupportedSections;
        }

        public class XmlLayout
        {
            [XmlAttribute]
            public string? Name = null;

            [XmlAttribute]
            public bool DrawFromCenter;

            public Size Size;
            public Size MaxPartsSize;
        }
    }
}
