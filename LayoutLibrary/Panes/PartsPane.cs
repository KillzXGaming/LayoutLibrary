using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LayoutLibrary
{
    public class PartsPane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "prt1";

        public float MagnifyX;
        public float MagnifyY;

        public List<PartsProperty> Properties = new List<PartsProperty>();

        public string LayoutFileName;

        public PartsPane() { }
    }

    public class PartsProperty
    {
        public string Name;

        public byte UsageFlag;
        public byte BasicUsageFlag;
        public byte MaterialUsageFlag;
        public byte Reserved0;

        public uint Flag1;

        public PartsPaneBasicInfo BasicInfo;

        public UserData UserData;

        public Pane Property;
    }

    public class PartsPaneBasicInfo
    {   
        public string UserName;
        public Vector3 Translate;
        public Vector3 Rotate;
        public Vector2 Scale;
        public byte Alpha;
        public byte Reserved0;
        public byte Reserved1;
        public byte Reserved2;

        public uint Reserved3;
        public uint Reserved4;
    }
}
