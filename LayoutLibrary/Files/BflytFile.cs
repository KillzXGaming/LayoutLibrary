using LayoutLibrary.Files;
using System.IO;
using System.Numerics;
using System.Text;

namespace LayoutLibrary.Cafe
{
    /// <summary>
    /// A UI layout format used for reading and writing BFLYT/BRLYT/BCLYT file binaries.
    /// </summary>
    public class BflytFile : LayoutHeader
    {
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
        public MaterialTable MaterialTable = new MaterialTable();

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

        public static bool Identity(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                reader.BaseStream.Position = 0;

                return magic == "FLYT" || magic == "RLYT" || magic == "CLYT";
            }
        }

        public BflytFile() { }
        public BflytFile(string path) : base(path) { }

        public BflytFile(Stream stream) : base(stream) { }

        internal override void ReadSections(FileReader reader, int sectionCount)
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
                vgl1 (vector graphics layer)
                ctl1 (capture texture layer)
                stm1 (state machine)
             */

            for (int i = 0; i < sectionCount; i++)
            {
                //start of section
                var pos = reader.BaseStream.Position;

                //section magic and size
                string signature = reader.ReadSignature();
                uint sectionSize = reader.ReadUInt32();

                switch (signature)
                {
                    // Layout
                    case "lyt1":
                        this.Layout = new Layout(reader, this); 
                        break;
                    // Texture List
                    case "txl1":
                        this.TextureList = ReadUtility.ReadStringSection(reader, this); 
                        break;
                    // Font List
                    case "fnl1":
                        this.FontList = ReadUtility.ReadStringSection(reader, this); 
                        break;
                    // Pane
                    case "pan1": 
                        currentPane = new Pane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Picture Pane
                    case "pic1":
                        currentPane = new PicturePane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Text Pane
                    case "txt1":
                        currentPane = new TextPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Window Pane
                    case "wnd1":
                        currentPane = new WindowPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Part Pane
                    case "prt1":
                        currentPane = new PartsPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Bounds Pane
                    case "bnd1":
                        currentPane = new BoundsPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Scissor Pane
                    case "scr1":
                        currentPane = new ScissorPane(reader, this);
                        Panes.Add(currentPane);
                        currentPane.Parent = parentPane;
                        break;
                    // Alignment Pane
                    case "ali1":
                        currentPane = new AlignmentPane(reader, this);
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
                        currentGroupPane = new Group(reader, this);
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
                        MaterialTable = new MaterialTable(reader, this);
                        break;
                    case "cnt1":
                        ControlSource = new ControlSource(reader, this);
                        break;
                    case "ctl1":
                        CaptureTextureLayer = new CaptureTextureLayer(reader, this);
                        break;
                    case "usd1":
                        var usd = new UserData(reader, this);

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


        internal override void WriteSections(FileWriter writer, ref int numSections)
        {
            WriteSection(writer, "lyt1", () => Layout.Write(writer, this), ref numSections);

            foreach (var usd in this.UserData)
            {
                WriteSection(writer, "usd1", () => usd.Write(writer, this), ref numSections);
            }

            if (this.TextureList?.Count > 0)
                WriteSection(writer, "txl1", () => WriteUtility.WriteStringSection(writer, this.TextureList, this), ref numSections);
            if (this.FontList?.Count > 0)
                WriteSection(writer, "fnl1", () => WriteUtility.WriteStringSection(writer, this.FontList, this), ref numSections);
            if (this.MaterialTable.Materials.Count > 0)
                WriteSection(writer, "mat1", () => MaterialTable.Write(writer, this), ref numSections);

            if (this.CaptureTextureLayer != null)
            {
                WriteSection(writer, "ctl1", () => CaptureTextureLayer.Write(writer, this), ref numSections);
            }

            if (this.Root != null)
                WritePanes(writer, this.Root, ref numSections);

            if (this.RootGroup != null)
                WriteGroups(writer, this.RootGroup, ref numSections);

            if (this.ControlSource != null)
            {
                WriteSection(writer, "cnt1", () => ControlSource.Write(writer, this), ref numSections);
                if (ControlSource.UserData != null)
                    WriteSection(writer, "usd1", () => ControlSource.UserData.Write(writer, this), ref numSections);
            }

            foreach (var section in this.UnsupportedSections)
                WriteSection(writer, section.Magic, () => section.Write(writer, this), ref numSections);
        }

        private void WritePanes(FileWriter writer, Pane pane, ref int numSections)
        {
            WriteSection(writer, pane.Magic, () => this.WritePaneKind(pane, writer), ref numSections);

            if (pane.UserData != null)
                WriteSection(writer, "usd1", () => pane.UserData.Write(writer, this), ref numSections);

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
            WriteSection(writer, "grp1", () => grp.Write(writer, this), ref numSections);

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
    }
}
