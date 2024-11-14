using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// A pane used for alignment.
    /// Possibly used for aligning children.
    /// </summary>
    public class AlignmentPane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "ali1";

        /// <summary>
        /// Alignment value.
        /// </summary>
        public Vector3 AlignmentValue { get; set; }
    }
}
