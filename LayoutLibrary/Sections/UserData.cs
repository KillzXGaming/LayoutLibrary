using LayoutLibrary.Cafe;
using LayoutLibrary.Files;

namespace LayoutLibrary
{
    public class UserData 
    {
        public List<UserDataEntry> Entries { get; set; } = new List<UserDataEntry>();

        public byte[] Raw;

        public UserData() 
        {

        }

        public UserData(FileReader reader, LayoutHeader header)
        {
            long headerStart = reader.Position - 8;

            ushort num_userdata = reader.ReadUInt16();
            reader.ReadUInt16(); //padding

            for (int i = 0; i < num_userdata; i++)
            {
                this.Entries.Add(new UserDataEntry(reader, header, headerStart));

                if (this.Entries[i].Type == UserDataType.SystemData)
                    break;
            }

            if (this.Entries.Any(x => x.Type == UserDataType.SystemData))
            {
                //read as raw
                reader.SeekBegin(headerStart + 4);
                var size = reader.ReadUInt32();
                this.Raw = reader.ReadBytes((int)size - 8);
            }
        }

        public void Write(FileWriter writer, LayoutHeader header)
        {
            if (this.Raw != null)
            {
                writer.Write(this.Raw);
                return;
            }

            long headerStart = writer.Position;

            writer.Write((ushort)this.Entries.Count);
            writer.Write((ushort)0);
            for (int i = 0; i < this.Entries.Count; i++)
            {
                this.Entries[i]._pos = writer.Position;

                writer.Write(0); //nameOffset
                writer.Write(0); //dataOffset
                writer.Write((ushort)this.Entries[i].GetDataLength());
                writer.Write((byte)this.Entries[i].Type);
                writer.Write((byte)this.Entries[i].Reserve0);
            }

            var data_start = writer.Position;

            for (int i = 0; i < this.Entries.Count; i++)
            {
                if (this.Entries[i].Type == UserDataType.String)
                    continue;

                writer.WriteUint32Offset(this.Entries[i]._pos + 4, (int)this.Entries[i]._pos);
                switch (this.Entries[i].Type)
                {
                    case UserDataType.Int:
                        writer.Write(this.Entries[i].GetInts());
                        break;
                    case UserDataType.Float:
                        writer.Write(this.Entries[i].GetFloats());
                        break;
                    case UserDataType.SystemData:
                        foreach (var structure in this.Entries[i].GetStructs())
                            writer.Write(structure.Write(header));
                        break;
                }
            }

            // Build string table
            Dictionary<string, List<(long, long)>> table = new Dictionary<string, List<(long, long)>>();
            for (int i = 0; i < this.Entries.Count; i++)
            {
                var ofs_start = this.Entries[i]._pos;

                void AddEntry(string value, long target)
                {
                    if (!table.ContainsKey(value))
                        table.Add(value, new List<(long, long)>());

                    table[value].Add((target, ofs_start));
                }

                if (this.Entries[i].Type == UserDataType.String)
                {
                    AddEntry(this.Entries[i].GetString(), ofs_start + 4);
                }
                AddEntry(this.Entries[i].Name, ofs_start);
            }

            // Write string table
            foreach (var str in table)
            {
                foreach (var v in str.Value)
                    writer.WriteUint32Offset(v.Item1, (int)v.Item2);
                writer.WriteStringZeroTerminated(str.Key);
            }
        }

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

            public UserDataEntry() { }

            public UserDataEntry(FileReader reader, LayoutHeader header, long headerStart)
            {
                long pos = reader.Position;

                uint nameOffset = reader.ReadUInt32();
                uint dataOffset = reader.ReadUInt32();
                this.Length = reader.ReadUInt16();
                this.Type = (UserDataType)reader.ReadByte();
                this.Reserve0 = reader.ReadByte();

                long origin = reader.Position;

                if (nameOffset != 0)
                {
                    reader.SeekBegin(pos + nameOffset);
                    this.Name = reader.ReadZeroTerminatedString();
                }

                if (dataOffset != 0)
                {
                    reader.SeekBegin(pos + dataOffset);
                    switch (this.Type)
                    {
                        case UserDataType.String:
                            this._data = reader.ReadFixedString((int)this.Length);
                            break;
                        case UserDataType.Int:
                            this._data = reader.ReadInt32s((int)this.Length);
                            break;
                        case UserDataType.Float:
                            this._data = reader.ReadSingles((int)this.Length);
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

            public byte[] Write(LayoutHeader header)
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

            public void Write(FileWriter writer, LayoutHeader header, long usdStart)
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
