using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    public static class WriteUtility
    {
        public static void Write(this FileWriter writer, Vector2 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
        }

        public static void Write(this FileWriter writer, Vector3 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
        }

        public static void Write(this FileWriter writer, Vector4 v)
        {
            writer.Write(v.X);
            writer.Write(v.Y);
            writer.Write(v.Z);
            writer.Write(v.W);
        }

    }
}
