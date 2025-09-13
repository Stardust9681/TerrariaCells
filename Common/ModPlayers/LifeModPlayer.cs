using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.ModPlayers
{
    public class LifeModPlayer : ModPlayer
    {
        public int extraHealth;

        public override void Load()
        {
            On_Player.ItemCheck_UseLifeCrystal += On_Player_ItemCheck_UseLifeCrystal;
        }
        public override void Unload()
        {
            On_Player.ItemCheck_UseLifeCrystal -= On_Player_ItemCheck_UseLifeCrystal;
        }

        private static void On_Player_ItemCheck_UseLifeCrystal(On_Player.orig_ItemCheck_UseLifeCrystal orig, Player self, Item sItem)
        {
            //IRONICALLY, vanilla still has to check here that sItem IS IN FACT a Life Crystal
            //So, *so* do we :(
            if (sItem.type == ItemID.LifeCrystal && self.itemAnimation > 0 && self.ItemTimeIsZero)
            {
                self.ApplyItemTime(sItem);
                self.GetModPlayer<LifeModPlayer>().IncreasePlayerHealth(20);
                //Does this matter? Do we care?
                //AchievementsHelper.HandleSpecialEvent(self, 0);
                return;
            }
            orig.Invoke(self, sItem);
        }

        internal void IncreasePlayerHealth(int amount)
        {
            extraHealth += amount;
            Player.Heal(amount);
        }

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            StatModifier mod = new();
            mana = mod;
            mod.Flat = (float)extraHealth;
            health = mod;
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            var packet = ModNetHandler.GetPacket(Mod, TCPacketType.PlayerPacket);
            packet.Write((byte)Player.whoAmI);
            packet.Write((ushort)extraHealth);
            packet.Send(toWho, fromWho);
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("extraHealth", extraHealth);
        }

        public override void LoadData(TagCompound tag)
        {
            extraHealth = tag.GetInt("extraHealth");
        }
    }
}
