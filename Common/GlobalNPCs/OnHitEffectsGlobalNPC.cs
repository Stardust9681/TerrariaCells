using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.GlobalProjectiles;

namespace TerrariaCells.Common.GlobalNPCs
{
    public class OnHitEffectsGlobalNPC : GlobalNPC
    {

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            SourceGlobalProjectile testGlobalProjectile = null;
            projectile.TryGetGlobalProjectile(out testGlobalProjectile);

            if (testGlobalProjectile != null)
            {
                TriggerOnHit(testGlobalProjectile.itemSource, npc);
                //Mod.Logger.Debug(projectile.Name + " hit using a " + testGlobalProjectile.itemSource.Name + " with modifiers: ");
            }

            base.OnHitByProjectile(npc, projectile, hit, damageDone);
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            TriggerOnHit(item, npc);
            //Mod.Logger.Debug(item.Name + " hit");

            base.OnHitByItem(npc, player, item, hit, damageDone);
        }

        public void TriggerOnHit(Item sourceItem, NPC npc)
        {
            ModifierGlobalItem modifierGlobalItem = null;
            if (sourceItem != null)
            {
                sourceItem.TryGetGlobalItem(out modifierGlobalItem);
            }

            if (modifierGlobalItem != null)
            {

                if (modifierGlobalItem.itemModifiers.Contains(ModifierSystem.Modifier.Burning))
                {
                    Mod.Logger.Debug("BURN BABY BURN");
                }

                if (modifierGlobalItem.itemModifiers.Contains(ModifierSystem.Modifier.ExplodeOnHit))
                {
                    Explosion(npc.Center, 20);
                }

            }
        }

        private void Explosion(Vector2 position, int size)
        {

            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(position, size, size, DustID.Smoke, 0f, 0f, 100, default, 1.7f);
                Main.dust[dust].velocity *= 1.4f;
            }
            for (int i = 0; i < 27; i++)
            {
                int dust = Dust.NewDust(position, size, size, DustID.Torch, 0f, 0f, 100, default, 2.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 5f;
                dust = Dust.NewDust(position, size, size, DustID.Torch, 0f, 0f, 100, default, 1.6f);
                Main.dust[dust].velocity *= 3f;
            }
        }
    }
}
