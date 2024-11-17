using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public class BoundsPane : Pane
    {
        [Newtonsoft.Json.JsonIgnore]
        public override string Magic => "bnd1";

        public BoundsPane() { }
        public BoundsPane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        internal override void Read(FileReader reader, LayoutHeader header)
        {
            base.Read(reader, header);
        }

        internal override void Write(FileWriter writer, LayoutHeader header)
        {
            base.Write(writer, header);
        }
    }
}
