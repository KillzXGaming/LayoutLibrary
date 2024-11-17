using LayoutLibrary.Files;
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

        public AlignmentPane() { }
        public AlignmentPane(FileReader reader, LayoutHeader header) { Read(reader, header); }

        #region Read/Write

        internal override void Read(FileReader reader, LayoutHeader header)
        {
            base.Read(reader, header);
            AlignmentValue = reader.ReadVec3();
        }

        internal override void Write(FileWriter writer, LayoutHeader header)
        {
            base.Write(writer, header);
            writer.Write(AlignmentValue);
        }

        #endregion
    }
}
