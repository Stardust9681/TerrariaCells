using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Buffs
{
    public class ShadowDodgeBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.ShadowFlame;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ShadowDodgePlayer>().isShadowDodgeActive = true;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            buffName = "Shadow Dodge";
            tip = "You will dodge the next attack";
        }
    }

    public class ShadowDodgePlayer : ModPlayer
    {
        public bool isShadowDodgeActive = false;

        public override void ResetEffects()
        {
            isShadowDodgeActive = false;
        }

        public override bool ConsumableDodge(Player.HurtInfo info)
        {
            if (isShadowDodgeActive)
            {
                isShadowDodgeActive = false;
                Player.ClearBuff(ModContent.BuffType<ShadowDodgeBuff>());
                Player.immune = true;
                Player.immuneTime = 80;
                Main.LocalPlayer.chatOverhead.NewMessage("Dodged!", 60);
                return true;
            }
            return false;
        }
    }
}
