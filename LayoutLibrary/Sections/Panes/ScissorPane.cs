using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// Scissor pane.
    /// </summary>
    public class ScissorPane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "scr1";
    }
}
