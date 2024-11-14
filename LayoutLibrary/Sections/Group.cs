using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LayoutLibrary
{
    /// <summary>
    /// A group for storing pane references.
    /// These can be in a hierachy and hardcoded to do multiple things.
    /// </summary>
    public class Group 
    {
        /// <summary>
        /// The group name.
        /// </summary>
        public string Name { get; set; } = "A_Group";

        /// <summary>
        /// A list of pane references.
        /// </summary>
        public List<string> Panes { get; set; } = new List<string>();

        private Group parent = null;

        /// <summary>
        /// The parent of this group.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Group Parent
        {
            get => parent;
            set
            {
                if (value != null)
                {
                    if (!value.Children.Contains(this))
                        value.Children.Add(this);
                    this.parent = value;
                }
            }
        }

        /// <summary>
        /// Children connected to this group.
        /// </summary>
        public List<Group> Children { get; set; } = new List<Group>();
    }
}
