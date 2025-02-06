using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class ManaDrop : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.DamageType == DamageClass.Magic || projectile.DamageType == DamageClass.MagicSummonHybrid)
            {
                if (Main.rand.NextBool(2) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Item.NewItem(projectile.GetSource_OnHit(target), target.Hitbox, new Item(ItemID.Star));
                }
            }
            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }
}
