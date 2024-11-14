using System.Numerics;
using System.Text;

namespace LayoutLibrary.Cafe
{
    /// <summary>
    /// A UI layout format used for reading and writing BFLYT file binaries.
    /// </summary>
    public class BflytFile
    {
        [Newtonsoft.Json.JsonIgnore]
        private const string Magic = "FLYT";

         //The full version number
        private uint Version;

        /// <summary>
        /// Version Major.
        /// </summary>
        public byte VersionMajor;

        /// <summary>
        /// Version Minor.
        /// </summary>
        public byte VersionMinor;

        /// <summary>
        /// Version Micro.
        /// </summary>
        public ushort VersionMicro;

        /// <summary>
        /// The byte order.
        /// </summary>
        public ushort ByteOrderMark;

        // header size
        private ushort HeaderSize;

        /// <summary>
        /// The layout information.
        /// </summary>
        public Layout Layout;

        /// <summary>
        /// A list of texture file references
        /// </summary>
        public List<string> TextureList = new List<string>();

        /// <summary>
        /// A list of font file references
        /// </summary>
        public List<string> FontList = new List<string>();

        /// <summary>
        /// A list of user data,
        /// </summary>
        public List<UserData> UserData = new List<UserData>();

        /// <summary>
        /// A table for storing material data.
        /// </summary>
        public MaterialTable MaterialTable;

        /// <summary>
        /// The root pane.
        /// </summary>
        public Pane Root;

        /// <summary>
        /// The root group.
        /// </summary>
        public Group RootGroup;

        /// <summary>
        /// Capture Texture Layer
        /// </summary>
        public CaptureTextureLayer CaptureTextureLayer;

        /// <summary>
        /// Control Source
        /// </summary>
        public ControlSource ControlSource;

        //Unsupported sections
        private List<UnsupportedSection> UnsupportedSections = new List<UnsupportedSection>();

        public BflytFile(string path)
        {
            Read(new FileReader(File.OpenRead(path)));
        }

        public BflytFile(Stream stream)
        {
            Read(new FileReader(stream));
        }

        public void Save(string path)
        {
            using (var writer = new FileWriter(path)) {
                Write(writer);
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream)) {
                Write(writer);
            }
        }

        private void Read(FileReader reader)
        {
            reader.SetByteOrder(true);

            reader.ReadSignature(Magic);
            ByteOrderMark = reader.ReadUInt16();
            reader.CheckByteOrderMark(ByteOrderMark);

            HeaderSize = reader.ReadUInt16();
            Version = reader.ReadUInt32();
            uint FileSize = reader.ReadUInt32();
            ushort sectionCount = reader.ReadUInt16();
            ushort Padding = reader.ReadUInt16();

            //Version set
            VersionMajor = (byte)(Version >> 24);
            VersionMinor = (byte)(Version >> 16 & 0xFF);
            VersionMicro = (ushort)(Version & 0xFFFF);

            reader.SeekBegin(HeaderSize);
            ReadSections(reader, sectionCount);
        }

        private void Write(FileWriter writer)
        {
            writer.SetByteOrder(true);

            writer.WriteSignature(Magic);
            writer.Write(ByteOrderMark);
            writer.CheckByteOrderMark(ByteOrderMark);
            writer.Write(HeaderSize);
            writer.Write(Version);
            var _ofsFileSize = writer.Position;
            writer.Write(uint.MaxValue);
            var _ofsBlockNum = writer.Position;
            writer.Write((ushort)0); //BlockCount
            writer.Write((ushort)0);

            writer.SeekBegin(HeaderSize);
            var numSections = 0;

            WriteSections(writer, ref numSections);

            //Save Block Count
            using (writer.TemporarySeek(_ofsBlockNum, SeekOrigin.Begin)) {
                writer.Write((ushort)numSections);
            }

            //Save File size
            using (writer.TemporarySeek(_ofsFileSize, SeekOrigin.Begin)) {
                writer.Write((uint)writer.BaseStream.Length);
            }
        }

