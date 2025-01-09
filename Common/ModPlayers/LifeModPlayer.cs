using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.ModPlayers
{
    public class LifeModPlayer : ModPlayer
    {
        public int extraHealth;

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            StatModifier mod = new();
            mana = mod;
            mod.Flat = (float)extraHealth;
            health = mod;
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
