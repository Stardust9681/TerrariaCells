using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.TileEntities {
    internal class SoundPlayerTileEntity : ModTileEntity {
        internal int soundId;
        internal int x;
        internal int y;
        public SoundStyle sound => SOUNDS[soundId];
        public string label => LABELS[soundId];
        public void next() {
            soundId++;
            soundId %= SOUNDS.Length;
        }
        public void prev() {
            soundId--;
            if (soundId < 0) {
                soundId += SOUNDS.Length;
            }
        }
        internal static SoundStyle style(string name) => new($"{nameof(TerrariaCells)}/Common/Assets/SoundPlayer/Sounds/{name}");
        public static readonly SoundStyle[] SOUNDS = [
            SoundID.Mech, 
            style("SecretZelda"),
            style("Secret1"),
            style("Secret2"),
        ];
        public static readonly string[] LABELS = [
            "Wire Click",
            "Zelda Secret",
            "Secret 1",
            "Secret 2",
        ];
        public override bool IsTileValidForEntity(int x, int y) {
            var tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SoundPlayerTile>();
        }

        public static bool TryGet(int i, int j, out SoundPlayerTileEntity entity) {
            if (ByPosition.TryGetValue(
                new(i, j),
                out TileEntity existing
            ) && existing is SoundPlayerTileEntity found) {
                entity = found;
                return true;
            }

            entity = null;
            return false;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
            }
            var ret = Place(i, j);

            if (TryGet(i, j, out var entity)) {
                entity.soundId = 0;
                entity.x = i;
                entity.y = j;
            }
            return ret;
        }

        public override void OnNetPlace() {
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
            }
        }
        public override void NetSend(BinaryWriter writer) {
            writer.Write(soundId);
            writer.Write(x);
            writer.Write(y);
        }
        public override void NetReceive(BinaryReader reader) {
            soundId = reader.ReadInt32();
            x = reader.ReadInt32();
            y = reader.ReadInt32();
        }

        public override void SaveData(TagCompound tag) {
            tag[nameof(soundId)] = soundId;
            tag[nameof(x)] = x;
            tag[nameof(y)] = y;
        }

        public override void LoadData(TagCompound tag) {
            soundId = tag.GetInt(nameof(soundId));
            x = tag.GetInt(nameof(x));
            y = tag.GetInt(nameof(y));
        }
    }
}
