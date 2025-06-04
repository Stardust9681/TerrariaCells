using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class ManaDrop : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool SpawnedMana = false;
        //setting projectiles we dont want spawning mana as not magic projectiles
        public override void SetDefaults(Projectile entity)
        {
            if (entity.type == ProjectileID.ClingerStaff || (entity.type >= ProjectileID.ToxicFlask && entity.type <= ProjectileID.ToxicCloud3))
            {
                entity.DamageType = DamageClass.Generic;
            }
            base.SetDefaults(entity);
        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            int starsSpawned = 1;
            if (
                projectile.TryGetGlobalProjectile(out ProjectileFunker projectileFunker)
                && projectileFunker.SetInstance(projectile)
            )
            {
                foreach (
                    FunkyModifier funkyModifier in projectileFunker.instance.modifiersOnSourceItem
                )
                {
                    if (funkyModifier.modifierType == FunkyModifierType.DropMoreMana)
                    {
                        starsSpawned *= (int)funkyModifier.modifier;
                    }
                }
            }

            if (target.life > 0) //This lets projectiles cause enemies to drop mana stars when killed too
            {                    // I have no idea why this is necessary for it though... :(
                if (!target.CanBeChasedBy() || NPCID.Sets.ProjectileNPC[target.type])
                    return;
            }

            //Fixed an error here that didn't show up for some reason
            if (projectile.DamageType.CountsAsClass(DamageClass.Magic))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && !SpawnedMana)
                {
                    SpawnedMana = true;
                    for (int i = 0; i < starsSpawned; i++)
                    {
                        Item.NewItem(projectile.GetSource_OnHit(target), target.Hitbox, new Item(ItemID.Star));
                    }
                }
            }
        }
    }
}
