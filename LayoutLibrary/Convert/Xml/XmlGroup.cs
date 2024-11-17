using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LayoutLibrary.XmlConverter
{
    public class XmlGroup
    {
        [XmlAttribute]
        public string Name;

        public List<string> Panes = new List<string>();

        [XmlArray("Children")]
        [XmlArrayItem("Group")]
        public XmlGroup[] Children;

        public XmlGroup() { }

        public XmlGroup(Group group)
        {
            this.Name = group.Name;
            this.Panes = group.Panes;

            this.Children = new XmlGroup[group.Children.Count];
            for (int i = 0; i < group.Children.Count; i++)
                this.Children[i] = new XmlGroup(group.Children[i]);
        }

        public Group Create()
        {
            Group group = new Group();
            group.Name = this.Name;
            group.Panes = this.Panes;
            for (int i = 0; i < this.Children.Length; i++)
                group.Children.Add(this.Children[i].Create());
            return group;
        }
    }
}
