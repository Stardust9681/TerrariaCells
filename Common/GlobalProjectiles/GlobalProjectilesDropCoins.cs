using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalProjectiles;

public class GlobalProjectilesDropCoins : GlobalProjectile
{
    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
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