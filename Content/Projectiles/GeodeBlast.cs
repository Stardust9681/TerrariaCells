using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles;

public class GeodeBlast : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
    }
    
    int Owner => (int)Projectile.ai[0];
    float TargetRotation => Projectile.ai[1];
    ref float BeamLen => ref Projectile.ai[2];

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
    }

    public override void AI()
    {
        NPC host = Main.npc[Owner];
        if(host.type != NPCID.GraniteFlyer || !host.active)
        {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;
        
        Projectile.Center = host.Center;
        
        Projectile.rotation = Utils.AngleTowards(Projectile.rotation, TargetRotation, MathHelper.ToRadians(40f/75f));
        Vector2 unitDirection = Vector2.UnitX.RotatedBy(Projectile.rotation);

        float[] samples = new float[3];
        Collision.LaserScan(Projectile.Center, unitDirection, 1, 50 * 16, samples);
        float sampleLen = samples.Sum() / 3;
        BeamLen = MathHelper.Lerp(BeamLen, sampleLen, 0.2f);
        
        DelegateMethods.v3_1 = new Color(100, 143, 246).ToVector3();
        Utils.PlotTileLine(Projectile.Center, Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * BeamLen, Projectile.width, DelegateMethods.CastLight);
        
        const float MinDustDistance = 4 * 16;
        if(Main.rand.NextBool())
        {
            float distance = Main.rand.NextFloat(MinDustDistance, BeamLen);
            Vector2 spawnPos = Projectile.Center + (unitDirection * distance);
            Dust d = Dust.NewDustDirect(spawnPos, 1, 1, DustID.Granite);
            d.noGravity = true;
            d.velocity = (host.Center - spawnPos).SafeNormalize(Vector2.Zero) * distance / 40;
            d.fadeIn = 0.2f;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if(projHitbox.Intersects(targetHitbox)) return true;

        float _ = float.NaN;
        Vector2 beamEndPos = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * BeamLen;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, Projectile.width, ref _);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        
        Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
        
        Utils.LaserLineFraming tileFraming = GeodeBlastFraming;
        
        Vector2 unitDirection = Vector2.UnitX.RotatedBy(Projectile.rotation);

        DelegateMethods.c_1 = new Color(79, 75, 134) * 0.33f;
        Utils.DrawLaser(Main.spriteBatch, tex, Projectile.Center - Main.screenPosition, Projectile.Center + (unitDirection * BeamLen) - Main.screenPosition, new Vector2(Projectile.scale * 1.66f), tileFraming);
        DelegateMethods.c_1 = new Color(100, 143, 246) * 0.66f;
        Utils.DrawLaser(Main.spriteBatch, tex, Projectile.Center - Main.screenPosition, Projectile.Center + (unitDirection* BeamLen) - Main.screenPosition, new Vector2(Projectile.scale * 1.33f), tileFraming);
        DelegateMethods.c_1 = Color.White * 0.9f;
        Utils.DrawLaser(Main.spriteBatch, tex, Projectile.Center - Main.screenPosition, Projectile.Center + (unitDirection * BeamLen) - Main.screenPosition, new Vector2(Projectile.scale), tileFraming);


        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        
        return false;
    }
    
    static void GeodeBlastFraming(int stage, Vector2 currentPosition, float distanceLeft, Rectangle lastFrame, out float distanceCovered, out Rectangle frame, out Vector2 origin, out Color color)
    {
        color = DelegateMethods.c_1;
        switch (stage)
        {
            case 0:
                distanceCovered = 20f;
                frame = new Rectangle(0, 0, 20, 20);
                origin = frame.Size() / 2f;
                break;
            case 1:
                frame = new Rectangle(0, 20, 20, 20);
                distanceCovered = 20;
                origin = new Vector2(frame.Width / 2, 0f);
                break;
            case 2:
                distanceCovered = 20f;
                frame = new Rectangle(0, 40, 20, 20);
                origin = new Vector2(frame.Width / 2, 1f);
                break;
            default:
                distanceCovered = 9999f;
                frame = Rectangle.Empty;
                origin = Vector2.Zero;
                color = Color.Transparent;
                break;
        }
    }
}