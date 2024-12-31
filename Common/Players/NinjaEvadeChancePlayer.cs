using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Players;

public class NinjaEvadeChancePlayer : ModPlayer
{
    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
    {
        float dodgeChance = 0f;
        Player localPlayer = Main.LocalPlayer;
        
        if (localPlayer.armor[1].type == ItemID.NinjaShirt && Player == localPlayer)
        {
            dodgeChance += 0.1f;
            
            if (localPlayer.armor[0].type == ItemID.NinjaHood && localPlayer.armor[2].type == ItemID.NinjaPants)
                dodgeChance += 0.15f;

            if (Main.rand.NextFloat() < dodgeChance)
            {
                localPlayer.chatOverhead.NewMessage("Dodged!", 60); // TODO: Find better way to display dodge message (like calamity fractured ark?)
                localPlayer.GiveImmuneTimeForCollisionAttack(10); // TODO: Find better way to give immunity frames (per damage source?)
                return true;
            }
        }
        
        return false;
    }
}