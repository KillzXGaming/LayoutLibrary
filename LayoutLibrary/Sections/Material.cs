using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public List<Material> Materials { get; set; } = new List<Material>();
    }

    /// <summary>
    /// Material for displaying and rendering layout data.
    /// </summary>
    public class Material 
    {
        /// <summary>
        /// The name of the material.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Bitflags describing the contents of the material.
        /// </summary>
        public MaterialBitfield Flags { get; set; }

        /// <summary>
        /// The color kind in bits for each count (total of 8). 
        /// If bit = 1, type is float.
        /// </summary>
        public byte ColorType { get; set; }

        public List<Color> Colors { get; set; } = new List<Color>();

        public List<MaterialTextureMap> Textures { get; set; } = new List<MaterialTextureMap>();
        public List<MaterialTextureSrt> TextureSrts { get; set; } = new List<MaterialTextureSrt>();
        public List<MaterialTexCoordGen> TexCoordGens { get; set; } = new List<MaterialTexCoordGen>();
        public List<ProjectionTexGenParam> ProjectionTexGens { get; set; } = new List<ProjectionTexGenParam>();
        public List<MaterialTextureExtension> TextureExtensions { get; set; } = new List<MaterialTextureExtension>();
        public List<MaterialTevCombiner> TevCombiners { get; set; } = new List<MaterialTevCombiner>();

        public byte[] DetailedCombinerData = new byte[0];
        public List<MaterialDetailedCombiner> DetailedCombiners { get; set; } = new List<MaterialDetailedCombiner>();
        public List<MaterialUserCombiner> UserCombiners { get; set; } = new List<MaterialUserCombiner>();
        public List<AlphaCompare> AlphaCompares { get; set; } = new List<AlphaCompare>();

        public BlendMode ColorBlend = new BlendMode();
        public BlendMode AlphaBlend = new BlendMode();

        public Color BlackColor => Colors.Count > 0 ? Colors[0] : Color.Black;
        public Color WhiteColor => Colors.Count > 1 ? Colors[1] : Color.White;

        public IndirectParameter IndirectParameter = new IndirectParameter();

        public FontShadowParameter FontShadowParameter = new FontShadowParameter();

        public List<BrickRepeatShaderInfo> BrickRepeatShaderInfos { get; set; } = new List<BrickRepeatShaderInfo>();

        public byte[] Raw;
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
        public Color BlackColor;
        public Color WhiteColor;
    }

    public class IndirectParameter
    {
        public float Rotation;
        public Vector2 Scale;
    }

    public class MaterialTextureMap
    {
        public ushort TextureIndex;
        public ushort Flags;

        public WrapMode WrapModeU
        {
            get => (WrapMode)BitUtils.GetBits(Flags, 0, 2);
            set => Flags = (byte)BitUtils.SetBits(Flags, 0, 2, (byte)value);
        }

        public WrapMode WrapModeV
        {
            get => (WrapMode)BitUtils.GetBits(Flags, 8, 2);
            set => Flags = (byte)BitUtils.SetBits(Flags, 8, 2, (byte)value);
        }

        public FilterMode MinFilterMode
        {
            get => (FilterMode)BitUtils.GetBits(Flags, 2, 2);
            set => Flags = (byte)BitUtils.SetBits(Flags, 2, 2, (byte)value);
        }

        public FilterMode MagFilterMode
        {
            get => (FilterMode)BitUtils.GetBits(Flags, 10, 2);
            set => Flags = (byte)BitUtils.SetBits(Flags, 10, 2, (byte)value);
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
        public byte Data;
        public byte Reserved0;
        public byte Reserved1;
        public byte Reserved2;
    }

    public class MaterialDetailedCombiner
    {
        public byte[] Data;
    }

    public class MaterialUserCombiner
    {
        public byte[] Data;
    }

    public class BrickRepeatShaderInfo
    {
        public byte[] Data;
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

        public uint GetFlags() => _data;

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

        public bool EnableIndirectParams
        {
            get => GetBits(14, 1) != 0;
            set => SetBits(14, 1, value ? 1u : 0u);
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
