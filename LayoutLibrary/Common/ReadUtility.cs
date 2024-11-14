using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public static class ReadUtility
    {
        public static Vector2 ReadVec2(this FileReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVec3(this FileReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVec4(this FileReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(),
                               reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
