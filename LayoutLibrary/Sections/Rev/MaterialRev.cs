using LayoutBXLYT.Revolution;
using LayoutLibrary.Files;
using LayoutLibrary.Revolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LayoutLibrary.Sections.Rev
{
    public class MaterialRev : MaterialBase
    {
        public MaterialBitfield Flags = new MaterialBitfield(0);

        public override string Name { get; set; } = "";

        public Color16 BlackColor { get; set; } = Color16.Black;
        public Color16 WhiteColor { get; set; } = Color16.White;

        public Color16 ColorRegister3 { get; set; } = Color16.White;
        public Color MatColor { get; set; } = Color.White;
        public Color TevColor1 { get; set; } = Color.White;
        public Color TevColor2 { get; set; } = Color.White;
        public Color TevColor3 { get; set; } = Color.White;
        public Color TevColor4 { get; set; } = Color.White;

        public List<MaterialTextureMap> TextureMaps { get; set; } = new List<MaterialTextureMap>();
        public List<MaterialTextureSrt> TextureSrts { get; set; } = new List<MaterialTextureSrt>();

        public List<TexCoordGenEntry> TexCoordGens { get; set; } = new List<TexCoordGenEntry>();
        public ChanCtrl ChanControl { get; set; } = new ChanCtrl();
        public TevSwapModeTable TevSwapModeTable { get; set; } = new TevSwapModeTable();
        public List<MaterialTextureSrt> IndirectTexTransforms { get; set; } = new List<MaterialTextureSrt>();

        public List<IndirectStage> IndirectStages { get; set; } = new List<IndirectStage>();
        public List<TevStage> TevStages { get; set; } = new List<TevStage>();

        public AlphaCompareRev AlphaCompare { get; set; }
        public BlendMode BlendMode { get; set; }

        public MaterialRev() { }
        public MaterialRev(FileReader reader) : base()
        {
            Name = reader.ReadFixedString(0x14);

            BlackColor = new Color16(reader.ReadUInt16s(4));
            WhiteColor = new Color16(reader.ReadUInt16s(4));
            ColorRegister3 = new Color16(reader.ReadUInt16s(4));
            TevColor1 = new Color(reader.ReadUInt32());
            TevColor2 = new Color(reader.ReadUInt32());
            TevColor3 = new Color(reader.ReadUInt32());
            TevColor4 = new Color(reader.ReadUInt32());
            Flags = new MaterialBitfield(reader.ReadUInt32());

            for (int i = 0; i < Flags.TextureCount; i++)
                TextureMaps.Add(new MaterialTextureMap()
                {
                    TextureIndex = reader.ReadUInt16(),
                    Flag1 = reader.ReadByte(),
                    Flag2 = reader.ReadByte(),
                });

            for (int i = 0; i < Flags.TexSrtCount; i++)
                TextureSrts.Add(new MaterialTextureSrt()
                {
                    Translate = reader.ReadVec2(),
                    Rotate = reader.ReadSingle(),
                    Scale = reader.ReadVec2(),
                });

            for (int i = 0; i < Flags.TexCoordGenCount; i++)
                TexCoordGens.Add(new TexCoordGenEntry(reader));

            if (Flags.HasChannelControl)
                ChanControl = new ChanCtrl(reader);

            if (Flags.HasMaterialColor)
                MatColor = new Color(reader.ReadUInt32());

            if (Flags.HasTevSwapTable)
                TevSwapModeTable = new TevSwapModeTable(reader);

            for (int i = 0; i < Flags.IndTexSrtCount; i++)
                IndirectTexTransforms.Add(new MaterialTextureSrt()
                {
                    Translate = reader.ReadVec2(),
                    Rotate = reader.ReadSingle(),
                    Scale = reader.ReadVec2(),
                });

            for (int i = 0; i < Flags.IndTexOrderCount; i++)
                IndirectStages.Add(new IndirectStage(reader));

            for (int i = 0; i < Flags.TevStagesCount; i++)
                TevStages.Add(new TevStage(reader));

            if (Flags.HasAlphaCompare)
                AlphaCompare = new AlphaCompareRev(reader);

            if (Flags.HasBlendMode)
                BlendMode = new BlendMode()
                {
                    BlendOp = (GfxBlendOp)reader.ReadByte(),
                    SourceFactor = (GfxBlendFactor)reader.ReadByte(),
                    DestFactor = (GfxBlendFactor)reader.ReadByte(),
                    LogicOp = (GfxLogicOp)reader.ReadByte(),
                };
        }

        internal override void WriteMaterial(FileWriter writer, LayoutHeader header)
        {
            this.Flags.TextureCount = (byte)this.TextureMaps.Count;
            this.Flags.TexCoordGenCount = (byte)this.TexCoordGens.Count;
            this.Flags.TexSrtCount = (byte)this.TextureSrts.Count;
            this.Flags.IndTexOrderCount = (byte)this.IndirectStages.Count;
            this.Flags.TevStagesCount = (byte)this.TevStages.Count;
            this.Flags.IndTexSrtCount = (byte)this.IndirectTexTransforms.Count;

            writer.WriteFixedString(Name, 0x14);
            writer.Write(BlackColor.ToUInt16s());
            writer.Write(WhiteColor.ToUInt16s());
            writer.Write(ColorRegister3.ToUInt16s());
            writer.Write(TevColor1.ToUInt32());
            writer.Write(TevColor2.ToUInt32());
            writer.Write(TevColor3.ToUInt32());
            writer.Write(TevColor4.ToUInt32());
            writer.Write(this.Flags.ToUInt32());

            for (int i = 0; i < TextureMaps.Count; i++)
            {
                writer.Write(TextureMaps[i].TextureIndex);
                writer.Write(TextureMaps[i].Flag1);
                writer.Write(TextureMaps[i].Flag2);
            }

            for (int i = 0; i < TextureSrts.Count; i++)
            {
                writer.Write(TextureSrts[i].Translate);
                writer.Write(TextureSrts[i].Rotate);
                writer.Write(TextureSrts[i].Scale);
            }

            for (int i = 0; i < TexCoordGens.Count; i++)
                TexCoordGens[i].Write(writer);

            if (Flags.HasChannelControl)
                ChanControl.Write(writer);
            if (Flags.HasMaterialColor)
                writer.Write(MatColor.ToUInt32());
            if (Flags.HasTevSwapTable)
                TevSwapModeTable.Write(writer);

            for (int i = 0; i < IndirectTexTransforms.Count; i++)
            {
                writer.Write(IndirectTexTransforms[i].Translate);
                writer.Write(IndirectTexTransforms[i].Rotate);
                writer.Write(IndirectTexTransforms[i].Scale);
            }

            for (int i = 0; i < IndirectStages.Count; i++)
                IndirectStages[i].Write(writer);

            for (int i = 0; i < TevStages.Count; i++)
                TevStages[i].Write(writer);

            if (Flags.HasAlphaCompare)
                AlphaCompare.Write(writer);
            if (Flags.HasBlendMode)
            {
                writer.Write((byte)BlendMode.BlendOp);
                writer.Write((byte)BlendMode.SourceFactor);
                writer.Write((byte)BlendMode.DestFactor);
                writer.Write((byte)BlendMode.LogicOp);
            }
        }

        public class MaterialBitfield
        {
            private uint _data;

            public uint ToUInt32() => _data;

            public byte Reserved
            {
                get => GetBits(0, 4);
                set => SetBits(0, 4, value);
            }

            public bool HasMaterialColor
            {
                get => GetBits(4, 1) != 0;
                set => SetBits(4, 1, value ? 1u : 0);
            }

            public bool HasChannelControl
            {
                get => GetBits(6, 1) != 0;
                set => SetBits(6, 1, value ? 1u : 0);
            }

            public bool HasBlendMode
            {
                get => GetBits(7, 1) != 0;
                set => SetBits(7, 1, value ? 1u : 0);
            }

            public bool HasAlphaCompare
            {
                get => GetBits(8, 1) != 0;
                set => SetBits(8, 1, value ? 1u : 0);
            }

            public byte TevStagesCount
            {
                get => GetBits(9, 5);
                set => SetBits(9, 5, value);
            }

            public byte IndTexOrderCount
            {
                get => GetBits(16, 1);
                set => SetBits(16, 1, value);
            }

            public byte IndTexSrtCount
            {
                get => GetBits(17, 2);
                set => SetBits(17, 2, value);
            }

            public bool HasTevSwapTable
            {
                get => GetBits(19, 1) != 0;
                set => SetBits(19, 1, value ? 1u : 0u);
            }

            public byte TexCoordGenCount
            {
                get => GetBits(20, 4);
                set => SetBits(20, 4, value);
            }

            public byte TexSrtCount
            {
                get => GetBits(24, 4);
                set => SetBits(24, 4, value);
            }

            public byte TextureCount
            {
                get => GetBits(28, 4);
                set => SetBits(28, 4, value);
            }

            public MaterialBitfield(uint flags)
            {
                _data = flags;
            }

            public override string ToString() => Convert.ToString(_data, 2).PadLeft(32, '0');

            private byte GetBits(int startBit, int count)
            {
                uint mask = 0;
                for (int i = startBit; i < startBit + count; i++)
                    mask |= (0x80000000 >> i);

                return (byte)((_data & mask) >> (32 - (startBit + count)));
            }

            private void SetBits(int startBit, int count, uint value)
            {
                uint mask = 0;
                for (int i = startBit; i < startBit + count; i++)
                    mask |= (0x80000000 >> i);

                _data &= ~mask;
                uint alignedValue = (uint)value << (32 - (startBit + count));

                _data |= alignedValue & mask;
            }
        }
    }
}
