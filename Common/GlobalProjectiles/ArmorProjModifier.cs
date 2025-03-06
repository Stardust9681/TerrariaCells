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
    public class ArmorProjModifier : GlobalProjectile
    {
        public override void SetDefaults(Projectile projectile)
        {
            switch (projectile.type)
            {
                case ProjectileID.BabySpider:
                    projectile.timeLeft = 480;
                    projectile.aiStyle = ProjAIStyleID.BabySpider;
                    projectile.penetrate = -1;
                    break;
            }
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            switch (projectile.type)
            {
                case ProjectileID.BabySpider:
                    target.BecomeImmuneTo(BuffID.Venom);
                    break;
            }
        }
    }
}
