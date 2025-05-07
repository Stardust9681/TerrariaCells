using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerrariaCells.Common.UI;
using TerrariaCells.Content.TileEntities;

namespace TerrariaCells.Content.Tiles {
    internal class SoundPlayerTile : ModTile {
        public override string Texture => $"{nameof(TerrariaCells)}/Common/Assets/SoundPlayer/Icon";
        static bool allow = true;
        public override void Load() {
            On_Wiring.HitSwitch += DisableClicks;
            On_SoundEngine.PlaySound_int_int_int_int_float_float += HackSound;
        }

        static private void DisableClicks(On_Wiring.orig_HitSwitch orig, int i, int j) {
            allow = false;
            orig(i, j);
            allow = true;
        }

        static private SoundEffectInstance HackSound(On_SoundEngine.orig_PlaySound_int_int_int_int_float_float orig, int type, int x, int y, int Style, float volumeScale, float pitchOffset) {
            if (type == 28 && !allow) {
                return null;
            }
            return orig(type, x, y, Style, volumeScale, pitchOffset);
        }

        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new(
                ModContent.GetInstance<SoundPlayerTileEntity>().Hook_AfterPlacement,
                -1,
                0,
                true
            );
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
            ModContent.GetInstance<SoundPlayerTileEntity>().Kill(i, j);
            var state = ModContent.GetInstance<SoundPlayerUI>().state;
            if (SoundPlayerTileEntity.TryGet(i, j, out var entity) && state.tile == entity) {
                state.tile = null;
            }
        }

        public override bool RightClick(int i, int j) {
            Player player = Main.LocalPlayer;
            Main.mouseRightRelease = false;
            if (player.sign > -1) {
                SoundEngine.PlaySound(SoundID.MenuClose);
                player.sign = -1;
                Main.editSign = false;
                Main.npcChatText = string.Empty;
            }
            if (Main.editChest) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                Main.npcChatText = string.Empty;
            }
            if (player.editedChestName) {
                NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
                player.editedChestName = false;
            }
            if (player.talkNPC > -1) {
                player.SetTalkNPC(-1);
                Main.npcChatCornerItem = 0;
                Main.npcChatText = string.Empty;
            }

            if (SoundPlayerTileEntity.TryGet(i, j, out var entity)) {
                ModContent.GetInstance<SoundPlayerUI>().state.tile = entity;
            }
            return true;
        }

        public override void HitWire(int i, int j) {
            if (SoundPlayerTileEntity.TryGet(i, j, out var entity)) {
                SoundEngine.PlaySound(
                    entity.sound,
                    new((entity.x + i) * 16, (entity.y + j) * 16)
                );
            }
        }
    }
}
