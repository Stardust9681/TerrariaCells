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
using Terraria.Audio;
using Terraria.DataStructures;
using Mono.Cecil;
using TerrariaCells.Content.Projectiles;
using Terraria.Utilities;
using log4net.Repository.Hierarchy;

namespace TerrariaCells.Common.GlobalNPCs
{
    /// <summary>
    /// GlobalNPC class responsible for applying on-hit effects
    /// </summary>
    public class OnHitEffectsGlobalNPC : GlobalNPC
    {

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            // If we can get the source of our projectile, attempt to trigger the on-hit effects
            if (projectile.TryGetGlobalProjectile(out SourceGlobalProjectile sourceGlobalProjectile))
            {
                TriggerOnHit(sourceGlobalProjectile.itemSource, npc);
            }
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            // Attempt to trigger on-hit effects
            TriggerOnHit(item, npc);
        }

        /// <summary>
        /// Function that is called when an NPC is hit by an item or projectile
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="npc"></param>
        public void TriggerOnHit(Item sourceItem, NPC npc)
        {
            // If the modifier can be accessed, trigger the effect based upon the corresponding modifiers below
            if (sourceItem.TryGetGlobalItem(out ModifierGlobalItem modifierGlobalItem))
            {
                // Burning
                if (modifierGlobalItem.itemModifiers.Contains(ModifierSystem.Modifier.BurnOnHit))
                {
                    ModifierData data = ModifierSystem.GetModifierData(ModifierSystem.Modifier.BurnOnHit);

                    // Only trigger the effect(OnFire debuff) if the rng is higher than our effect chance
                    float rng = FastRandom.CreateWithRandomSeed().NextFloat();
                    if (rng > data.effectChance)
                    {
                        npc.AddBuff(BuffID.OnFire, 60); // Burn for 1 seconds
                    }
                }

                // Electrified
                if (modifierGlobalItem.itemModifiers.Contains(ModifierSystem.Modifier.Electrified))
                {
                    npc.AddBuff(BuffID.Electrified, 80); // Electrified for 1.33 seconds
                }

                // Exploding
                if (modifierGlobalItem.itemModifiers.Contains(ModifierSystem.Modifier.ExplodeOnHit))
                {
                    // Spawn explosion as projectile
                    var source = npc.GetSource_FromAI();
                    Vector2 position = npc.Center;
                    Projectile.NewProjectile(source, position, new Vector2(0, 0), ModContent.ProjectileType<ExplosionModProjectile>(), 10, 0f, Main.myPlayer);
                }

            }
        }
    }
}
