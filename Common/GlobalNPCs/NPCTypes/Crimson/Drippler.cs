using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using Microsoft.Xna.Framework;

using TerrariaCells.Common.Utilities;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;

using static TerrariaCells.Common.Utilities.NPCHelpers;
using static TerrariaCells.Common.Utilities.NumberHelpers;
using Terraria.DataStructures;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Crimson
{
    public class Drippler : GlobalNPC, Shared.PreHitEffect.IGlobal
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Drippler;

        public override bool PreAI(NPC npc)
        {
            if (npc.ai[0] == 0)
                FloatAI(npc);
            else
                KilledAI(npc);

            return false;
        }

        private void FloatAI(NPC npc)
        {
            npc.ai[1] = MathF.Sign(npc.velocity.X);
            if (npc.ai[1] == 0)
                npc.ai[1] = 1;

            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.ai[1] * 0.8f, 0.05f);
            if (npc.collideX)
            {
                npc.velocity.X *= -1;
            }
            npc.direction = MathF.Sign(npc.velocity.X);

            const int GROUND_DISTANCE = 5;
            Vector2 ground = npc.FindGroundInFront(GROUND_DISTANCE);
            if (ground.Y < npc.position.Y + ((GROUND_DISTANCE - 1) * 16))
            {
                npc.ai[2] = 25;
                npc.velocity.Y -= 0.04f;
            }
            if (npc.ai[2] > 0)
            {
                npc.ai[2]--;
                npc.velocity.Y -= 0.03f;
            }
            else
            {
                npc.velocity.Y += 0.02f;
            }
            npc.velocity.Y = MathF.Min(MathF.Abs(npc.velocity.Y), 0.8f) * MathF.Sign(npc.velocity.Y);
        }
        private void KilledAI(NPC npc)
        {
            npc.velocity.Y += 0.14f;
            npc.rotation += npc.velocity.X;
            npc.ai[0]++;

            if (npc.ai[0] > 45 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.StrikeInstantKill();
            }
        }

        public bool PreHitEffect(NPC npc, int dir, double dmg, bool instant)
        {
            if (npc.ai[0] < 45)
                return false;
            return true;
        }
        public override bool CheckDead(NPC npc)
        {
            npc.dontTakeDamage = true;
            npc.life = 1;
            npc.ai[0] = MathF.Max(npc.ai[0], 1);
            return npc.ai[0] > 45;
        }
        public override bool PreKill(NPC npc)
        {
            bool dies = npc.ai[0] > 45 && npc.dontTakeDamage;
            if (!dies)
                npc.life = 1;
            return dies;
        }
        public override void OnKill(NPC npc)
        {
            Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DripplerExplosion>(), npc.damage, 1f, Main.myPlayer);
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            CombatNPC.ToggleContactDamage(npc, true);
        }
    }

    public class DripplerExplosion : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1;
            Projectile.tileCollide = false;
            Projectile.width = 120;
            Projectile.height = 120;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.position -= (Projectile.Size * 0.5f);

            for (int i = 0; i < 8 + Main.rand.Next(8); i++)
            {
                Vector2 spawnPos = Projectile.Center + (Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Projectile.Size.X * 0.33f);
                Vector2 vel = (spawnPos - Projectile.Center).SafeNormalize(Vector2.Zero);

                for (int j = 0; j < Main.rand.Next(3, 5); j++)
                {
                    Dust dust = Dust.NewDustDirect(spawnPos, 1, 1, DustID.Blood);
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    dust.scale = Main.rand.NextFloat(1.2f, 1.5f);
                    dust.velocity = vel * (j+4) * 0.5f;
                    dust.noGravity = false;
                }
            }
        }
    }
}
