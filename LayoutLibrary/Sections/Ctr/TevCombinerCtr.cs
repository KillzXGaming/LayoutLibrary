using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary.Ctr
{
    public class TevCombinerCtr
    {
        public TevMode ColorMode { get; set; }
        public TevMode AlphaMode { get; set; }

        public TevSource[] ColorSources;
        public TevColorOp[] ColorOperators;
        public TevScale ColorScale;
        public Boolean ColorSavePrevReg;

        public byte ColorUnknown;
        public TevSource[] AlphaSources;
        public TevAlphaOp[] AlphaOperators;
        public TevScale AlphaScale;
        public Boolean AlphaSavePrevReg;

        public uint ConstColors;

        private uint flags1;
        private uint flags2;

        public TevCombinerCtr() { }

        public TevCombinerCtr(FileReader reader)
        {
            flags1 = reader.ReadUInt32();
            ColorSources = new TevSource[] { (TevSource)(flags1 & 0xF), (TevSource)((flags1 >> 4) & 0xF), (TevSource)((flags1 >> 8) & 0xF) };
            ColorOperators = new TevColorOp[] { (TevColorOp)((flags1 >> 12) & 0xF), (TevColorOp)((flags1 >> 16) & 0xF), (TevColorOp)((flags1 >> 20) & 0xF) };
            ColorMode = (TevMode)((flags1 >> 24) & 0xF);
            ColorScale = (TevScale)((flags1 >> 28) & 0x3);
            ColorSavePrevReg = ((flags1 >> 30) & 0x1) == 1;
            flags2 = reader.ReadUInt32();
            AlphaSources = new TevSource[] { (TevSource)(flags2 & 0xF), (TevSource)((flags2 >> 4) & 0xF), (TevSource)((flags2 >> 8) & 0xF) };
            AlphaOperators = new TevAlphaOp[] { (TevAlphaOp)((flags2 >> 12) & 0xF), (TevAlphaOp)((flags2 >> 16) & 0xF), (TevAlphaOp)((flags2 >> 20) & 0xF) };
            AlphaMode = (TevMode)((flags2 >> 24) & 0xF);
            AlphaScale = (TevScale)((flags2 >> 28) & 0x3);
            AlphaSavePrevReg = ((flags2 >> 30) & 0x1) == 1;

            ConstColors = reader.ReadUInt32();
        }

        public void Write(FileWriter writer)
        {
            UpdateFlags();

            writer.Write(flags1);
            writer.Write(flags2);
            writer.Write(ConstColors);
        }

        private void UpdateFlags()
        {
            flags1 = GenerateFlags(ColorSources, ColorOperators.Select(x => (uint)x).ToArray(), ColorMode, ColorScale, ColorSavePrevReg);
            flags2 = GenerateFlags(AlphaSources, AlphaOperators.Select(x => (uint)x).ToArray(), AlphaMode, AlphaScale, AlphaSavePrevReg);
        }

        private uint GenerateFlags(TevSource[] source, uint[] operators, TevMode mode, TevScale scale, bool savePrevReg)
        {
            uint flags1 = 0;

            flags1 |= (uint)source[0];
            flags1 |= (uint)source[1] << 4;
            flags1 |= (uint)source[2] << 8;

            flags1 |= (uint)operators[0] << 12;
            flags1 |= (uint)operators[1] << 16;
            flags1 |= (uint)operators[2] << 20;

            flags1 |= (uint)mode << 24;
            flags1 |= (uint)scale << 28;

            if (savePrevReg)
                flags1 |= 1U << 30;

            return flags1;
        }
    }

    public enum TevMode : byte
    {
        Replace,
        Modulate,
        Add,
        AddSigned,
        Interpolate,
        Subtract,
        AddMultiplicate,
        MultiplcateAdd,
        Overlay,
        Indirect,
        BlendIndirect,
        EachIndirect,
    }



    public enum TevScale
    {
        Scale1,
        Scale2,
        Scale4
    }

    public enum TevSource
    {
        Tex0 = 0,
        Tex1 = 1,
        Tex2 = 2,
        Tex3 = 3,
        Constant = 4,
        Primary = 5,
        Previous = 6,
        Register = 7
    }
    public enum TevColorOp
    {
        RGB = 0,
        InvRGB = 1,
        Alpha = 2,
        InvAlpha = 3,
        RRR = 4,
        InvRRR = 5,
        GGG = 6,
        InvGGG = 7,
        BBB = 8,
        InvBBB = 9
    }
    public enum TevAlphaOp
    {
        Alpha = 0,
        InvAlpha = 1,
        R = 2,
        InvR = 3,
        G = 4,
        InvG = 5,
        B = 6,
        InvB = 7
    }
}
