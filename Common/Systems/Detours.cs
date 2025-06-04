using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using TerrariaCells.Common.Configs;
using Terraria.GameContent.ObjectInteractions;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace TerrariaCells.Common.Systems
{
    public class Detours : ModSystem
    {
		public override void Load()
        {
            On_Main.DoDraw_UpdateCameraPosition += On_Main_DoDraw_UpdateCameraPosition;
            On_Player.PickupItem += On_Player_PickupItem;
            On_UIWorldSelect.NewWorldClick += On_UIWorldSelect_NewWorldClick;
            On_Player.PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool +=
                NoAmmoDamage;
            On_Player.QuickMinecartSnap += On_Player_QuickMinecartSnap;
            On_Player.QuickMinecart += On_Player_QuickMinecart;
            Terraria.UI.IL_ItemSlot.OverrideHover_ItemArray_int_int += IL_OverrideHover_ItemArray_int_int;
        }

        public override void Unload()
		{
			On_Main.DoDraw_UpdateCameraPosition -= On_Main_DoDraw_UpdateCameraPosition;
			On_Player.PickupItem -= On_Player_PickupItem;
			On_UIWorldSelect.NewWorldClick -= On_UIWorldSelect_NewWorldClick;
			On_Player.PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool -= NoAmmoDamage;
            On_Player.QuickMinecartSnap -= On_Player_QuickMinecartSnap;
            On_Player.QuickMinecart -= On_Player_QuickMinecart;
            Terraria.UI.IL_ItemSlot.OverrideHover_ItemArray_int_int -= IL_OverrideHover_ItemArray_int_int;
        }



        private void IL_OverrideHover_ItemArray_int_int(MonoMod.Cil.ILContext context)
        {
            try
            {
                ILCursor cursor = new ILCursor(context);

                while (cursor.TryGotoNext(
                    i => i.Match(OpCodes.Ldc_I4_6),
                    i => i.MatchStsfld(typeof(Main), "cursorOverride")))
                {
                    cursor.RemoveRange(2);
                    cursor.Emit(OpCodes.Ret);
                }
            }
            catch (Exception x)
            {
                ModContent.GetInstance<TerrariaCells>().Logger.Error(x);
            }
        }

        private void On_Player_QuickMinecart(On_Player.orig_QuickMinecart orig, Player self)
        {
            return;
        }
        private bool On_Player_QuickMinecartSnap(On_Player.orig_QuickMinecartSnap orig, Player self)
        {
            return false;
        }

        private void NoAmmoDamage(
            On_Player.orig_PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool orig,
            Player self,
            Item sItem,
            ref int projToShoot,
            ref float speed,
            ref bool canShoot,
            ref int totalDamage,
            ref float KnockBack,
            out int usedAmmoItemId,
            bool dontConsume
        )
        {
            int damag = totalDamage;
            orig(
                self,
                sItem,
                ref projToShoot,
                ref speed,
                ref canShoot,
                ref totalDamage,
                ref KnockBack,
                out usedAmmoItemId,
                dontConsume
            );
            totalDamage = damag;
        }

        private void On_UIWorldSelect_NewWorldClick(
            On_UIWorldSelect.orig_NewWorldClick orig,
            UIWorldSelect self,
            Terraria.UI.UIMouseEvent evt,
            Terraria.UI.UIElement listeningElement
        )
        {
            if (DevConfig.Instance.EnableCustomWorldGen)
            {
                string worldName =
                    "TerraCells-v" + ModLoader.GetMod("TerrariaCells").Version.ToString();

                // worldName = Main.GetWorldPathFromName(worldName, false);
                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
                string text = "";
                foreach (char c in worldName)
                {
                    text += (!invalidFileNameChars.Contains(c)) ? ((c != ' ') ? c : '_') : '-';
                }

                text = text.Replace(".", "-");
                text = text.Replace("*", "_");

                worldName = text;

                // orig.Invoke(self, evt, listeningElement);

                // UIWorldCreation.FinishCreatingWorld();

                // UIWorldCreation.

                // // Main.spawnTileX = 840;
                // // Main.spawnTileY = 240;
                // // case WorldSizeId.Large:
                Main.maxTilesX = 8400;
                Main.maxTilesY = 2400;
                Main.worldSurface = 400;
                Main.rockLayer = 800;

                WorldGen.setWorldSize();

                // // case WorldDifficultyId.Creative:
                Main.GameMode = 3;

                // // case WorldEvilId.Random:
                WorldGen.WorldGenParam_Evil = -1;

                if (FileUtilities.Exists(Main.WorldPath + "/" + worldName + ".wld", false))
                {
                    FileUtilities.Delete(Main.WorldPath + "/" + worldName + ".wld", false, true);
                }
                if (FileUtilities.Exists(Main.WorldPath + "/" + worldName + ".twld", false))
                {
                    FileUtilities.Delete(Main.WorldPath + "/" + worldName + ".twld", false, true);
                }

                Main.ActiveWorldFileData = WorldFile.CreateMetadata(
                    Main.worldName = worldName,
                    false,
                    Main.GameMode
                );

                // if (processedSeed.Length == 0)
                Main.ActiveWorldFileData.SetSeedToRandom();
                // else
                //     Main.ActiveWorldFileData.SetSeed(processedSeed);

                // Main.menuMode = 10;

                SoundEngine.PlaySound(SoundID.MenuOpen);
                WorldGen
                    .CreateNewWorld()
                    .GetAwaiter()
                    .OnCompleted(
                        delegate
                        {
                            FileUtilities.Copy(
                                Main.worldPathName,
                                Main.worldPathName + ".bak",
                                false
                            );
                            WorldGen.playWorld();
                        }
                    );

                return;
            }
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Debug.Write(Main.WorldPath);
            byte[] bytes = ModContent
                .GetInstance<TerrariaCells>()
                .GetFileBytes("Common/Assets/World/terracellsv0.2.1.wld");
            File.WriteAllBytes(Main.WorldPath + "/terracellsv0.2.1.wld", bytes);
            byte[] bytes2 = ModContent
                .GetInstance<TerrariaCells>()
                .GetFileBytes("Common/Assets/World/terracellsv0.2.1.twld");
            File.WriteAllBytes(Main.WorldPath + "/terracellsv0.2.1.twld", bytes2);

            Main.ActiveWorldFileData = new WorldFileData(
                Main.WorldPath + "/terracellsv0.2.1.wld",
                false
            );
            Main.worldName = "terracellsv0.3";
            WorldGen.playWorld();
        }

        //reduce amount of mana the little star pickups give
        private Item On_Player_PickupItem(
            On_Player.orig_PickupItem orig,
            Player self,
            int playerIndex,
            int worldItemArrayIndex,
            Item itemToPickUp
        )
        {
            if (
                itemToPickUp.type == ItemID.Star
                || itemToPickUp.type == ItemID.SoulCake
                || itemToPickUp.type == ItemID.SugarPlum
            )
            {
                SoundEngine.PlaySound(SoundID.Grab, self.position);
                self.statMana += 10;
                if (Main.myPlayer == self.whoAmI)
                {
                    self.ManaEffect(10);
                }
                if (self.statMana > self.statManaMax2)
                {
                    self.statMana = self.statManaMax2;
                }
                itemToPickUp = new Item();
                Main.item[worldItemArrayIndex] = itemToPickUp;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, worldItemArrayIndex);
                }
                return itemToPickUp;
            }
            return orig(self, playerIndex, worldItemArrayIndex, itemToPickUp);
        }

        //kill scope effect
        private void On_Main_DoDraw_UpdateCameraPosition(
            Terraria.On_Main.orig_DoDraw_UpdateCameraPosition orig
        )
        {
            int originalType = Main.LocalPlayer.HeldItem.type;
            Main.LocalPlayer.HeldItem.type = ItemID.None;
            orig();
            Main.LocalPlayer.HeldItem.type = originalType;
        }
    }
}
