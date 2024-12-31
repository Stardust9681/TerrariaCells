using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.GlobalProjectiles;
using TerrariaCells.Content.Projectiles;

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
                if (sourceGlobalProjectile.itemSource != null)
                {
                    TriggerOnHit(sourceGlobalProjectile.itemSource, npc);
                }
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
                foreach (ModifierSystem.Modifier modifier in modifierGlobalItem.itemModifiers)
                {
                    switch (modifier)
                    {
                        case ModifierSystem.Modifier.BurnOnHit:
                            {
                                ModifierData data = ModifierSystem.GetModifierData(ModifierSystem.Modifier.BurnOnHit);

                                // Only trigger the effect(OnFire debuff) if the rng is higher than our effect chance
                                float rng = Terraria.Main.rand.NextFloat();

                                if (rng > data.effectChance)
                                {
                                    npc.AddBuff(BuffID.OnFire, 60); // Burn for 1 seconds
                                }

                                break;
                            }

                        case ModifierSystem.Modifier.Electrified:
                            {
                                ModifierData data = ModifierSystem.GetModifierData(ModifierSystem.Modifier.BurnOnHit);

                                // Only trigger the effect(OnFire debuff) if the rng is higher than our effect chance
                                float rng = Terraria.Main.rand.NextFloat();

                                if (rng > data.effectChance)
                                {
                                    npc.AddBuff(BuffID.Electrified, 80); // Electrified for 1.33 seconds
                                }

                                break;
                            }
                        case ModifierSystem.Modifier.ExplodeOnHit:
                            {
                                ModifierData data = ModifierSystem.GetModifierData(ModifierSystem.Modifier.BurnOnHit);

                                // Only trigger the effect(OnFire debuff) if the rng is higher than our effect chance
                                float rng = Terraria.Main.rand.NextFloat();

                                if (rng > data.effectChance)
                                {
                                    // Spawn explosion as projectile
                                    var source = npc.GetSource_FromAI();
                                    Vector2 position = npc.Center;
                                    Projectile.NewProjectile(source, position, new Vector2(0, 0), ModContent.ProjectileType<ExplosionModProjectile>(), 10, 0f, Main.myPlayer);
                                }

                                break;
                            }


                    }

                }

            }

        }
    }
}
