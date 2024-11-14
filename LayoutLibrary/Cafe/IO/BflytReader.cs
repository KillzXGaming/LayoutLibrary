using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace LayoutLibrary.Cafe
{
    public class BflytReader
    {
        #region Section Reader

        public static List<string> ReadStringSection(FileReader reader, BflytFile header)
        {
            List<string> values = new List<string>();

            ushort count = reader.ReadUInt16();
            reader.Seek(2); //padding

            return ReadStringOffsets(reader, count);
        }

        public static List<string> ReadStringOffsets(FileReader reader, int count)
        {
            List<string> values = new List<string>();

            long pos = reader.Position;
            uint[] offsets = reader.ReadUInt32s(count);
            for (int i = 0; i < offsets.Length; i++)
            {
                reader.SeekBegin(offsets[i] + pos);
                values.Add(reader.ReadZeroTerminatedString());
            }
            return values;
        }

        #endregion

        #region LYT1

        public static Layout ReadLayout(FileReader reader, BflytFile header)
        {
            Layout lyt = new Layout();
            lyt.DrawFromCenter = reader.ReadBoolean();
            reader.Seek(3); //padding
            lyt.Width = reader.ReadSingle();
            lyt.Height = reader.ReadSingle();
            lyt.MaxPartsWidth = reader.ReadSingle();
            lyt.MaxPartsHeight = reader.ReadSingle();
            lyt.Name = reader.ReadZeroTerminatedString();
            return lyt;
        }

        #endregion

        #region GRP1

        public static Group ReadGroup(FileReader reader, BflytFile header)
        {
            Group grp = new Group();

            if (header.VersionMajor >= 5)
            {
                grp.Name = reader.ReadFixedString(0x21);
                reader.ReadByte();
                ushort num_children = reader.ReadUInt16();
                for (int i = 0; i < num_children; i++)
                    grp.Panes.Add(reader.ReadFixedString(0x18));
            }
            else
            {
                grp.Name = reader.ReadFixedString(0x18);
                ushort num_children = reader.ReadUInt16();
                reader.ReadUInt16();
                for (int i = 0; i < num_children; i++)
                    grp.Panes.Add(reader.ReadFixedString(0x18));
            }
            return grp;
        }

        #endregion

        #region PAN1

        public static Pane ReadPane(FileReader reader, BflytFile header)
        {
            Pane pane = new Pane();
            ReadPane(pane, reader, header);
            return pane;
        }

        public static void ReadPane(Pane pane, FileReader reader, BflytFile header)
        {
            pane.Flags1 = reader.ReadByte();
            byte origin = reader.ReadByte();
            pane.Alpha = reader.ReadByte();
            pane.PaneMagFlags = reader.ReadByte();
            pane.Name = reader.ReadFixedString(0x18);
            pane.UserDataInfo = reader.ReadFixedString(0x8);
            pane.Translate = reader.ReadVec3();
            pane.Rotate = reader.ReadVec3();
            pane.Scale = reader.ReadVec2();
            pane.Width = reader.ReadSingle();
            pane.Height = reader.ReadSingle();

            int mainorigin = origin % 16;
            int parentorigin = origin / 16;

            pane.OriginX = (OriginX)(mainorigin % 4);
            pane.OriginY = (OriginY)(mainorigin / 4);
            pane.ParentOriginX = (OriginX)(parentorigin % 4);
            pane.ParentOriginY = (OriginY)(parentorigin / 4);
        }

        #endregion

        #region MAT1

        public static MaterialTable ReadMaterialTable(FileReader reader, BflytFile header)
        {
            MaterialTable table = new MaterialTable();

            long pos = reader.Position - 8;

            reader.SeekBegin(pos + 4);
            uint sectionSize = reader.ReadUInt32();

            ushort numMats = reader.ReadUInt16();
            reader.Seek(2); //padding

            uint[] offsets = reader.ReadUInt32s(numMats);
            for (int i = 0; i < numMats; i++)
            {
                reader.SeekBegin(pos + offsets[i]);

                var size =  sectionSize - offsets[i];
                if (i < numMats - 1)
                    size = offsets[i + 1] - offsets[i];

                table.Materials.Add(ReadMaterial(reader, header, (int)size));
            }
            return table;
        }

        static Material ReadMaterial(FileReader reader, BflytFile header, int size)
        {
            var pos = reader.Position;

            Material mat = new Material();
            mat.Name = reader.ReadFixedString(0x1C);
            if (header.VersionMajor >= 8)
            {
                mat.Flags = new MaterialBitfield(reader.ReadUInt32());

                var cpos = reader.Position;
                mat.ColorType = reader.ReadByte();
                 var colorCount = reader.ReadByte();

                byte[] colorOffsets = reader.ReadBytes((int)colorCount);

                for (int i = 0; i < colorCount; i++)
                {
                    reader.SeekBegin(cpos + colorOffsets[i]);
                    var type = mat.ColorType >> i;
                    if (type == 0)
                        mat.Colors.Add(new Color(reader.ReadUInt32()));
                    else if (type == 1)
                        mat.Colors.Add(new Color(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()));

                }
            }
            else
            {
                mat.Colors.Add(new Color(reader.ReadUInt32()));
                mat.Colors.Add(new Color(reader.ReadUInt32()));
                mat.Flags = new MaterialBitfield(reader.ReadUInt32());
            }

            var start = reader.Position;
            var end = pos + size;

            var diff = start - pos;

            mat.Raw = reader.ReadBytes((int)size - (int)diff);

            reader.SeekBegin(start);
            for (int i = 0; i < mat.Flags.TexMapCount; i++)
            {
                mat.Textures.Add(new MaterialTextureMap()
                {
                    TextureIndex = reader.ReadUInt16(),
                    Flags = reader.ReadUInt16(),
                });
            }

            if (mat.Flags.HasTextureExtensions)
                mat.TextureExtensions.Add(new MaterialTextureExtension(reader.ReadInt32()));

            for (int i = 0; i < mat.Flags.TexSrtCount; i++)
            {
                mat.TextureSrts.Add(new MaterialTextureSrt()
                {
                    Translate = reader.ReadVec2(),
                    Rotate = reader.ReadSingle(),
                    Scale = reader.ReadVec2(),
                });
            }

            for (int i = 0; i < mat.Flags.TexCoordGenCount; i++)
            {
                MaterialTexCoordGen texCoordGen = new MaterialTexCoordGen();
                texCoordGen.MatrixType = (TexGenMatrixType)reader.ReadByte();
                texCoordGen.Source = (TexGenType)reader.ReadByte();
                texCoordGen.Unknown = reader.ReadUInt16();
                texCoordGen.Unknown2 = reader.ReadUInt32();

                if (header.VersionMajor >= 8)
                    texCoordGen.Unknown3 = reader.ReadUInt64();

                mat.TexCoordGens.Add(texCoordGen);
            }

            if (mat.Flags.UseDetailedCombiner)
            {
                mat.DetailedCombinerData = reader.ReadBytes(28); //7 tev colors?
                for (int i = 0; i < mat.Flags.TevCombinerCount; i++)
                {
                    mat.DetailedCombiners.Add(new MaterialDetailedCombiner()
                    {
                        Data = reader.ReadBytes(20),
                    });
                }
            }
            else
            {
                for (int i = 0; i < mat.Flags.TevCombinerCount; i++)
                {
                    mat.TevCombiners.Add(new MaterialTevCombiner()
                    {
                        Data = reader.ReadByte(),
                        Reserved0 = reader.ReadByte(),
                        Reserved1 = reader.ReadByte(),
                        Reserved2 = reader.ReadByte(),
                    });
                }
            }


            for (int i = 0; i < mat.Flags.UserCombinerCount; i++)
            {
                mat.UserCombiners.Add(new MaterialUserCombiner()
                {
                    Data = reader.ReadBytes(116),
                });
            }

            for (int i = 0; i < mat.Flags.AlphaCompareCount; i++)
            {
                var mode = reader.ReadByte();
                reader.ReadBytes(0x3);
                var value = reader.ReadSingle();

                mat.AlphaCompares.Add(new AlphaCompare()
                {
                    CompareMode = (GfxAlphaFunction)mode,
                    Value = value
                });
            }

            if (mat.Flags.ColorBlendMode)
                mat.ColorBlend = new BlendMode()
                {
                    BlendOp = (GfxBlendOp)reader.ReadByte(),
                    SourceFactor = (GfxBlendFactor)reader.ReadByte(),
                    DestFactor = (GfxBlendFactor)reader.ReadByte(),
                    LogicOp = (GfxLogicOp)reader.ReadByte(),
                };

            if (mat.Flags.ColorAndAlphaBlendMode)
                mat.AlphaBlend = new BlendMode()
                {
                    BlendOp = (GfxBlendOp)reader.ReadByte(),
                    SourceFactor = (GfxBlendFactor)reader.ReadByte(),
                    DestFactor = (GfxBlendFactor)reader.ReadByte(),
                    LogicOp = (GfxLogicOp)reader.ReadByte(),
                };

            if (mat.Flags.EnableIndirectParams)
                mat.IndirectParameter = new IndirectParameter()
                {
                    Rotation = reader.ReadSingle(),
                    Scale = reader.ReadVec2(),
                };

            for (int i = 0; i < mat.Flags.ProjectionTexGenCount; i++)
            {
                ProjectionTexGenParam texCoordGen = new ProjectionTexGenParam();
                texCoordGen.Position = reader.ReadVec2();
                texCoordGen.Scale = reader.ReadVec2();
                texCoordGen.Flags = reader.ReadUInt32();
                mat.ProjectionTexGens.Add(texCoordGen);
            }

            if (mat.Flags.EnableFontShadowParams)
            {
                mat.FontShadowParameter = new FontShadowParameter()
                {
                    BlackColor = new Color(reader.ReadUInt32()),
                    WhiteColor = new Color(reader.ReadUInt32()),
                };
            }

            for (int i = 0; i < mat.Flags.BrickRepeatShaderInfoCount; i++)
            {
                BrickRepeatShaderInfo info = new BrickRepeatShaderInfo();
                info.Data = reader.ReadBytes(88);
                mat.BrickRepeatShaderInfos.Add(info);
            }

            //Check how much data is left
            var s = end - reader.Position;

            if (s != 0 )
                Console.WriteLine($"size left over {s.ToString()}" + $" UserCombinerCount {mat.Flags.UserCombinerCount} DetailedCombinerCount {mat.Flags.UseDetailedCombiner} TevCombinerCount {mat.Flags.TevCombinerCount}");

            return mat;
        }
        
        #endregion

        #region PIC1

        public static PicturePane ReadPicturePane(FileReader reader, BflytFile header)
        {
            PicturePane pane = new PicturePane();
            ReadPane(pane, reader, header);

            pane.ColorTopLeft = new Color(reader.ReadUInt32());
            pane.ColorTopRight = new Color(reader.ReadUInt32());
            pane.ColorBottomLeft = new Color(reader.ReadUInt32());
            pane.ColorBottomRight = new Color(reader.ReadUInt32());
            pane.MaterialIndex = reader.ReadUInt16();
            byte numUVs = reader.ReadByte();
            pane.IsShape = reader.ReadBoolean();

            pane.TexCoords = new TexCoord[numUVs];
            for (int i = 0; i < numUVs; i++)
            {
                pane.TexCoords[i] = new TexCoord()
                {
                    TopLeft     = reader.ReadVec2(),
                    TopRight    = reader.ReadVec2(),
                    BottomLeft  = reader.ReadVec2(),
                    BottomRight = reader.ReadVec2(),
                };
            }
            return pane;
        }

        #endregion

        #region PRT1

        public static PartsPane ReadPartPane(FileReader reader, BflytFile header)
        {
            long headerStart = reader.Position - 8;

            PartsPane pane = new PartsPane();
            ReadPane(pane, reader, header);

            uint num_properties = reader.ReadUInt32();
            pane.MagnifyX = reader.ReadSingle();
            pane.MagnifyY = reader.ReadSingle();
            for (int i = 0; i < num_properties; i++)
                pane.Properties.Add(ReadPartProperty(reader, header, headerStart));
            pane.LayoutFileName = reader.ReadZeroTerminatedString();

            return pane;
        }

        static PartsProperty ReadPartProperty(FileReader reader, BflytFile header, long headerStart)
        {
            PartsProperty prop = new PartsProperty();

            prop.Name = reader.ReadFixedString(0x18);
            prop.UsageFlag = reader.ReadByte();
            prop.BasicUsageFlag = reader.ReadByte();
            prop.MaterialUsageFlag = reader.ReadByte();
            prop.Reserved0 = reader.ReadByte();

            uint pane_offset = reader.ReadUInt32();
            uint user_data__offset = reader.ReadUInt32();
            uint pane_info_offset = reader.ReadUInt32();

            // todo usage flag
            prop.Flag1 = user_data__offset;

            var origin = reader.BaseStream.Position;

            if (pane_offset != 0)
            {
                reader.SeekBegin(headerStart + pane_offset);
                prop.Property = ReadPaneKind(reader, header);
            }
            if (user_data__offset != 0 && user_data__offset > 10)
            {
                reader.SeekBegin(headerStart + user_data__offset);
                //magic + size
                reader.ReadSignature("usd1");
                reader.ReadUInt32();

                prop.UserData = ReadUserData(reader, header);
            }



            if (pane_info_offset != 0)
            {
                reader.SeekBegin(headerStart + pane_info_offset);

                prop.BasicInfo = new PartsPaneBasicInfo();
                prop.BasicInfo.UserName = reader.ReadFixedString(0x8);
                prop.BasicInfo.Translate = reader.ReadVec3();
                prop.BasicInfo.Rotate = reader.ReadVec3();
                prop.BasicInfo.Scale = reader.ReadVec2();
                prop.BasicInfo.Alpha = reader.ReadByte();
                prop.BasicInfo.Reserved0 = reader.ReadByte();
                prop.BasicInfo.Reserved1 = reader.ReadByte();
                prop.BasicInfo.Reserved2 = reader.ReadByte();

                prop.BasicInfo.Reserved3 = reader.ReadUInt32();
                prop.BasicInfo.Reserved4 = reader.ReadUInt32();
            }

            reader.SeekBegin(origin);

            return prop;
        }


        public static Pane ReadPaneKind(FileReader reader, BflytFile header)
        {
            //start of section
            var pos = reader.BaseStream.Position;

            //section magic and size
            string signature = reader.ReadString(4, Encoding.ASCII);
            uint sectionSize = reader.ReadUInt32();

            switch (signature)
            {
                case "pan1": return ReadPane(reader, header);
                case "pic1": return ReadPicturePane(reader, header);
                case "wnd1": return ReadWindowPane(reader, header);
                case "txt1": return ReadTextPane(reader, header);
                case "bnd1": return ReadBoundsPane(reader, header);
                case "scr1": return ReadScissorPane(reader, header);
                case "ali1": return ReadAlignmentPane(reader, header);
                case "prt1": return ReadPartPane(reader, header);
                default:
                    throw new Exception($"Unsupported pane kind for part pane {signature}!");
            }
        }

        #endregion

        #region USD1

        public static UserData ReadUserData(FileReader reader, BflytFile header)
        {
            UserData usd = new UserData();

            long headerStart = reader.Position - 8;

            ushort num_userdata = reader.ReadUInt16();
            reader.ReadUInt16(); //padding

            for (int i = 0; i < num_userdata; i++)
            {
                usd.Entries.Add(ReadUserDataEntry(reader, header, headerStart));

                if (usd.Entries[i].Type == UserDataType.SystemData)
                    break;
            }

            if (usd.Entries.Any(x => x.Type == UserDataType.SystemData))
            {
                //read as raw
                reader.SeekBegin(headerStart + 4);
                var size = reader.ReadUInt32();
                usd.Raw = reader.ReadBytes((int)size - 8);
            }

            return usd;
        }

        static UserData.UserDataEntry ReadUserDataEntry(FileReader reader, BflytFile header, long headerStart)
        {
            UserData.UserDataEntry entry = new UserData.UserDataEntry();

            long pos = reader.Position;

            uint nameOffset = reader.ReadUInt32();
            uint dataOffset = reader.ReadUInt32();
            entry.Length = reader.ReadUInt16();
            entry.Type = (UserDataType)reader.ReadByte();
            entry.Reserve0 = reader.ReadByte();

            long origin = reader.Position;

            if (nameOffset != 0)
            {
                reader.SeekBegin(pos + nameOffset);
                entry.Name = reader.ReadZeroTerminatedString();
            }

            if (dataOffset != 0)
            {
                reader.SeekBegin(pos + dataOffset);
                switch (entry.Type)
                {
                    case UserDataType.String:
                        entry._data = reader.ReadFixedString((int)entry.Length);
                        break;
                    case UserDataType.Int:
                        entry._data = reader.ReadInt32s((int)entry.Length);
                        break;
                    case UserDataType.Float:
                        entry._data = reader.ReadSingles((int)entry.Length);
                        break;
                    case UserDataType.SystemData:
                     //   var structs = new List<UserData.SystemData>();
                      //  for (int i = 0; i < entry.Length; i++)
                        //    structs.Add(new UserData.SystemData(reader, header));
                      //  entry._data = structs;
                        break;
                }
            }

            reader.SeekBegin(origin);

            return entry;
        }

        #endregion

        #region TXT1

        public static TextPane ReadTextPane(FileReader reader, BflytFile header)
        {
            long pos = reader.Position - 8;

            TextPane pane = new TextPane();
            ReadPane(pane, reader, header);

            pane.TextLength = reader.ReadUInt16();
            pane.MaxTextLength = reader.ReadUInt16();
            pane.MaterialIndex = reader.ReadUInt16();
            pane.FontIndex = reader.ReadUInt16();
            pane.TextAlignment = reader.ReadByte();
            pane.LineAlignment = (LineAlign)reader.ReadByte();
            pane._flags = reader.ReadByte();
            pane.Unknown3 = reader.ReadByte();
            pane.ItalicTilt = reader.ReadSingle();
            uint textOffset = reader.ReadUInt32();
            pane.FontTopColor = new Color(reader.ReadUInt32());
            pane.FontBottomColor = new Color(reader.ReadUInt32());
            pane.FontSize = reader.ReadVec2();
            pane.CharacterSpace = reader.ReadSingle();
            pane.LineSpace = reader.ReadSingle();
            uint nameOffset = reader.ReadUInt32();
            pane.ShadowXY = reader.ReadVec2();
            pane.ShadowXYSize = reader.ReadVec2();
            pane.ShadowForeColor = new Color(reader.ReadUInt32());
            pane.ShadowBackColor = new Color(reader.ReadUInt32());
            pane.ShadowItalic = reader.ReadSingle();

            uint lineTransformOffset = 0;
            if (header.VersionMajor >= 8)
                lineTransformOffset = reader.ReadUInt32();

            uint perCharTransformOffset = 0;
            if (header.VersionMajor > 3)
                perCharTransformOffset = reader.ReadUInt32();

            if (textOffset != 0 && pane.TextLength > 0)
            {
                reader.SeekBegin(pos + textOffset);
                pane.Text = reader.ReadString(Syroot.BinaryData.BinaryStringFormat.ZeroTerminated,
                       Encoding.Unicode);
            }

            if (nameOffset != 0)
            {
                reader.SeekBegin(pos + nameOffset);
                pane.TextBoxName = reader.ReadZeroTerminatedString();
            }

            if (pane.PerCharTransformEnabled && perCharTransformOffset != 0)
            {
                reader.SeekBegin(pos + perCharTransformOffset);
                var transform = new PerCharacterTransform();
                transform.CurveTimeOffset = reader.ReadSingle();
                transform.CurveWidth = reader.ReadSingle();
                transform.LoopType = reader.ReadByte();
                transform.VerticalOrigin = reader.ReadByte();
                transform.HasAnimInfo = reader.ReadByte();
                transform.padding = reader.ReadByte();
                pane.PerCharacterTransform = transform;

                transform.CharList = reader.ReadBytes(20);

                if (transform.HasAnimInfo != 0)
                {
                    transform.AnimationInfo = new AnimationInfo();
                    transform.AnimationInfo.Read(reader);

                    if (transform.AnimationInfo.Kind != "FLCC")
                    {

                    }
                }
            }

            return pane;
        }

        #endregion

        #region BND1

        public static BoundsPane ReadBoundsPane(FileReader reader, BflytFile header)
        {
            BoundsPane pane = new BoundsPane();
            ReadPane(pane, reader, header);
            return pane;
        }

        #endregion

        #region SCR1

        public static ScissorPane ReadScissorPane(FileReader reader, BflytFile header)
        {
            ScissorPane pane = new ScissorPane();
            ReadPane(pane, reader, header);
            return pane;
        }

        #endregion

        #region ALI1

        public static AlignmentPane ReadAlignmentPane(FileReader reader, BflytFile header)
        {
            AlignmentPane pane = new AlignmentPane();
            ReadPane(pane, reader, header);
            pane.AlignmentValue = reader.ReadVec3();
            return pane;
        }

        #endregion

        #region CTL1
        public static CaptureTextureLayer ReadCaptureTextureLayer(FileReader reader, BflytFile header)
        {
            long pos = reader.Position - 8;

            CaptureTextureLayer ctl = new CaptureTextureLayer();

            reader.Seek(pos + 4, SeekOrigin.Begin);
            var size = reader.ReadUInt32();

            ctl.Raw = reader.ReadBytes((int)size - 8);

            return ctl;
        }
        #endregion

        #region CNT1

        public static ControlSource ReadControlSource(FileReader reader, BflytFile header)
        {
            long pos = reader.Position - 8;

            ControlSource cnt = new ControlSource();
            
            uint controlNameOffset = reader.ReadUInt32();
            uint paneNameOffset = reader.ReadUInt32();
            ushort paneCount = reader.ReadUInt16();
            ushort animCount = reader.ReadUInt16();
            uint paneArrayOffset = reader.ReadUInt32();
            uint animArrayOffset = reader.ReadUInt32();

            reader.SeekBegin(pos + 28);
            cnt.Name        = reader.ReadZeroTerminatedString();

            reader.SeekBegin(pos + controlNameOffset);
            cnt.ControlName = reader.ReadZeroTerminatedString();

            reader.SeekBegin(pos + paneNameOffset);
            for (int i = 0; i < paneCount; i++)
                cnt.Panes.Add(reader.ReadFixedString(24));

            cnt.AnimationStates = ReadStringOffsets(reader, (int)animCount);

            reader.SeekBegin(pos + paneArrayOffset);
            cnt.PaneStates = ReadStringOffsets(reader, (int)paneCount);

            reader.SeekBegin(pos + animArrayOffset);
            cnt.Animations = ReadStringOffsets(reader, (int)animCount);

            return cnt;
        }

        #endregion

        #region WND1

        public static WindowPane ReadWindowPane(FileReader reader, BflytFile header)
        {
            long pos = reader.Position - 8;

            WindowPane pane = new WindowPane();
            ReadPane(pane, reader, header);

            pane.StretchLeft = reader.ReadUInt16();
            pane.StretchRight = reader.ReadUInt16();
            pane.StretchTop = reader.ReadUInt16();
            pane.StretchBottm = reader.ReadUInt16();
            pane.FrameElementLeft = reader.ReadUInt16();
            pane.FrameElementRight = reader.ReadUInt16();
            pane.FrameElementTop = reader.ReadUInt16();
            pane.FrameElementBottm = reader.ReadUInt16();
            var frameCount = reader.ReadByte();
            pane.Flag = reader.ReadByte();
            reader.ReadUInt16();//padding
            uint contentOffset = reader.ReadUInt32();
            uint frameOffsetTbl = reader.ReadUInt32();

            pane.WindowKind = (WindowKind)((pane.Flag >> 2) & 3);

            reader.SeekBegin(pos + contentOffset);
            pane.Content = ReadWindowContent(reader);

            reader.SeekBegin(pos + frameOffsetTbl);
            var offsets = reader.ReadUInt32s(frameCount);
            foreach (int offset in offsets)
            {
                reader.SeekBegin(pos + offset);
                pane.WindowFrames.Add(new WindowFrame()
                {
                    MaterialIndex = reader.ReadUInt16(),
                    TextureFlip = (WindowFrameTexFlip)reader.ReadByte(),
                });
                reader.ReadByte(); //padding
            }
            return pane;
        }

        static WindowContent ReadWindowContent(FileReader reader)
        {
            WindowContent cnt = new WindowContent();

            cnt.ColorTopLeft = new Color(reader.ReadUInt32());
            cnt.ColorTopRight = new Color(reader.ReadUInt32());
            cnt.ColorBottomLeft = new Color(reader.ReadUInt32());
            cnt.ColorBottomRight = new Color(reader.ReadUInt32());
            cnt.MaterialIndex = reader.ReadUInt16();
            byte UVCount = reader.ReadByte();
            reader.ReadByte(); //padding

            for (int i = 0; i < UVCount; i++)
                cnt.TexCoords.Add(new TexCoord()
                {
                    TopLeft     = reader.ReadVec2(),
                    TopRight    = reader.ReadVec2(),
                    BottomLeft  = reader.ReadVec2(),
                    BottomRight = reader.ReadVec2(),
                });
            return cnt;
        }

        #endregion
    }
}
