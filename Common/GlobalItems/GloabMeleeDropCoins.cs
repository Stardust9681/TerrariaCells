using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems;

public class GloabMeleeDropCoins : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.damage > 0;
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        int setPieces = 0;
        
        if (Main.LocalPlayer.armor[0].type == ItemID.GoldHelmet)
            setPieces++;
        if (Main.LocalPlayer.armor[1].type == ItemID.GoldChainmail) 
            setPieces++;
        if (Main.LocalPlayer.armor[2].type == ItemID.GoldGreaves)
            setPieces++;
        
        if (setPieces > 0)
            Item.NewItem(null, target.position, target.width, target.height, ItemID.SilverCoin, 5 * setPieces);
    }
}