using LayoutLibrary.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary.XmlConverter
{
    public class XmlControlSource
    {
        public string Name { get; set; } = "";
        public string ControlName { get; set; } = "";
        public List<string> Panes { get; set; } = new List<string>();
        public List<string> Animations { get; set; } = new List<string>();
        public List<string> PaneStates { get; set; } = new List<string>();
        public List<string> AnimationStates { get; set; } = new List<string>();

        public XmlUserData UserData { get; set; }

        public XmlControlSource() { }

        public XmlControlSource(ControlSource controlSource)
        {
            this.Animations = controlSource.Animations;
            this.AnimationStates = controlSource.AnimationStates;
            this.ControlName = controlSource.ControlName;
            this.PaneStates = controlSource.PaneStates;
            this.Name = controlSource.Name;
            this.Panes = controlSource.Panes;

            if (controlSource.UserData != null)
                this.UserData = new XmlUserData(controlSource.UserData);
        }

        public ControlSource Create()
        {
            return new ControlSource()
            {
                Animations = this.Animations,
                AnimationStates = this.AnimationStates,
                Panes = this.Panes,
                ControlName = this.ControlName,
                PaneStates = this.PaneStates,
                Name = this.Name,
                UserData = UserData != null ? UserData.Create() : null,
            };
        }
    }
}
