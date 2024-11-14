using LayoutLibrary.Cafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class ControlSource
    {
        public string Name { get; set; } = "";
        public string ControlName { get; set; } = "";
        public List<string> Panes { get; set; } = new List<string>();
        public List<string> Animations { get; set; } = new List<string>();
        public List<string> PaneStates { get; set; } = new List<string>();
        public List<string> AnimationStates { get; set; } = new List<string>();

        public UserData UserData { get; set; }
    }
}
