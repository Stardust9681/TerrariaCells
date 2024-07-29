using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class SpearAI : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public float SwingIntensity;
        public bool Stabby;
        public float OriginalRotation;
        public Vector2 OriginalVelocity;
        public override void SetDefaults(Projectile entity)
        {
            base.SetDefaults(entity);
            //SwingIntensity = 0;
            int[] swing1 = { ProjectileID.CobaltNaginata, ProjectileID.PalladiumPike, ProjectileID.AdamantiteGlaive, ProjectileID.MythrilHalberd, ProjectileID.OrichalcumHalberd , ProjectileID.TheRottedFork};
            if (swing1.Contains(entity.type))
            {
                SwingIntensity = 1f;
            }
        }
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            OriginalVelocity = projectile.velocity;
            base.OnSpawn(projectile, source);
            
        }
        public override void AI(Projectile projectile)
        {
            
            if (projectile.aiStyle != ProjAIStyleID.Spear)
            {
                return;
            }
            Player owner = Main.player[projectile.owner];
            if (!owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                projectile.Kill();
                return;
            }
            //owner.itemAnimationMax = originalItemAnimation;
            //ai[0] increases over time and reaches its max of the weapon's use time, and i need that info
            float maxAI = owner.HeldItem.useTime;
            int stabTime = 10;
            int beginRealbackTime = 12;
            
            float length = TextureAssets.Projectile[projectile.type].Size().Length() + 10;
            float angle = projectile.rotation - MathHelper.ToRadians(135 * -projectile.direction);
            float Offset = 5;
            Vector2 startPos = owner.Center + new Vector2(0, Offset) + new Vector2(50 * -projectile.direction, 0).RotatedBy(angle);
            if (SwingIntensity == 0)
            {
                if (projectile.ai[0] < stabTime)
                {
                    projectile.Center = Vector2.Lerp(startPos, owner.Center + new Vector2(0, Offset) + new Vector2(length * -projectile.direction, 0).RotatedBy(angle), projectile.ai[0] / stabTime);
                }
                else if (projectile.ai[0] > beginRealbackTime)
                {
                    float x = 1 - (projectile.ai[0] - beginRealbackTime) / (maxAI - beginRealbackTime);
                    projectile.Center = Vector2.Lerp(startPos, owner.Center + new Vector2(0, Offset) + new Vector2(length * -projectile.direction, 0).RotatedBy(angle), x * x * x);
                }
                else
                {
                    projectile.Center = owner.Center + new Vector2(0, Offset) + new Vector2(length * -projectile.direction, 0).RotatedBy(angle);
                }
            }
            else
            {
                float rx = projectile.ai[0] / maxAI;
                float rlerper = rx < 0.5 ? 16 * rx * rx * rx * rx * rx : 1 - (float)Math.Pow(-2 * rx + 2, 5) / 2;
                projectile.velocity = Vector2.Lerp(new Vector2(OriginalVelocity.Length(), 0).RotatedBy(OriginalVelocity.ToRotation() + MathHelper.ToRadians(-20 * SwingIntensity * projectile.direction)), new Vector2(OriginalVelocity.Length(), 0).RotatedBy(OriginalVelocity.ToRotation() + MathHelper.ToRadians(15 * SwingIntensity * projectile.direction)), rx);
                projectile.rotation = (projectile.velocity * - projectile.direction).ToRotation() + MathHelper.ToRadians(135 * -projectile.direction);
                angle = projectile.rotation - MathHelper.ToRadians(135 * -projectile.direction);
                startPos = owner.Center + new Vector2(50 * -projectile.direction, 0).RotatedBy(angle);

                if (projectile.ai[0] < maxAI / 2)
                {

                    float x = (projectile.ai[0] / (maxAI / 2));
                    float lerper = 1 - (float)Math.Pow(1 - x, 3);

                    projectile.Center = Vector2.Lerp(startPos, owner.Center + new Vector2(0, Offset) + new Vector2(length * -projectile.direction, 0).RotatedBy(angle), lerper);
                }
                else
                {
                    float x = 1 - ((projectile.ai[0] - maxAI / 2) / (maxAI - maxAI / 2));
                    float lerper = x * x * x;
                    projectile.Center = Vector2.Lerp(startPos, owner.Center + new Vector2(0, Offset) + new Vector2(length * -projectile.direction, 0).RotatedBy(angle), x);
                }

            }
        }
    }
}
