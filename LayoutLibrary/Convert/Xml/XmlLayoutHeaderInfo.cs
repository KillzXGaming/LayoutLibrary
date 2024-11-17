using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlLayoutHeaderInfo
    {
        [XmlAttribute]
        public string Magic;
        [XmlAttribute]
        public byte VersionMajor;
        [XmlAttribute]
        public byte VersionMinor;
        [XmlAttribute]
        public ushort VersionMicro;

        [XmlAttribute]
        public string ByteOrderMark;
    }
}
