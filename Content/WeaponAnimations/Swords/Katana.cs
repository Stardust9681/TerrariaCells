using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.WeaponAnimations.Swords;

public class Katana : SwordModifier
{
    public override short Type { get => ItemID.Katana; }
    public override void SetDefaults(Sword globalItem, Item item)
    {
        SwingData.ApplySwingStyleOverrides(
            globalItem, 
            [
                new SwingData() with { easingStyle = TCellsUtils.LerpEasing.InBack },
                new SwingData() with { },   
                new SwingData() with { finalSwingReuseTimer = 0 }
            ]
        );
        item.scale *= 1.15f;
        item.damage = (int)(item.damage * 1.5f);
    }
}

public class KatanaStun : GlobalItem {
    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (item.type == ItemID.Katana
            && player.TryGetModPlayer(out WeaponPlayer mplayer)
            && mplayer.swingType == 0) {
            target.velocity = target.velocity.SafeNormalize(Vector2.Zero);
        }
    }
}