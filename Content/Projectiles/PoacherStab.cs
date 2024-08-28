using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    public class PoacherStab : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = 20;
            Projectile.width = Projectile.height = 20;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.Opacity = 0;
            Projectile.scale = 0.6f;
            Projectile.hide = true;
            Projectile.friendly = false;
            Projectile.hostile = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Projectile.type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];

            if (owner == null || !owner.active || owner.type != NPCID.DesertScorpionWalk)
            {
                Projectile.Kill();
                return false;
            }
            if (!Collision.CanHitLine(Projectile.Center, 1, 1, owner.Center, 1, 1))
            {
                return false;
            }
            return base.Colliding(projHitbox, targetHitbox);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }
        public override void AI()
        {
            int maxTime = 20;

            float xIn = 1 - ((float)Projectile.timeLeft - maxTime/2) / (maxTime/2);
            if (Projectile.timeLeft < maxTime / 2) xIn = 1;
            float xFull = 1 - (float)Projectile.timeLeft / maxTime;
            float xOut = 1 - ((float)Projectile.timeLeft) / (maxTime * 0.8f);
            if (Projectile.timeLeft > maxTime * 0.8f) xOut = 0;

            float lerpIn = xIn * xIn * xIn * xIn * xIn;
            float lerpFull = 1 - (float)Math.Pow(1 - xFull, 5);
            float lerpOut = 1 - (float)Math.Pow(1 - xOut, 5);

            NPC owner = Main.npc[(int)Projectile.ai[0]];

            if (owner == null || !owner.active || owner.type != NPCID.DesertScorpionWalk)
            {
                Projectile.Kill();
                return;
            }

            Projectile.rotation = MathHelper.PiOver2;
            Vector2 start = new Vector2(0, -25);
            Vector2 end = new Vector2(20*owner.direction, -25);
            Projectile.position = Vector2.Lerp(owner.Center + start, owner.Center + end, lerpFull);

            if (Projectile.timeLeft < maxTime * 0.8f)
            {
                Projectile.Opacity = MathHelper.Lerp(1, 0, lerpOut);
            }
            if (Projectile.timeLeft > maxTime * 0.8f)
            {
                Projectile.Opacity = 1;
                //Projectile.Opacity = MathHelper.Lerp(0, 1, lerpIn);
            }

            base.AI();
        }
    }
}
