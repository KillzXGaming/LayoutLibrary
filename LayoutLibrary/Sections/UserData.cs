using LayoutLibrary.Cafe;

namespace LayoutLibrary
{
    public class UserData 
    {
        public List<UserDataEntry> Entries { get; set; } = new List<UserDataEntry>();

        public byte[] Raw;

        public UserData() { }

        public class UserDataEntry
        {
            public string Name { get; set; }
            public ushort Length { get; set; }
            public UserDataType Type { get; set; }
            public byte Reserve0 { get; set; }

            public string GetString() => (string)_data;
            public float[] GetFloats() => (float[])_data;
            public int[] GetInts() => (int[])_data;
            public byte[] GetBytes() => (byte[])_data;
            public List<SystemData> GetStructs() => (List<SystemData>)_data;

            public object _data { get; set; }

            internal long _pos;

            internal int GetDataLength()
            {
                if (_data is string)
                    return ((string)_data).Length;
                else if (_data is int[])
                    return ((int[])_data).Length;
                else if (_data is float[])
                    return ((float[])_data).Length;
                else if (_data is List<SystemData>)
                    return ((List<SystemData>)_data).Count;
                return 0;
            }
        }

        public class SystemData
        {
            public ushort Version { get; set; } // 0

            public List<SystemDataEntry> Entries = new List<SystemDataEntry>();

            public SystemData(FileReader reader, BflytFile header)
            {
                long pos = reader.Position;

                Version = reader.ReadUInt16();
                ushort numEntries = reader.ReadUInt16();
                uint[] offsets = reader.ReadUInt32s(numEntries);

                for (int i = 0; i < numEntries; i++)
                {
                    reader.SeekBegin(pos + offsets[i]);
                    Entries.Add(new SystemDataEntry(reader, header));
                }
            }

            public byte[] Write( BflytFile header)
            {
                var mem = new MemoryStream();
                var writer = new FileWriter(mem);

                long pos = writer.Position;

                writer.Write(Version);
                writer.Write((ushort)Entries.Count);

                long _ofsPos = writer.Position;
                //Fill empty spaces for offsets later
                writer.Write(new uint[Entries.Count]);

                for (int i = 0; i < Entries.Count; i++)
                {
                    writer.WriteUint32Offset(_ofsPos + (i * 4), (int)pos);
                    Entries[i].Write(writer, header, pos);
                }

                return mem.ToArray();
            }
        }

        public class SystemDataEntry
        {
            public uint DataType;

            public List<string> TagNames = new List<string>();

            public SystemDataEntry(FileReader reader, BflytFile header)
            {
                long pos = reader.Position;

                DataType = reader.ReadUInt32();
                switch (DataType)
                {
                    case 0:
                        uint numEntries = reader.ReadUInt32();
                        uint[] offsets = reader.ReadUInt32s((int)numEntries);
                        for (int i = 0; i < numEntries; i++)
                        {
                            reader.SeekBegin(pos + offsets[i]);
                            TagNames.Add(reader.ReadZeroTerminatedString());
                        }
                        break;
                }
            }

            public void Write(FileWriter writer, BflytFile header, long usdStart)
            {
                long pos = writer.Position;

                writer.Write(DataType);
                switch (DataType)
                {
                    case 0:
                        writer.Write(TagNames.Count);
                        long _ofsPos = writer.Position;
                        //Fill empty spaces for offsets later
                        writer.Write(new uint[TagNames.Count]);

                        var alignPos = writer.Position;
                        for (int i = 0; i < TagNames.Count; i++)
                        {
                            writer.WriteUint32Offset(_ofsPos + (i * 4), (int)pos);
                            writer.WriteStringZeroTerminated((string)TagNames[i]);
                        }
                        //Here we align from string table start
                        writer.AlignBytes(alignPos, 64);
                        break;
                }

            }
        }
    }
}
