using System.Reflection.PortableExecutable;

namespace LayoutLibrary
{
    public class ShaderVariationTableFile
    {
        public List<Variation> Variations = new List<Variation>();

        public ShaderVariationTableFile() { }
        public ShaderVariationTableFile(Stream stream)
        {
            Read(new FileReader(stream));
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream))
            {
                Write(writer);
            }
        }

        private void Read(FileReader reader)
        {
            uint numVariations = reader.ReadUInt32();
            for (int  i = 0; i < numVariations; i++)
            {
                Variation variation = new Variation();
                //DTCB (Detailed Combiner)
                //CBUS (Combiner User)
                //DTSH (Detailed Shader)
                //NORM (Normal)
                variation.Kind = reader.ReadSignature();
                variation.Size = reader.ReadUInt32() * 4;

                switch (variation.Kind)
                {
                    case "CBUS":
                        //Always 96 bytes length, likely fixed length string
                        variation.CodeFile = reader.ReadFixedString((int)variation.Size);
                        break;
                    case "DTCB":
                    case "NORM":
                    case "DRSH":
                    default:
                        variation.Values = reader.ReadUInt32s((int)variation.Size / 4);
                        break;
                }
                Variations.Add(variation);
            }
        }

        private void Write(FileWriter writer)
        {
            writer.Write(this.Variations.Count);
            for (int i = 0; i < this.Variations.Count; i++)
            {
                //DTCB (Detailed Combiner)
                //CBUS (Combiner User)
                //DTSH (Detailed Shader)
                //NORM (Normal)
                writer.WriteSignature(Variations[i].Kind);
                switch (Variations[i].Kind)
                {
                    case "CBUS":
                        //Always 96 bytes length, likely fixed length string
                        writer.Write(96 / 4);
                        writer.WriteFixedString(Variations[i].CodeFile, 96);
                        break;
                    case "DTCB":
                    case "NORM":
                    case "DRSH":
                    default:
                        writer.Write(Variations[i].Values.Length);
                        writer.Write(Variations[i].Values);
                        break;
                }
            }
        }

        public class Variation
        {
            public string Kind;
            public uint Size;

            public string CodeFile;

            public uint[] Values;
        }
    }
}
