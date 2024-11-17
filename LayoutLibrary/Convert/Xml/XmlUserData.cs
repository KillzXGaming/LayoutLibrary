using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlUserData
    {
        [XmlArray("Entries")]
        [XmlArrayItem("UserDataEntry")]
        public List<XmlUserDataEntry> Entries = new List<XmlUserDataEntry>();

        public byte[]? RawData;

        public XmlUserData() { }
        public XmlUserData(UserData userData)
        {
            foreach (var item in userData.Entries)
            {
                XmlUserDataEntry xmlUsdEntry = new XmlUserDataEntry();
                xmlUsdEntry.Name = item.Name;
                xmlUsdEntry.Data = item._data;
                xmlUsdEntry.Type = item.Type;
                this.Entries.Add(xmlUsdEntry);
            }

            if (userData?.Raw?.Length > 0)
                this.RawData = userData.Raw;
        }

        public UserData Create()
        {
            UserData userData = new UserData();
            foreach (var xmlEntry in this.Entries)
            {
                if (xmlEntry.Data == null) continue;

                userData.Entries.Add(new UserData.UserDataEntry
                {
                    Name = xmlEntry.Name,
                    Type = xmlEntry.Type,
                    _data = xmlEntry.Data,
                });
            }

            if (this.RawData != null && this.RawData.Length > 0)
                userData.Raw = this.RawData;

            return userData;
        }
    }

    public class XmlUserDataEntry
    {
        public string Name;
        public UserDataType Type;

        [XmlElement("FloatArray", typeof(float[]))]
        [XmlElement("IntArray", typeof(int[]))]
        [XmlElement("String", typeof(string))]
        public object Data { get; set; }
    }
}