        private void WriteSections(FileWriter writer, ref int numSections)
        {
            WriteSection(writer, "lyt1", () => BflytWriter.WriteLayout(Layout, writer, this), ref numSections);

            foreach (var usd in this.UserData) {
                WriteSection(writer, "usd1", () => BflytWriter.WriteUserData(usd, writer, this), ref numSections);
            }

            if (this.TextureList?.Count > 0)
                WriteSection(writer, "txl1", () => BflytWriter.WriteStringSection(writer, this.TextureList, this), ref numSections);
            if (this.FontList?.Count > 0)
                WriteSection(writer, "fnl1", () => BflytWriter.WriteStringSection(writer, this.FontList, this), ref numSections);
            if (this.MaterialTable != null)
                WriteSection(writer, "mat1", () => BflytWriter.WriteMaterialTable(this.MaterialTable, writer, this), ref numSections);

            if (this.CaptureTextureLayer != null)
            {
                WriteSection(writer, "ctl1", () => BflytWriter.WriteCaptureTextureLayer(
                    this.CaptureTextureLayer, writer, this), ref numSections);
            }

            if (this.Root != null)
                WritePanes(writer, this.Root, ref numSections);

            if (this.RootGroup != null)
                WriteGroups(writer, this.RootGroup, ref numSections);

            if (this.ControlSource != null)
            {
                WriteSection(writer, "cnt1", () => BflytWriter.WriteControlSource(this.ControlSource, writer, this), ref numSections);
                if (ControlSource.UserData != null)
                    WriteSection(writer, "usd1", () => BflytWriter.WriteUserData(ControlSource.UserData, writer, this), ref numSections);
            }

            foreach (var section in this.UnsupportedSections)
                WriteSection(writer, section.Magic, () => section.Write(writer, this), ref numSections);
        }

        private void WritePanes(FileWriter writer, Pane pane, ref int numSections)
        {
            WriteSection(writer, pane.Magic, () => BflytWriter.WritePaneKind(pane, writer, this), ref numSections);

            if (pane.UserData != null)
                WriteSection(writer, "usd1", () => BflytWriter.WriteUserData(pane.UserData, writer, this), ref numSections);

            if (pane.Children.Count > 0)
            {
                //Write start of children section
                WriteSection(writer, "pas1", null, ref numSections);

                foreach (var child in pane.Children)
                    WritePanes(writer, child, ref numSections);

                //Write pae1 of children section
                WriteSection(writer, "pae1", null, ref numSections);
            }
        }

        private void WriteGroups(FileWriter writer, Group grp, ref int numSections)
        {
            WriteSection(writer, "grp1", () => BflytWriter.WriteGroup(grp, writer, this), ref numSections);

            if (grp.Children.Count > 0)
            {
                //Write start of children section
                WriteSection(writer, "grs1", null, ref numSections);

                foreach (var child in grp.Children)
                    WriteGroups(writer, child, ref numSections);

                //Write pae1 of children section
                WriteSection(writer, "gre1", null, ref numSections);
            }
        }

        private void WriteSection(FileWriter writer, string magic, Action writeSection, ref int numSections)
        {
            var pos = writer.Position;
            writer.WriteSignature(magic);
            writer.Write(uint.MaxValue); //section size written later

            writeSection?.Invoke();

            writer.AlignBytes(4);

            var length = writer.Position - pos;
            using (writer.TemporarySeek(pos + 4, SeekOrigin.Begin))
            {
                writer.Write((uint)length);
            }
            numSections++;
        }

