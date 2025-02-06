using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Content.Projectiles.HeldProjectiles;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class HoldoutWeaponChanges : GlobalProjectile
    {
        public override void SetDefaults(Projectile entity)
        {

            if (entity.type == ProjectileID.TerraBlade2 || entity.type == ProjectileID.NightsEdge)
            {
                entity.Opacity = 0;
            }
        }
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            base.OnSpawn(projectile, source);
        }
        public override void AI(Projectile projectile)
        {
            //exception for vortex beater to work with the active reload stuff
            if (projectile.type == ProjectileID.VortexBeater && projectile.active && false)
            {
                Player owner = Main.player[projectile.owner];
                if (!owner.active || owner.dead || owner.noItems || owner.CCed)
                {
                    return;
                }
                Item item = owner.HeldItem;
                WeaponHoldoutify weapon = item.GetGlobalItem<WeaponHoldoutify>();
                weapon.Ammo--;
                if (weapon.Ammo <= 0)
                {
                    Projectile.NewProjectileDirect(owner.GetSource_ItemUse(item), owner.Center, Vector2.Zero, ModContent.ProjectileType<Guns>(), 0, 0, owner.whoAmI, item.type, 0, 1);
                    projectile.Kill();

                }
            }
            if (projectile.type == ProjectileID.DD2PhoenixBow && projectile.active)
            {
                Player owner = Main.player[projectile.owner];
                if (!owner.active || owner.dead || owner.noItems || owner.CCed)
                {
                    return;
                }
                Item item = owner.HeldItem;
                if (owner.controlUseTile && owner.itemAnimation == 0)
                {
                    Projectile.NewProjectileDirect(owner.GetSource_ItemUse(item), owner.Center, Vector2.Zero, ModContent.ProjectileType<Bow>(), 0, 0, owner.whoAmI, item.type, 0, 1);
                    projectile.Kill();
                }
            }

            base.AI(projectile);
        }
    }
}
