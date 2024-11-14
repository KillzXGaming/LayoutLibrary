using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class BoundsPane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "bnd1";
    }
}
