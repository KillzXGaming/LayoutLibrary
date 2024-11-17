using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
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

        public ScissorPane() { }
        public ScissorPane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal override void Read(FileReader reader, LayoutHeader header)
        {
            base.Read(reader, header);
        }

        internal override void Write(FileWriter writer, LayoutHeader header)
        {
            base.Write(writer, header);
        }

        #endregion
    }
}
