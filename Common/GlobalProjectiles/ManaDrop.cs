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
                foreach (FunkyModifier funkyModifier in projectileFunker.instance.modifiersOnSourceItem)
                {
                    if (funkyModifier.modifierType == FunkyModifierType.DropMoreMana)
                    {
                        starsSpawned *= (int)funkyModifier.modifier;
                    }
                }
            }

            if (NPCID.Sets.ProjectileNPC[target.type])
                return;

            if (projectile.DamageType.CountsAsClass(DamageClass.Magic))
            {
                if (!SpawnedMana)
                {
                    SpawnedMana = true;
                    for (int i = 0; i < starsSpawned; i++)
                    {
                        int whoAmI = Item.NewItem(projectile.GetSource_OnHit(target), target.getRect(), ItemID.Star, noGrabDelay:true);
                        if(Main.netMode == 1)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, whoAmI, -1);
                        }
                    }
                }
            }
        }
    }
}
