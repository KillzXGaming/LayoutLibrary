using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LayoutLibrary.Cafe
{
    public class BflytWriter
    {
        #region Section Writer

        public static void WriteStringSection(FileWriter writer, List<string> values, BflytFile header)
        {
            writer.Write((ushort)values.Count);
            writer.Write((ushort)0);

            //Fill empty spaces for offsets later
            long pos = writer.Position;
            writer.Write(new uint[values.Count]);

            //Save offsets and strings
            for (int i = 0; i < values.Count; i++)
            {
                writer.WriteUint32Offset(pos + (i * 4), (int)pos);
                writer.WriteStringZeroTerminated(values[i]);
            }
            writer.AlignBytes(4);
        }

        public static void WriteStringOffsets(FileWriter writer, List<string> values)
        {
            //Fill empty spaces for offsets later
            long pos = writer.Position;
            writer.Write(new uint[values.Count]);

            //Save offsets and strings
            for (int i = 0; i < values.Count; i++)
            {
                writer.WriteUint32Offset(pos + (i * 4), (int)pos);
                writer.WriteStringZeroTerminated(values[i]);
            }
            writer.AlignBytes(4);
        }

        #endregion

        #region LYT1

        public static void WriteLayout(Layout layout, FileWriter writer, BflytFile header)
        {
            writer.Write(layout.DrawFromCenter);
            writer.Seek(3);
            writer.Write(layout.Width);
            writer.Write(layout.Height);
            writer.Write(layout.MaxPartsWidth);
            writer.Write(layout.MaxPartsHeight);
            writer.WriteStringZeroTerminated(layout.Name);
            writer.AlignBytes(4);
        }

        #endregion

        #region GRP1

        public static void WriteGroup(Group grp, FileWriter writer, BflytFile header)
        {
            if (header.VersionMajor >= 5)
            {
                writer.WriteFixedString(grp.Name, 0x21);
                writer.Write((byte)0);
                writer.Write((ushort)grp.Panes.Count);
            }
            else
            {
                writer.WriteFixedString(grp.Name, 0x18);
                writer.Write((ushort)grp.Panes.Count);
                writer.Write((ushort)0);
            }
            for (int i = 0; i < grp.Panes.Count; i++)
                writer.WriteFixedString(grp.Panes[i], 0x18);
        }

        #endregion

        #region PAN1

        public static void WritePane(Pane pane, FileWriter writer, BflytFile header)
        {
            int originL = (int)pane.OriginX;
            int originH = (int)pane.OriginY * 4;
            int originPL = (int)pane.ParentOriginX;
            int originPH = (int)pane.ParentOriginY * 4;
            byte parentOrigin = (byte)((originPL + originPH) * 16);
            byte origin = (byte)(originL + originH + parentOrigin);

            writer.Write(pane.Flags1);
            writer.Write(origin);
            writer.Write(pane.Alpha);
            writer.Write(pane.PaneMagFlags);
            writer.WriteFixedString(pane.Name, 0x18);
            writer.WriteFixedString(pane.UserDataInfo, 0x8);
            writer.Write(pane.Translate);
            writer.Write(pane.Rotate);
            writer.Write(pane.Scale);
            writer.Write(pane.Width);
            writer.Write(pane.Height);
        }

        #endregion

        #region MAT1

        public static void WriteMaterialTable(MaterialTable table, FileWriter writer, BflytFile header)
        {
            long pos = writer.Position - 8;

            writer.Write((ushort)table.Materials.Count);
            writer.Write((ushort)0);

            //offset allocate
            writer.Write(new uint[table.Materials.Count]);

            for (int i = 0; i < table.Materials.Count; i++)
            {
                writer.WriteUint32Offset(pos + 12 + i * 4, (int)pos);
                WriteMaterial(table.Materials[i], writer, header);
            }
        }

        static void WriteMaterial(Material mat, FileWriter writer, BflytFile header)
        {
            writer.WriteFixedString(mat.Name, 0x1C);

            if (header.VersionMajor >= 8)
            {
                writer.Write(mat.Flags.GetFlags());

                var cpos = writer.Position;
                writer.Write((byte)mat.ColorType);
                writer.Write((byte)mat.Colors.Count);
                writer.Write(new byte[mat.Colors.Count]);

                for (int i = 0; i < mat.Colors.Count; i++)
                {
                    writer.WriteByteOffset(cpos + 2 + (i), (int)cpos);
                        
                    var type = mat.ColorType >> i;
                    if (type == 0)
                        writer.Write(mat.Colors[i].ToUInt32());
                    else
                    {
                        writer.Write(mat.Colors[i].R);
                        writer.Write(mat.Colors[i].G);
                        writer.Write(mat.Colors[i].B);
                        writer.Write(mat.Colors[i].A);
                    }
                }
            }
            else
            {
                writer.Write(mat.BlackColor.ToUInt32());
                writer.Write(mat.WhiteColor.ToUInt32());
                writer.Write(mat.Flags.GetFlags());
            }

            mat.Flags.TexMapCount = (byte)mat.Textures.Count;
            mat.Flags.TexSrtCount = (byte)mat.TextureSrts.Count;
            mat.Flags.TexCoordGenCount = (byte)mat.TexCoordGens.Count;
            mat.Flags.TevCombinerCount = (byte)mat.TevCombiners.Count;
            mat.Flags.AlphaCompareCount = (byte)mat.AlphaCompares.Count;
            mat.Flags.ProjectionTexGenCount = (byte)mat.ProjectionTexGens.Count;
            mat.Flags.UseDetailedCombiner = mat.DetailedCombiners.Count > 0;
            mat.Flags.TevCombinerCount = (byte)(mat.DetailedCombiners.Count + mat.TevCombiners.Count);

            for (int i = 0; i < mat.Textures.Count; i++)
            {
                writer.Write(mat.Textures[i].TextureIndex);
                writer.Write(mat.Textures[i].Flags);
            }
            
            if (mat.Flags.HasTextureExtensions)
                writer.Write(mat.TextureExtensions[0].Flags);

            for (int i = 0; i < mat.TextureSrts.Count; i++)
            {
                writer.Write(mat.TextureSrts[i].Translate);
                writer.Write(mat.TextureSrts[i].Rotate);
                writer.Write(mat.TextureSrts[i].Scale);
            }

            for (int i = 0; i < mat.TexCoordGens.Count; i++)
            {
                writer.Write((byte)mat.TexCoordGens[i].MatrixType);
                writer.Write((byte)mat.TexCoordGens[i].Source);
                writer.Write((ushort)mat.TexCoordGens[i].Unknown);
                writer.Write(mat.TexCoordGens[i].Unknown2);
                writer.Write(mat.TexCoordGens[i].Unknown3);
            }

            if (mat.DetailedCombiners.Count > 0)
            {
                writer.Write(mat.DetailedCombinerData);
                for (int i = 0; i < mat.DetailedCombiners.Count; i++)
                {
                    writer.Write(mat.DetailedCombiners[i].Data);
                }
            }
            else
            {
                for (int i = 0; i < mat.TevCombiners.Count; i++)
                {
                    writer.Write((byte)mat.TevCombiners[i].Data);
                    writer.Write((byte)mat.TevCombiners[i].Reserved0);
                    writer.Write((byte)mat.TevCombiners[i].Reserved1);
                    writer.Write((byte)mat.TevCombiners[i].Reserved2);
                }
            }

            for (int i = 0; i < mat.UserCombiners.Count; i++)
            {
                writer.Write(mat.UserCombiners[i].Data);
            }

            for (int i = 0; i < mat.AlphaCompares.Count; i++)
            {
                writer.Write((byte)mat.AlphaCompares[i].CompareMode);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write(mat.AlphaCompares[i].Value);
            }

            if (mat.Flags.ColorBlendMode)
            {
                writer.Write((byte)mat.ColorBlend.BlendOp);
                writer.Write((byte)mat.ColorBlend.SourceFactor);
                writer.Write((byte)mat.ColorBlend.DestFactor);
                writer.Write((byte)mat.ColorBlend.LogicOp);
            }
            if (mat.Flags.ColorAndAlphaBlendMode)
            {
                writer.Write((byte)mat.AlphaBlend.BlendOp);
                writer.Write((byte)mat.AlphaBlend.SourceFactor);
                writer.Write((byte)mat.AlphaBlend.DestFactor);
                writer.Write((byte)mat.AlphaBlend.LogicOp);
            }
            if (mat.Flags.EnableIndirectParams)
            {
                writer.Write(mat.IndirectParameter.Rotation);
                writer.Write(mat.IndirectParameter.Scale);
            }

            for (int i = 0; i < mat.ProjectionTexGens.Count; i++)
            {
                writer.Write(mat.ProjectionTexGens[i].Position);
                writer.Write(mat.ProjectionTexGens[i].Scale);
                writer.Write(mat.ProjectionTexGens[i].Flags);
            }

            if (mat.Flags.EnableFontShadowParams)
            {
                writer.Write(mat.FontShadowParameter.BlackColor.ToUInt32());
                writer.Write(mat.FontShadowParameter.WhiteColor.ToUInt32());
            }

            for (int i = 0; i < mat.BrickRepeatShaderInfos.Count; i++)
            {
                writer.Write(mat.BrickRepeatShaderInfos[i].Data);
            }
        }

        #endregion

        #region PIC1

        static void WritePicturePane(PicturePane pane, FileWriter writer, BflytFile header)
        {
            WritePane(pane, writer, header);
            writer.Write(pane.ColorTopLeft.ToUInt32());
            writer.Write(pane.ColorTopRight.ToUInt32());
            writer.Write(pane.ColorBottomLeft.ToUInt32());
            writer.Write(pane.ColorBottomRight.ToUInt32());
            writer.Write((ushort)pane.MaterialIndex);
            writer.Write((byte)pane.TexCoords.Length);
            writer.Write((byte)(pane.IsShape ? 1 : 0));
            for (int i = 0; i < pane.TexCoords.Length; i++)
            {
                writer.Write(pane.TexCoords[i].TopLeft);
                writer.Write(pane.TexCoords[i].TopRight);
                writer.Write(pane.TexCoords[i].BottomLeft);
                writer.Write(pane.TexCoords[i].BottomRight);
            }
        }

        #endregion

        #region CTL1
        public static void WriteCaptureTextureLayer(CaptureTextureLayer ctl1, FileWriter writer, BflytFile header)
        {
            writer.Write(ctl1.Raw);
        }
        #endregion

        #region CNT1

        public static void WriteControlSource(ControlSource cnt1, FileWriter writer, BflytFile header)
        {
            long pos = writer.Position - 8;

            ControlSource cnt = new ControlSource();
            writer.Write(40);
            writer.Write(0); //pane name offset later
            writer.Write((ushort)cnt1.Panes.Count);
            writer.Write((ushort)cnt1.Animations.Count);
            writer.Write(0); //pane array offset later
            writer.Write(0); //anim array offset later

            writer.WriteStringZeroTerminated(cnt1.Name);
            writer.AlignBytes(4);

            writer.WriteUint32Offset(pos + 8, (int)pos);
            writer.WriteStringZeroTerminated(cnt1.ControlName);
            writer.AlignBytes(4);

            writer.WriteUint32Offset(pos + 8 + 4, (int)pos);
            foreach (var pane in cnt1.Panes)
                writer.WriteFixedString(pane, 24);

            WriteStringOffsets(writer, cnt1.AnimationStates);

            writer.WriteUint32Offset(pos + 8 + 12, (int)pos);
            WriteStringOffsets(writer, cnt1.PaneStates);

            writer.WriteUint32Offset(pos + 8 + 16, (int)pos);
            WriteStringOffsets(writer, cnt1.Animations);

            writer.AlignBytes(4);
        }

        #endregion

        #region PRT1

        static void WritePartPane(PartsPane pane, FileWriter writer, BflytFile header)
        {
            long pos = writer.Position - 8;

            WritePane(pane, writer, header);
            writer.Write(pane.Properties.Count);
            writer.Write(pane.MagnifyX);
            writer.Write(pane.MagnifyY);

            long[] offsets_start = new long[pane.Properties.Count];
            for (int i = 0; i < pane.Properties.Count; i++)
            {
                writer.WriteFixedString(pane.Properties[i].Name, 0x18);
                writer.Write(pane.Properties[i].UsageFlag);
                writer.Write(pane.Properties[i].BasicUsageFlag);
                writer.Write(pane.Properties[i].MaterialUsageFlag);
                writer.Write(pane.Properties[i].Reserved0);

                offsets_start[i] = writer.Position;

                writer.Write(0); //pane_offset
                writer.Write(pane.Properties[i].Flag1); //user_data__offset or flag by usage
                writer.Write(0); //pane_info_offset
            }
            writer.Align(4);

            writer.WriteStringZeroTerminated(pane.LayoutFileName);
            writer.Align(4);

            // Write sections next
            for (int i = 0; i < pane.Properties.Count; i++)
            {
                if (pane.Properties[i].Property != null)
                {
                    writer.WriteUint32Offset(offsets_start[i], (int)pos);

                    var o = writer.Position;
                    writer.Write(Encoding.ASCII.GetBytes(pane.Properties[i].Property.Magic));
                    writer.Write(0); //size later
                    WritePaneKind(pane.Properties[i].Property, writer, header);

                    writer.Align(4);

                    var size = writer.Position - o;
                    using (writer.TemporarySeek(o + 4, SeekOrigin.Begin)) {
                        writer.Write((uint)size);
                    }
                }
                if (pane.Properties[i].UserData != null)
                {
                    writer.WriteUint32Offset(offsets_start[i] + 4, (int)pos);

                    var o = writer.Position;
                    writer.Write(Encoding.ASCII.GetBytes("usd1"));
                    writer.Write(0); //size later
                    WriteUserData(pane.Properties[i].UserData, writer, header);

                    writer.Align(4);

                    var size = writer.Position - o;
                    using (writer.TemporarySeek(o + 4, SeekOrigin.Begin)) {
                        writer.Write((uint)size);
                    }
                }
                if (pane.Properties[i].BasicInfo != null)
                {
                    writer.WriteUint32Offset(offsets_start[i] + 8, (int)pos);
                    writer.WriteFixedString(pane.Properties[i].BasicInfo.UserName, 0x8);
                    writer.Write(pane.Properties[i].BasicInfo.Translate);
                    writer.Write(pane.Properties[i].BasicInfo.Rotate);
                    writer.Write(pane.Properties[i].BasicInfo.Scale);
                    writer.Write((byte)pane.Properties[i].BasicInfo.Alpha);
                    writer.Write((byte)pane.Properties[i].BasicInfo.Reserved0);
                    writer.Write((byte)pane.Properties[i].BasicInfo.Reserved1);
                    writer.Write((byte)pane.Properties[i].BasicInfo.Reserved2);
                    writer.Write(pane.Properties[i].BasicInfo.Reserved3);
                    writer.Write(pane.Properties[i].BasicInfo.Reserved4);
                }
            }
        }


        public static void WritePaneKind(Pane pane, FileWriter writer, BflytFile header)
        {
            if (pane is PicturePane) 
                WritePicturePane((PicturePane)pane, writer, header);
            else if (pane is TextPane)
                WriteTextPane((TextPane)pane, writer, header);
            else if (pane is PartsPane) 
                WritePartPane((PartsPane)pane, writer, header);
            else if (pane is WindowPane)
                WriteWindowPane((WindowPane)pane, writer, header);
            else if (pane is ScissorPane)
                WriteScissorPane((ScissorPane)pane, writer, header);
            else if (pane is AlignmentPane)
                WriteAlignmentPane((AlignmentPane)pane, writer, header);
            else if (pane is BoundsPane)
                WriteBoundsPane((BoundsPane)pane, writer, header);
            else 
                WritePane((Pane)pane, writer, header);
        }

        #endregion

        #region USD1

        public static void WriteUserData(UserData usd, FileWriter writer, BflytFile header)
        {
            if (usd.Raw != null)
            {
                writer.Write(usd.Raw);
                return;
            }

            long headerStart = writer.Position;

            writer.Write((ushort)usd.Entries.Count);
            writer.Write((ushort)0);
            for (int i = 0; i < usd.Entries.Count; i++)
            {
                usd.Entries[i]._pos = writer.Position;

                writer.Write(0); //nameOffset
                writer.Write(0); //dataOffset
                writer.Write((ushort)usd.Entries[i].GetDataLength());
                writer.Write((byte)usd.Entries[i].Type);
                writer.Write((byte)usd.Entries[i].Reserve0);
            }

            var data_start = writer.Position;

            for (int i = 0; i < usd.Entries.Count; i++)
            {
                if (usd.Entries[i].Type == UserDataType.String)
                    continue;

                writer.WriteUint32Offset(usd.Entries[i]._pos + 4, (int)usd.Entries[i]._pos);
                switch (usd.Entries[i].Type)
                {
                    case UserDataType.Int:
                        writer.Write(usd.Entries[i].GetInts());
                        break;
                    case UserDataType.Float:
                        writer.Write(usd.Entries[i].GetFloats());
                        break;
                    case UserDataType.SystemData:
                        foreach (var structure in usd.Entries[i].GetStructs())
                            writer.Write(structure.Write(header));
                        break;
                }
            }

            // Build string table
            Dictionary<string, List<(long, long)>> table = new Dictionary<string, List<(long, long)>>();
            for (int i = 0; i < usd.Entries.Count; i++)
            {
                var ofs_start = usd.Entries[i]._pos;

                void AddEntry(string value, long target)
                {
                    if (!table.ContainsKey(value))
                        table.Add(value, new List<(long, long)>());

                    table[value].Add((target, ofs_start));
                }

                if (usd.Entries[i].Type == UserDataType.String) {
                    AddEntry(usd.Entries[i].GetString(), ofs_start + 4);
                }
                AddEntry(usd.Entries[i].Name, ofs_start);
            }

            // Write string table
            foreach (var str in table)
            {
                foreach (var v in str.Value)
                    writer.WriteUint32Offset(v.Item1, (int)v.Item2);
                writer.WriteStringZeroTerminated(str.Key);
            }
        }

        #endregion

        #region TXT1

        #endregion

        public static void WriteTextPane(TextPane pane, FileWriter writer, BflytFile header)
        {
            long pos = writer.Position - 8;

            WritePane(pane, writer, header);

            writer.Write((ushort)pane.TextLength);
            writer.Write((ushort)pane.MaxTextLength);
            writer.Write((ushort)pane.MaterialIndex);
            writer.Write((ushort)pane.FontIndex);
            writer.Write((byte)pane.TextAlignment);
            writer.Write((byte)pane.LineAlignment);
            writer.Write((byte)pane._flags);
            writer.Write((byte)pane.Unknown3);
            writer.Write(pane.ItalicTilt);

            var textOffset = (int)writer.Position;
            writer.Write(0);
            writer.Write(pane.FontTopColor.ToUInt32());
            writer.Write(pane.FontBottomColor.ToUInt32());
            writer.Write(pane.FontSize);
            writer.Write(pane.CharacterSpace);
            writer.Write(pane.LineSpace);

            var nameOffset = (int)writer.Position; 
            writer.Write(0);

            writer.Write(pane.ShadowXY);
            writer.Write(pane.ShadowXYSize);
            writer.Write(pane.ShadowForeColor.ToUInt32());
            writer.Write(pane.ShadowBackColor.ToUInt32());
            writer.Write(pane.ShadowItalic);

            var lineTransformOffset = (int)writer.Position;
            if (header.VersionMajor >= 8)
                writer.Write(0);

            var perCharTransformOffset = (int)writer.Position;
            if (header.VersionMajor > 3)
                writer.Write(0);

            writer.Align(4);

            writer.WriteUint32Offset(textOffset, (int)pos);
            if (!string.IsNullOrEmpty(pane.Text))
            {
                if (writer.ByteOrder == Syroot.BinaryData.ByteOrder.BigEndian)
                    writer.Write(Encoding.BigEndianUnicode.GetBytes(pane.Text));
                else
                    writer.Write(Encoding.Unicode.GetBytes(pane.Text));
                writer.Write((byte)0);
                writer.Align(4);
            }

            if (!string.IsNullOrEmpty(pane.TextBoxName))
            {
                writer.WriteUint32Offset(nameOffset, (int)pos);
                writer.WriteStringZeroTerminated(pane.TextBoxName);
                writer.Align(4);
            }

            if (pane.PerCharacterTransform != null)
            {
                writer.WriteUint32Offset(perCharTransformOffset, (int)pos);
                var val = pane.PerCharacterTransform;

                writer.Write(val.CurveTimeOffset);
                writer.Write(val.CurveWidth);
                writer.Write((byte)val.LoopType);
                writer.Write((byte)val.VerticalOrigin);
                writer.Write((byte)val.HasAnimInfo);
                writer.Write((byte)val.padding);

                writer.Write(val.CharList);

                writer.Align(4);

                if (val.HasAnimInfo != 0)
                    val.AnimationInfo.Write(writer);
            }
        }

        #region BND1

        public static void WriteBoundsPane(BoundsPane pane, FileWriter writer, BflytFile header) {
            WritePane(pane, writer, header);
        }

        #endregion

        #region SCR1

        public static void WriteScissorPane(ScissorPane pane, FileWriter writer, BflytFile header) {
            WritePane(pane, writer, header);
        }

        #endregion

        #region ALI1

        public static void WriteAlignmentPane(AlignmentPane pane, FileWriter writer, BflytFile header) {
            WritePane(pane, writer, header);
            writer.Write(pane.AlignmentValue);
        }

        #endregion

        #region WND1

        public static WindowPane WriteWindowPane(WindowPane pane, FileWriter writer, BflytFile header)
        {
            pane.Flag = (byte)((pane.Flag & ~0x0C) | ((int)pane.WindowKind << 2));

            long pos = writer.Position - 8;

            WritePane(pane, writer, header);

            writer.Write((ushort)pane.StretchLeft);
            writer.Write((ushort)pane.StretchRight);
            writer.Write((ushort)pane.StretchTop);
            writer.Write((ushort)pane.StretchBottm);
            writer.Write((ushort)pane.FrameElementLeft);
            writer.Write((ushort)pane.FrameElementRight);
            writer.Write((ushort)pane.FrameElementTop);
            writer.Write((ushort)pane.FrameElementBottm);
            writer.Write((byte)pane.WindowFrames.Count);
            writer.Write((byte)pane.Flag);
            writer.Write((ushort)0); // padding

            int contentOffset = (int)writer.Position;
            writer.Write(0);

            int frameOffsetTbl = (int)writer.Position; 
            writer.Write(0);

            writer.WriteUint32Offset(contentOffset, (int)pos);
            WriteWindowContent(pane.Content, writer);

            writer.WriteUint32Offset(frameOffsetTbl, (int)pos);

            var frameStart = writer.Position;
            writer.Write(new uint[pane.WindowFrames.Count]);

            for (int i = 0; i < pane.WindowFrames.Count; i++)
            {
                writer.WriteUint32Offset(frameStart + i * 4, (int)pos);
                writer.Write((ushort)pane.WindowFrames[i].MaterialIndex);
                writer.Write((byte)pane.WindowFrames[i].TextureFlip);
                writer.Write((byte)0); //padding
            }
            return pane;
        }

        static void WriteWindowContent(WindowContent cnt, FileWriter writer)
        {
            writer.Write(cnt.ColorTopLeft.ToUInt32());
            writer.Write(cnt.ColorTopRight.ToUInt32());
            writer.Write(cnt.ColorBottomLeft.ToUInt32());
            writer.Write(cnt.ColorBottomRight.ToUInt32());
            writer.Write((ushort)cnt.MaterialIndex);
            writer.Write((byte)cnt.TexCoords.Count);
            writer.Write((byte)0);

            for (int i = 0; i < cnt.TexCoords.Count; i++)
            {
                writer.Write(cnt.TexCoords[i].TopLeft);
                writer.Write(cnt.TexCoords[i].TopRight);
                writer.Write(cnt.TexCoords[i].BottomLeft);
                writer.Write(cnt.TexCoords[i].BottomRight);
            }
        }

        #endregion
    }
}
