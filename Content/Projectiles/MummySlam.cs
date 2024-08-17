using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    public class MummySlam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.DirtBlock;
        public override void SetDefaults()
        {
            base.SetDefaults();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void AI()
        {

            base.AI();
        }
    }
}
