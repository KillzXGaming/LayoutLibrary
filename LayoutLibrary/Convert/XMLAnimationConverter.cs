using LayoutLibrary.Files;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    /// <summary>
    /// Converts BFLAN/BRLAN/BCLAN to and from the XML file format.
    /// </summary>
    public partial class XMLAnimationConverter
    {
        public static string ToXml(BflanFile bflan)
        {
            XmlHeader header = new XmlHeader();
            header.TagInfo = new XmlTagInfo();
            header.AnimationInfo = new XmlAnimationInfo();

            header.Header = new XmlLayoutHeaderInfo()
            {
                Magic = bflan.Magic,
                ByteOrderMark = ((ByteOrder)bflan.ByteOrderMark).ToString(),
                VersionMajor = bflan.VersionMajor,
                VersionMinor = bflan.VersionMinor,
                VersionMicro = bflan.VersionMicro,
            };

            if (header.TagInfo != null)
                header.TagInfo = new XmlTagInfo()
                {
                    AnimationOrder = bflan.TagInfo.AnimationOrder,
                    ChildBinding = bflan.TagInfo.ChildBinding,
                    StartFrame = bflan.TagInfo.StartFrame,
                    EndFrame = bflan.TagInfo.EndFrame,
                    Name = bflan.TagInfo.Name,
                    Groups = bflan.TagInfo.Groups,
                    UnknownData = bflan.TagInfo.UnknownData,
                    UserData = bflan.TagInfo.UserData,
                };
            if (header.AnimationInfo != null)
            {
                header.AnimationInfo = new XmlAnimationInfo();
                header.AnimationInfo.Textures = bflan.AnimationInfo.Textures;
                header.AnimationInfo.FrameSize = bflan.AnimationInfo.FrameSize;
                header.AnimationInfo.Loop = bflan.AnimationInfo.Loop;

                foreach (var g in bflan.AnimationInfo.Entries)
                {
                    XmlAnimationGroup xmlAnimGroup = new XmlAnimationGroup();
                    xmlAnimGroup.Name = g.Name;
                    xmlAnimGroup.Type = g.Type.ToString();
                    header.AnimationInfo.Groups.Add(xmlAnimGroup);

                    foreach (var t in g.Tags)
                        xmlAnimGroup.SubGroups.Add(ConvertSubGroup(t));
                }
            }

            using (var writer = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(typeof(XmlHeader));
                serializer.Serialize(writer, header);
                writer.Flush();
                return writer.ToString();
            }
        }

        public static XmlAnimationSubGroup ConvertSubGroup(AnimationInfoSubGroup t)
        {
            XmlAnimationSubGroup xmlAnimSubGroup = new XmlAnimationSubGroup();
            xmlAnimSubGroup.Kind = t.Kind.Remove(0, 1); //type without first char

            Dictionary<int, string> target_types = new Dictionary<int, string>();

            if (AnimationInfoSubGroup.TypeEnumDefine.ContainsKey(t.Kind))
            {
                var targetEnum = AnimationInfoSubGroup.TypeEnumDefine[t.Kind];
                foreach (int v in Enum.GetValues(targetEnum).Cast<byte>())
                    target_types.Add(v, Enum.GetName(targetEnum, v));
            }
            if (AnimationInfoSubGroup.TypeDefine.ContainsKey(xmlAnimSubGroup.Kind))
            {
                var targetName = AnimationInfoSubGroup.TypeDefine[xmlAnimSubGroup.Kind];
                xmlAnimSubGroup.Kind = targetName;
            }

            foreach (var track in t.Targets)
            {
                XmlAnimationTrack xmlTrack = new XmlAnimationTrack();
                xmlTrack.CurveType = track.CurveType.ToString();
                xmlTrack.Index = track.Index;
                xmlTrack.Target = track.Target.ToString();
                xmlAnimSubGroup.Tracks.Add(xmlTrack);

                if (target_types.ContainsKey(track.Target))
                {
                    xmlTrack.Target = target_types[track.Target];
                }

                foreach (var key in track.KeyFrames)
                {
                    xmlTrack.KeyFrames.Add(new XmlKeyFrame()
                    {
                        Frame = key.Frame,
                        Value = key.Value,
                        Slope = key.Slope,
                    });
                }
            }
            return xmlAnimSubGroup;
        }

        public static BflanFile FromXml(string xmlString)
        {
            XmlHeader header = Deserialize<XmlHeader>(xmlString);
            BflanFile bflan = new BflanFile();
            bflan.Magic = header.Header.Magic;
            bflan.ByteOrderMark = (ushort)Enum.Parse(typeof(ByteOrder), header.Header.ByteOrderMark);
            bflan.VersionMajor = header.Header.VersionMajor;
            bflan.VersionMinor = header.Header.VersionMinor;
            bflan.VersionMicro = header.Header.VersionMicro;

            if (header.TagInfo != null)
            {
                bflan.TagInfo = new TagInfo()
                {
                    AnimationOrder = header.TagInfo.AnimationOrder,
                    ChildBinding = header.TagInfo.ChildBinding,
                    StartFrame = header.TagInfo.StartFrame,
                    EndFrame = header.TagInfo.EndFrame,
                    Name = header.TagInfo.Name,
                    Groups = header.TagInfo.Groups,
                    UnknownData = header.TagInfo.UnknownData,
                    UserData = header.TagInfo.UserData
                };
            }

            if (header.AnimationInfo != null)
            {
                bflan.AnimationInfo = new AnimationInfo()
                {
                    Textures = header.AnimationInfo.Textures,
                    FrameSize = header.AnimationInfo.FrameSize,
                    Loop = header.AnimationInfo.Loop
                };

                foreach (var xmlGroup in header.AnimationInfo.Groups)
                {
                    AnimationInfoGroup group = new AnimationInfoGroup
                    {
                        Name = xmlGroup.Name,
                        Type = (AnimationTargetType)Enum.Parse(typeof(AnimationTargetType), xmlGroup.Type)
                    };

                    foreach (var xmlSubGroup in xmlGroup.SubGroups)
                        group.Tags.Add(ConvertXmlSubGroup(xmlSubGroup, header.Header.Magic));

                    bflan.AnimationInfo.Entries.Add(group);
                }
            }

            return bflan;
        }

        public static AnimationInfoSubGroup ConvertXmlSubGroup(XmlAnimationSubGroup xmlSubGroup, string version = "FLAN")
        {
            AnimationInfoSubGroup subGroup = new AnimationInfoSubGroup
            {
                Kind = xmlSubGroup.Kind
            };

            if (AnimationInfoSubGroup.TypeDefine.ContainsValue(xmlSubGroup.Kind))
            {
                subGroup.Kind = AnimationInfoSubGroup.TypeDefine.FirstOrDefault(x =>
                x.Value == xmlSubGroup.Kind).Key;
            }

            //Set magic start
            switch (version)
            {
                case "RLAN": subGroup.Kind = $"R{subGroup.Kind}"; break;
                case "CLAN": subGroup.Kind = $"C{subGroup.Kind}"; break;
                case "FLAN": subGroup.Kind = $"F{subGroup.Kind}"; break;
            }

            Dictionary<string, byte> targetTypes = new Dictionary<string, byte>();

            if (AnimationInfoSubGroup.TypeEnumDefine.ContainsKey(subGroup.Kind))
            {
                var targetEnum = AnimationInfoSubGroup.TypeEnumDefine[subGroup.Kind];
                targetTypes = Enum.GetValues(targetEnum).Cast<byte>().ToDictionary(v => Enum.GetName(targetEnum, v), v => v);
            }

            foreach (var xmlTrack in xmlSubGroup.Tracks)
            {
                AnimationTarget track = new AnimationTarget
                {
                    CurveType = (AnimCurveType)Enum.Parse(typeof(AnimCurveType), xmlTrack.CurveType),
                    Index = xmlTrack.Index,
                    Target = targetTypes.ContainsKey(xmlTrack.Target)
                             ? (byte)targetTypes[xmlTrack.Target]
                             : byte.Parse(xmlTrack.Target)
                };

                foreach (var xmlKeyFrame in xmlTrack.KeyFrames)
                {
                    track.KeyFrames.Add(new KeyFrame
                    {
                        Frame = xmlKeyFrame.Frame,
                        Value = xmlKeyFrame.Value,
                        Slope = xmlKeyFrame.Slope
                    });
                }
                subGroup.Targets.Add(track);
            }
            return subGroup;
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

            [XmlElement]
            public XmlTagInfo TagInfo;

            [XmlElement]
            public XmlAnimationInfo AnimationInfo;
        }

        public class XmlTagInfo
        {
            [XmlAttribute]
            public string Name = "";

            [XmlAttribute]
            public ushort AnimationOrder = 2;

            [XmlAttribute]
            public short StartFrame;

            [XmlAttribute]
            public short EndFrame;

            [XmlAttribute]
            public bool ChildBinding;

            [XmlAttribute]
            public byte[] UnknownData = new byte[3];

            public List<string> Groups = new List<string>();

            public UserData UserData;
        }

        public class XmlAnimationInfo
        {
            public List<string> Textures = new List<string>();

            [XmlAttribute]
            public ushort FrameSize;

            [XmlAttribute]
            public bool Loop;

            [XmlArray("AnimGroups")]
            [XmlArrayItem("Group")]
            public List<XmlAnimationGroup> Groups = new List<XmlAnimationGroup>();
        }

        public class XmlAnimationGroup
        {
            [XmlAttribute]
            public string Name;

            [XmlAttribute]
            public string Type;


            [XmlArray("AnimSubGroups")]
            [XmlArrayItem("SubGroup")]
            public List<XmlAnimationSubGroup> SubGroups = new List<XmlAnimationSubGroup>();
        }

        public class XmlAnimationSubGroup
        {
            [XmlAttribute]
            public string Kind;
            [XmlAttribute]
            public byte Type;

            [XmlArrayItem("Track")]
            public List<XmlAnimationTrack> Tracks = new List<XmlAnimationTrack>();
        }

        public class XmlAnimationTrack
        {
            [XmlAttribute]
            public byte Index;
            [XmlAttribute]
            public string Target;
            [XmlAttribute]
            public string CurveType;

            [XmlArrayItem("KeyFrame")]
            public List<XmlKeyFrame> KeyFrames = new List<XmlKeyFrame>();

        }

        public class XmlKeyFrame
        {
            [XmlAttribute]
            public float Frame;
            [XmlAttribute]
            public float Value;
            [XmlAttribute]
            public float Slope;
        }
    }
}
