using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class VanillaTrapsDamage : GlobalProjectile
    {
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            int[] trapProjectiles = {
                ProjectileID.SpearTrap, ProjectileID.Boulder, ProjectileID.MiniBoulder,
                ProjectileID.PoisonDart, ProjectileID.PoisonDartTrap, ProjectileID.GeyserTrap,
                ProjectileID.SpikyBallTrap, ProjectileID.FlamesTrap, ProjectileID.FlamethrowerTrap
            };

            if (trapProjectiles.Contains(projectile.type))
            {
                projectile.damage = 25;
            }
        }
    }
}