        private void ReadSections(FileReader reader, int sectionCount)
        {
            List<Pane> Panes = new List<Pane>();
            List<Group> Groups = new List<Group>();

            Pane currentPane = null;
            Pane parentPane = null;

            Group currentGroupPane = null;
            Group parentGroupPane = null;

            /*
                Old 8.0.0:
                lyt1 (layout)
                usd1 (user data)
                txl1 (texture)
                fnl1 (font)
                mat1 (material)
                pas1 (pane start)
                pan1 (null pane)
                prt1 (part pane)
                bnd1 (bounding pane)
                txt1 (text pane)
                pic1 (picture pane)
                wnd1 (window pane)
                pae1 (pane end)
                grs1 (group start)
                grp1 (group)
                gre1 (group end)
                cnt1 (controlsrc)
                spi1 (shape info)

                New in 9.0.0:
                ali1 (alignment pane)
                cpt1 (capture pane)
                scr1 (scissor pane)
                ugl1 (user graphics layer?)
                vgl1 (vector graphics layer)
                ctl1 (capture texture layer)
                stm1 (state machine)
             */

            for (int i = 0; i < sectionCount; i++)
            {
                //start of section
                var pos = reader.BaseStream.Position;

                //section magic and size
                string signature = reader.ReadString(4, Encoding.ASCII);
                uint sectionSize = reader.ReadUInt32();

                switch (signature)
                {
                    // Layout
                    case "lyt1":
                        this.Layout = BflytReader.ReadLayout(reader, this); 
                        break;
                    // Texture List
                    case "txl1":
                        this.TextureList = BflytReader.ReadStringSection(reader, this); 
                        break;
                    // Font List
                    case "fnl1":
                        this.FontList = BflytReader.ReadStringSection(reader, this); 
                        break;
                    // Pane
                    case "pan1": 
                        currentPane = BflytReader.ReadPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Picture Pane
                    case "pic1":
                        currentPane = BflytReader.ReadPicturePane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Text Pane
                    case "txt1":
                        currentPane = BflytReader.ReadTextPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Window Pane
                    case "wnd1":
                        currentPane = BflytReader.ReadWindowPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Part Pane
                    case "prt1":
                        currentPane = BflytReader.ReadPartPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Bounds Pane
                    case "bnd1":
                        currentPane = BflytReader.ReadBoundsPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Scissor Pane
                    case "scr1":
                        currentPane = BflytReader.ReadScissorPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Alignment Pane
                    case "ali1":
                        currentPane = BflytReader.ReadAlignmentPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Pane Connection Start
                    case "pas1":
                        if (currentPane != null)
                            parentPane = currentPane;
                        break;
                    // Pane Connection End
                    case "pae1":
                        if (parentPane != null)
                            currentPane = parentPane;
                        parentPane = (Pane)currentPane.Parent;
                        break;
                    // Group
                    case "grp1":
                        currentGroupPane = BflytReader.ReadGroup(reader, this);
                        Groups.Add(currentGroupPane);
                        currentGroupPane.Parent = parentGroupPane;
                        break;
                    // Group Connection Start
                    case "grs1":
                        if (currentGroupPane != null)
                            parentGroupPane = currentGroupPane;
                        break;
                    // Group Connection End
                    case "gre1":
                        currentGroupPane = parentGroupPane;
                        parentGroupPane = currentGroupPane.Parent;
                        break;
                    // Material table
                    case "mat1":
                        MaterialTable = BflytReader.ReadMaterialTable(reader, this);
                        break;
                    case "cnt1":
                        ControlSource = BflytReader.ReadControlSource(reader, this);
                        break;
                    case "ctl1":
                        CaptureTextureLayer = BflytReader.ReadCaptureTextureLayer(reader, this);
                        break;
                    case "usd1":
                        var usd = BflytReader.ReadUserData(reader, this);

                        if (ControlSource  != null)
                            ControlSource.UserData = usd;
                        else if (currentPane != null)
                            currentPane.UserData = usd;
                        else
                            this.UserData.Add(usd);
                        break;
                    default:
                        this.UnsupportedSections.Add(new UnsupportedSection()
                        {
                            Magic = signature,
                            Data = reader.ReadBytes((int)sectionSize - 8),
                        });
                        break;
                }
                reader.SeekBegin(pos + sectionSize);
            }

            Root = Panes.FirstOrDefault(x => x.Parent == null);
            RootGroup = Groups.FirstOrDefault(x => x.Parent == null);
        }
    }

    class UnsupportedSection 
    {
        public string Magic;
        public byte[] Data;

        public void Write(FileWriter writer, BflytFile header)
        {
            writer.Write(Data);
        }
    }
}
