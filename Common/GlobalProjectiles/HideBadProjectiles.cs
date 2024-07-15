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
    public class HideBadProjectiles : GlobalProjectile
    {
        public override void SetDefaults(Projectile entity)
        {

            if (entity.type == ProjectileID.TerraBlade2 || entity.type == ProjectileID.NightsEdge)
            {
                entity.Opacity = 0;
            }
        }
    }
}
