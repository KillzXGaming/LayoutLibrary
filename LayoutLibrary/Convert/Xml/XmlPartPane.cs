using LayoutLibrary.Cafe;
using LayoutLibrary.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary.XmlConverter
{
    public class XmlPartPane : XmlPaneContent
    {
        public float MagnifyX;
        public float MagnifyY;

        public List<XmlPartProperty> Properties = new List<XmlPartProperty>();

        public string LayoutFileName;

        public XmlPartPane() { }

        public XmlPartPane(PartsPane pane, BflytFile bflyt)
        {
            this.MagnifyX = pane.MagnifyX;
            this.MagnifyY = pane.MagnifyY;
            this.LayoutFileName = pane.LayoutFileName;

            foreach (var prop in pane.Properties)
            {
                XmlPartsPaneBasicInfo info = null;

                if (prop.BasicInfo != null)
                    info = new XmlPartsPaneBasicInfo()
                    {
                        Alpha = prop.BasicInfo.Alpha,
                        Reserved0 = prop.BasicInfo.Reserved0,
                        Reserved1 = prop.BasicInfo.Reserved1,
                        Reserved2 = prop.BasicInfo.Reserved2,
                        Reserved3 = prop.BasicInfo.Reserved3,
                        Reserved4 = prop.BasicInfo.Reserved4,
                        Rotate = new XmlVector3(prop.BasicInfo.Rotate),
                        Translate = new XmlVector3(prop.BasicInfo.Translate),
                        Scale = new XmlVector2(prop.BasicInfo.Scale),
                        UserName = prop.BasicInfo.UserName,
                    };

                this.Properties.Add(new XmlPartProperty()
                {
                    BasicInfo = info,
                    BasicUsageFlag = prop.BasicUsageFlag,
                    UsageFlag = prop.UsageFlag,
                    Flag1 = prop.Flag1,
                    MaterialUsageFlag = prop.MaterialUsageFlag,
                    Name = prop.Name,
                    Reserved0 = prop.Reserved0,
                    Property = prop.Property == null ? null : new XmlPane(prop.Property, bflyt),
                    UserData = prop.UserData == null ? null : new XmlUserData(prop.UserData),
                });
            }
        }

        public PartsPane Create(BflytFile bflyt)
        {
            PartsPane pane = new PartsPane();
            pane.MagnifyX = this.MagnifyX;
            pane.MagnifyY = this.MagnifyY;
            pane.LayoutFileName = this.LayoutFileName;

            foreach (var xmlProp in this.Properties)
            {
                var basicInfo = xmlProp.BasicInfo != null
                    ? new PartsPaneBasicInfo
                    {
                        Alpha = xmlProp.BasicInfo.Alpha,
                        Reserved0 = xmlProp.BasicInfo.Reserved0,
                        Reserved1 = xmlProp.BasicInfo.Reserved1,
                        Reserved2 = xmlProp.BasicInfo.Reserved2,
                        Reserved3 = xmlProp.BasicInfo.Reserved3,
                        Reserved4 = xmlProp.BasicInfo.Reserved4,
                        Rotate = xmlProp.BasicInfo.Rotate.ToVector3(),
                        Translate = xmlProp.BasicInfo.Translate.ToVector3(),
                        Scale = xmlProp.BasicInfo.Scale.ToVector2(),
                        UserName = xmlProp.BasicInfo.UserName,
                    }
                    : null;

                pane.Properties.Add(new PartsProperty
                {
                    BasicInfo = basicInfo,
                    BasicUsageFlag = xmlProp.BasicUsageFlag,
                    UsageFlag = xmlProp.UsageFlag,
                    Flag1 = xmlProp.Flag1,
                    MaterialUsageFlag = xmlProp.MaterialUsageFlag,
                    Name = xmlProp.Name,
                    Reserved0 = xmlProp.Reserved0,
                    Property = xmlProp.Property == null ? null : xmlProp.Property.Create(bflyt),
                    UserData = xmlProp.UserData == null ? null : xmlProp.UserData.Create()
                });
            }

            return pane;
        }
    }

    public class XmlPartProperty
    {
        public string Name;

        public byte UsageFlag;
        public byte BasicUsageFlag;
        public byte MaterialUsageFlag;
        public byte Reserved0;

        public uint Flag1;

        public XmlPartsPaneBasicInfo BasicInfo;

        public XmlUserData UserData;

        public XmlPane Property;
    }

    public class XmlPartsPaneBasicInfo
    {
        public string UserName;
        public XmlVector3 Translate;
        public XmlVector3 Rotate;
        public XmlVector2 Scale;
        public byte Alpha;
        public byte Reserved0;
        public byte Reserved1;
        public byte Reserved2;

        public uint Reserved3;
        public uint Reserved4;
    }
}
