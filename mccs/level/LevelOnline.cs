using System.IO.Compression;
using System.Text;

namespace MineCS.mccs.level
{
    public class LevelOnline
    {
        private Client client;

        public LevelOnline(Client client)
        {
            this.client = client;
        }

        public bool saveLevelOnline(Level level, string url, string username, string id, string worldName, int worldId)
        {
            client.loadingScreenHeader("Saving level");
            client.loadingScreen("Failed: Minectfak does not support cloud saving/loading");
            Thread.Sleep(1000);
            return false;
        }

        public bool loadLevelOnline(Level level, string url, string username, int worldId)
        {
            client.loadingScreenHeader("Loading level");
            client.loadingScreen("Failed: Minectfak does not support cloud saving/loading");
            Thread.Sleep(1000);
            return false;
        }

        // Not focusing on compatibility with real client due to the fact it'll be broken in
        // a few updates anyway, and fixed when NBT data gets introduced. - Yunivers
        public bool load(Level level, FileStream data)
        {
            client.loadingScreenHeader("Loading level");
            client.loadingScreen("Reading..");
            BinaryReader reader = new BinaryReader(new MemoryStream());
            new GZipStream(data, CompressionMode.Decompress).CopyTo(reader.BaseStream);
            if (reader.BaseStream.Length == 0)
                return false;

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            int header = BitConverter.ToInt32(reader.ReadBytes(4));
            if (header != 0x271BB788)
                return false;

            byte version = reader.ReadByte();
            if (version > 1)
                return false;

            short nameLen = BitConverter.ToInt16(reader.ReadBytes(2));
            string name = Encoding.UTF8.GetString(reader.ReadBytes(nameLen));

            short usernameLen = BitConverter.ToInt16(reader.ReadBytes(2));
            string username = Encoding.UTF8.GetString(reader.ReadBytes(usernameLen));

            long creationTime = BitConverter.ToInt64(reader.ReadBytes(8));

            short width = BitConverter.ToInt16(reader.ReadBytes(2));
            short height = BitConverter.ToInt16(reader.ReadBytes(2));
            short depth = BitConverter.ToInt16(reader.ReadBytes(2));

            byte[] byArray = reader.ReadBytes(width * height * depth);
            reader.Close();
            reader.Dispose();

            level.load(width, depth, height, byArray);
            level.name = name;
            level.username = username;
            level.creationTime = creationTime;
            return true;
        }

        public bool loadOld(Level level, FileStream data)
        {
            client.loadingScreenHeader("Loading level");
            client.loadingScreen("Reading..");
            BinaryReader reader = new BinaryReader(new MemoryStream());
            new GZipStream(data, CompressionMode.Decompress).CopyTo(reader.BaseStream);
            if (reader.BaseStream.Length == 0)
                return false;
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            level.load(256, 64, 256, reader.ReadBytes(256 * 64 * 256));
            level.name = "--";
            level.username = "unknown";
            level.creationTime = 0L;
            return true;
        }

        public static void save(Level level, ref FileStream data)
        {
            BinaryWriter writer = new BinaryWriter(new GZipStream(data, CompressionLevel.Optimal));

            writer.Write(0x271BB788); // Header
            writer.Write((byte)1); // Version

            writer.Write((short)level.name.Length);
            writer.Write(Encoding.UTF8.GetBytes(level.name));

            writer.Write((short)level.username.Length);
            writer.Write(Encoding.UTF8.GetBytes(level.username));

            writer.Write(level.creationTime);

            writer.Write((short)level.width);
            writer.Write((short)level.height);
            writer.Write((short)level.depth);

            writer.Write(level.blockArray);
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }
    }
}
