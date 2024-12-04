using LayoutLibrary.Cafe;
using LayoutLibrary.Ctr;
using LayoutLibrary.Files;
using LayoutLibrary.Sections.Rev;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary
{
    /// <summary>
    /// A table of material data.
    /// Used by picture, window, and other panes by indexing the material list.
    /// </summary>
    public class MaterialTable 
    {
        /// <summary>
        /// The material list.
        /// </summary>
        public List<MaterialBase> Materials { get; set; } = new List<MaterialBase>();

        public ushort GetMaterialIndex(MaterialBase material)
        {
            return (ushort)material.Index;

            var idx = Materials.FindIndex(x => x.Name == material.Name);
            if (idx == -1)
            {
                idx = Materials.Count;
                Materials.Add(material);
            }

            return (ushort)idx;
        }

        public MaterialTable() { }

        public MaterialTable(FileReader reader, LayoutHeader header)
        {
            long pos = reader.Position - 8;

            reader.SeekBegin(pos + 4);
            uint sectionSize = reader.ReadUInt32();

            ushort numMats = reader.ReadUInt16();
            reader.Seek(2); //padding

            uint[] offsets = reader.ReadUInt32s(numMats);
            for (int i = 0; i < numMats; i++)
            {
                reader.SeekBegin(pos + offsets[i]);

                var size = sectionSize - offsets[i];
                if (i < numMats - 1)
                    size = offsets[i + 1] - offsets[i];

                this.Materials.Add(MaterialBase.ReadMaterial(reader, header, (int)size));
            }
        }

        public void Write(FileWriter writer, LayoutHeader header)
        {
            long pos = writer.Position - 8;

            writer.Write((ushort)this.Materials.Count);
            writer.Write((ushort)0);

            //offset allocate
            writer.Write(new uint[this.Materials.Count]);

            for (int i = 0; i < this.Materials.Count; i++)
            {
                writer.WriteUint32Offset(pos + 12 + i * 4, (int)pos);
                this.Materials[i].WriteMaterial(writer, header);
            }
        }
    }

    /// <summary>
    /// Material for displaying and rendering layout data.
    /// </summary>
    public class MaterialBase
    {
        /// <summary>
        /// The name of the material.
        /// </summary>
        public virtual string Name { get; set; }

        public int Index { get; set; }

        internal static MaterialBase ReadMaterial(FileReader reader, LayoutHeader header, int size)
        {
            if (header.IsCTR)
                return new MaterialCtr(reader, header, size);
            if (header.IsRev)
                return new MaterialRev(reader);

            return new MaterialCafe(reader, header, size);
        }

        internal virtual void WriteMaterial(FileWriter writer, LayoutHeader header)
        {

        }
    }

    public class MaterialTextureSrt
    {
        public Vector2 Translate;
        public float Rotate;
        public Vector2 Scale;
    }

    public class MaterialTexCoordGen
    {
        public TexGenMatrixType MatrixType { get; set; }
        public TexGenType Source { get; set; }

        public ushort Unknown;
        public uint Unknown2;

        //Version >= 8
        public ulong Unknown3;
    }

    public class ProjectionTexGenParam
    {
        public Vector2 Position;
        public Vector2 Scale;
        public uint Flags;
    }

    public class FontShadowParameter
    {
        public Color BlackColor = Color.Black;
        public Color WhiteColor = Color.White;
    }

    public class IndirectParameter
    {
        public float Rotation;
        public Vector2 Scale;
    }

    public class MaterialTextureMap
    {
        public ushort TextureIndex;

        public byte Flag1 = 0;
        public byte Flag2 = 0;

        public WrapMode WrapModeU
        {
            get => (WrapMode)BitUtils.GetBits(Flag1, 0, 2);
            set => Flag1 = (byte)BitUtils.SetBits(Flag1, (int)value, 0, 2);
        }

        public FilterMode MinFilterMode
        {
            get => (FilterMode)BitUtils.GetBits(Flag1, 2, 2);
            set => Flag1 = (byte)BitUtils.SetBits(Flag1, (int)value, 2, 2);
        }

        public WrapMode WrapModeV
        {
            get => (WrapMode)BitUtils.GetBits(Flag2, 0, 2);
            set => Flag2 = (byte)BitUtils.SetBits(Flag2, (int)value, 0, 2);
        }
        public FilterMode MagFilterMode
        {
            get => (FilterMode)BitUtils.GetBits(Flag2, 2, 2);
            set => Flag2 = (byte)BitUtils.SetBits(Flag2, (int)value, 2, 2);
        }
    }

    public class MaterialTextureExtension
    {
        public int Flags;

        public bool IsCapture
        {
            get => BitUtils.GetBit(Flags, 0);
            set => Flags = BitUtils.SetBit(Flags, value, 0);
        }

        public bool IsVector
        {
            get => BitUtils.GetBit(Flags, 1);
            set => Flags = BitUtils.SetBit(Flags, value, 1);
        }

        public MaterialTextureExtension(int flags)
        {
            Flags = flags;
        }
    }

    public class MaterialTevCombiner
    {
        public byte ColorFlags;
        public byte AlphaFlags;
        public byte Reserved1;
        public byte Reserved2;
    }

    public class MaterialDetailedCombiner
    {
        public uint Value1;

        public Color Color1;
        public Color Color2;
        public Color Color3;
        public Color Color4;
        public Color Color5;
        public Color Color6;

        public List<MaterialDetailedCombinerEntry> Entries = new List<MaterialDetailedCombinerEntry>();
    }

    public class MaterialDetailedCombinerEntry
    {
        public TevSource[] ColorSources = new TevSource[3];
        public TevSource[] AlphaSources = new TevSource[3];
        public TevColorOp[] ColorOps = new TevColorOp[3];
        public TevAlphaOp[] AlphaOps = new TevAlphaOp[3];

        public TevMode ColorMode;
        public TevScale ColorScale;

        public TevMode AlphaMode;
        public TevScale AlphaScale;

        public uint Unknown1; //konst color flags?
        public int ColorFlags;
        public int AlphaFlags;
        public uint Unknown2; //34

        public MaterialDetailedCombinerEntry() { }
        public MaterialDetailedCombinerEntry(FileReader reader)
        {
            ColorFlags = reader.ReadInt32();
            AlphaFlags = reader.ReadInt32();
            Unknown1 = reader.ReadUInt32();
            Unknown2 = reader.ReadUInt32();

            ColorSources[0] = (TevSource)BitUtils.GetBits(ColorFlags, 0, 4);
            ColorSources[1] = (TevSource)BitUtils.GetBits(ColorFlags, 4, 4);
            ColorSources[2] = (TevSource)BitUtils.GetBits(ColorFlags, 8, 4);
            ColorOps[0] = (TevColorOp)BitUtils.GetBits(ColorFlags, 12, 4);
            ColorOps[1] = (TevColorOp)BitUtils.GetBits(ColorFlags, 16, 4);
            ColorOps[2] = (TevColorOp)BitUtils.GetBits(ColorFlags, 20, 4);
            ColorMode = (TevMode)BitUtils.GetBits(ColorFlags, 24, 4);
            ColorScale = (TevScale)BitUtils.GetBits(ColorFlags, 28, 3);

            AlphaSources[0] = (TevSource)BitUtils.GetBits(AlphaFlags, 0, 4);
            AlphaSources[1] = (TevSource)BitUtils.GetBits(AlphaFlags, 4, 4);
            AlphaSources[2] = (TevSource)BitUtils.GetBits(AlphaFlags, 8, 4);
            AlphaOps[0] = (TevAlphaOp)BitUtils.GetBits(AlphaFlags, 12, 4);
            AlphaOps[1] = (TevAlphaOp)BitUtils.GetBits(AlphaFlags, 16, 4);
            AlphaOps[2] = (TevAlphaOp)BitUtils.GetBits(AlphaFlags, 20, 4);
            AlphaMode = (TevMode)BitUtils.GetBits(AlphaFlags, 24, 4);
            AlphaScale = (TevScale)BitUtils.GetBits(AlphaFlags, 28, 3);
        }

        public void Write(FileWriter writer)
        {
            writer.Write(ColorFlags);
            writer.Write(AlphaFlags);
            writer.Write(Unknown1);
            writer.Write(Unknown2);
        }

        public enum TevSource : byte
        {
            Primary = 0,
            Texture0 = 3,
            Texture1 = 4,
            Texture2 = 5,
            Constant = 14,
            Previous = 15,
        }

        public enum TevScale
        {
            Scale1,
            Scale2,
            Scale4
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

    public class MaterialUserCombiner
    {
        public string Name; //fixed string 0x60

        public Color Color1;
        public Color Color2;
        public Color Color3;
        public Color Color4;
        public Color Color5;
    }

    public class BrickRepeatShaderInfo
    {
        public Vector2 Scale1;
        public Vector2 Offset1;

        public Vector2 Scale2;
        public Vector2 Offset2;

        public Vector4 Unknown1;
        public Vector2 Unknown2;

        public Vector2 RotationRange = new Vector2(-180, 180);

        public Vector2 Unknown3;
        public Vector4 Unknown4;


        public float[] Data;
    }

    public class AlphaCompare
    {
        public GfxAlphaFunction CompareMode { get; set; }
        public float Value { get; set; }
    }

    public class BlendMode
    {
        public GfxBlendOp BlendOp { get; set; }
        public GfxBlendFactor SourceFactor { get; set; }
        public GfxBlendFactor DestFactor { get; set; }
        public GfxLogicOp LogicOp { get; set; }
    }

    public class MaterialBitfield
    {
        private uint _data;

        public uint ToUInt32() => _data;

        public byte TexMapCount
        {
            get => GetBits(0, 2);
            set => SetBits(0, 2, value);
        }

        public byte TexSrtCount
        {
            get => GetBits(2, 2);
            set => SetBits(2, 2, value);
        }

        public byte TexCoordGenCount
        {
            get => GetBits(4, 2);
            set => SetBits(4, 2, value);
        }

        public byte TevCombinerCount
        {
            get => GetBits(6, 3);
            set => SetBits(6, 3, value);
        }

        public byte AlphaCompareCount
        {
            get => GetBits(9, 1);
            set => SetBits(9, 1, value);
        }

        public bool ColorBlendMode
        {
            get => GetBits(10, 1) != 0;
            set => SetBits(10, 1, value ? 1u : 0u);
        }

        public bool UseTextureOnly
        {
            get => GetBits(11, 1) != 0;
            set => SetBits(11, 1, value ? 1u : 0u);
        }

        public bool ColorAndAlphaBlendMode
        {
            get => GetBits(12, 1) != 0;
            set => SetBits(12, 1, value ? 1u : 0u);
        }

        public byte Reserve1
        {
            get => GetBits(13, 1);
            set => SetBits(13, 1, value);
        }

        public byte IndirectSrtCount
        {
            get => GetBits(14, 1);
            set => SetBits(14, 1, value);
        }

        public byte ProjectionTexGenCount
        {
            get => GetBits(15, 2);
            set => SetBits(15, 2, value);
        }

        public bool EnableFontShadowParams
        {
            get => GetBits(17, 1) != 0;
            set => SetBits(17, 1, value ? 1u : 0u);
        }

        public byte AlphaInterpolation
        {
            get => GetBits(18, 1);
            set => SetBits(18, 1, value);
        }

        public bool UseDetailedCombiner
        {
            get => GetBits(19, 1) != 0;
            set => SetBits(19, 1, value ? 1u : 0u);
        }

        public byte UserCombinerCount
        {
            get => GetBits(20, 1);
            set => SetBits(20, 1, value);
        }

        public bool HasTextureExtensions
        {
            get => GetBits(21, 1) != 0;
            set => SetBits(21, 1, value ? 1u : 0u);
        }

        public byte VectorTextureInfoCount
        {
            get => GetBits(22, 2);
            set => SetBits(22, 2, value);
        }

        public byte BrickRepeatShaderInfoCount
        {
            get => GetBits(24, 2);
            set => SetBits(24, 2, value);
        }

        public byte Reserve5
        {
            get => GetBits(26, 6);
            set => SetBits(26, 6, value);
        }

        public MaterialBitfield(uint flags)
        {
            _data = flags;
        }

        public override string ToString() => Convert.ToString(_data, 2).PadLeft(32, '0');

        private byte GetBits(int startBit, int count)
        {
            uint mask = (1u << count) - 1;
            return (byte)((_data >> startBit) & mask);
        }

        private void SetBits(int startBit, int count, uint value)
        {
            uint mask = (1u << count) - 1;
            _data = (_data & ~(mask << startBit)) | ((value & mask) << startBit);
        }
    }
}
