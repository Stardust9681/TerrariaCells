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
			if (target.lifeMax <= 5 || target.friendly || !target.CanBeChasedBy() || NPCID.Sets.ProjectileNPC[target.type])
				return;

			//Fixed an error here that didn't show up for some reason
			if (projectile.DamageType.CountsAsClass(DamageClass.Magic))
			{
				if (Main.netMode != NetmodeID.MultiplayerClient && !SpawnedMana)
				{
					SpawnedMana = true;
					Item.NewItem(projectile.GetSource_OnHit(target), target.Hitbox, new Item(ItemID.Star));
				}
			}
		}
    }
}
