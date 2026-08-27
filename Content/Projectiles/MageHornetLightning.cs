using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles;

public class MageHornetLightning : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.VortexLightning}";

    private static readonly Color LightningColour = new Color(255, 85, 174);

    public override void SetStaticDefaults()
    {
        //Handling this ourselves
        ProjectileID.Sets.TrailingMode[Projectile.type] = -1;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 32;
    }

    public override void SetDefaults()
    {
        this.Projectile.extraUpdates = ExtraUpdates;
        this.Projectile.width = 14;
        this.Projectile.height = 14;
        //this.Projectile.aiStyle = 88;
        this.Projectile.hostile = true;
        this.Projectile.alpha = 255;
        this.Projectile.ignoreWater = true;
        this.Projectile.tileCollide = false;
        this.Projectile.extraUpdates = 4;
        this.Projectile.timeLeft = Projectile.ArrowLifeTime;

        Array.Resize(ref Projectile.oldPos, ProjectileID.Sets.TrailCacheLength[Projectile.type]);
        Array.Resize(ref Projectile.oldRot, ProjectileID.Sets.TrailCacheLength[Projectile.type]);
        Array.Resize(ref Projectile.oldSpriteDirection, ProjectileID.Sets.TrailCacheLength[Projectile.type]);
    }

    const int HeightAboveGround = 24;
    public override void OnSpawn(IEntitySource source)
    {
        const int AdjustmentPerStep = 3;
        for (int i = 0; i < HeightAboveGround * 16 / AdjustmentPerStep; i++)
        {
            if (Collision.IsWorldPointSolid(Projectile.Bottom + new Vector2(0, AdjustmentPerStep)))
                break;
            Projectile.position.Y += AdjustmentPerStep;
        }
        
        Projectile.ai[1] = Projectile.Bottom.X;
        Projectile.ai[2] = Projectile.Bottom.Y;
        
        //Spawn 16 tiles in the air
        Projectile.position.Y -= HeightAboveGround * 16;
        Projectile.velocity = Vector2.Zero;

        Array.Fill(Projectile.oldPos, Projectile.position);
        Array.Fill(Projectile.oldRot, Projectile.rotation);
        Array.Fill(Projectile.oldSpriteDirection, Projectile.spriteDirection);
    }
    
    private void PushOldValueTrail()
    {
        int len = ProjectileID.Sets.TrailCacheLength[Projectile.type];
        for (int i = 1; i < len; i++)
        {
            Projectile.oldPos[i-1] = Projectile.oldPos[i];
            Projectile.oldRot[i-1] = Projectile.oldRot[i];
            Projectile.oldSpriteDirection[i-1] = Projectile.oldSpriteDirection[i];
        }
        Projectile.oldPos[len-1] = Projectile.position;
        Projectile.oldRot[len-1] = Projectile.rotation;
        Projectile.oldSpriteDirection[len-1] = Projectile.spriteDirection;
    }
    
    const int ExtraUpdates = 7;
    const int Countdown = 30 * ExtraUpdates;
    const float MaxHorizontalVariance = 0.66f * 16;
    public override void AI()
    {
        if (Projectile.Center.Y >= Projectile.ai[2])
        {
            Projectile.velocity = Vector2.Zero;
            if(Projectile.timeLeft > 90 * Projectile.extraUpdates)
            {
                Projectile.timeLeft = 90 * Projectile.extraUpdates;
                PushOldValueTrail();
            }
            return;
        }

        Projectile.ai[0]++;
        
        if(MathF.Abs(Projectile.Center.X - Projectile.ai[1]) > MaxHorizontalVariance || (Projectile.ai[0] > Countdown && Main.rand.NextBool(60 + Countdown - (int)Projectile.ai[0])))
        {
            Projectile.ai[0] = Countdown;
        }
        
        if(Projectile.ai[0] == Countdown)
        {
            PushOldValueTrail();
            int direction = Projectile.Center.X < Projectile.ai[1] ? 1 : -1;
            Projectile.velocity = new Vector2(Main.rand.NextFloat(MaxHorizontalVariance * 0.1f, MaxHorizontalVariance * 0.33f) * direction, Main.rand.NextFloat(4, 6));
            Projectile.netUpdate = true;
        }
    }

    public override void PostAI()
    {
        if(Projectile.ai[0] < Countdown)
        {
            if(Main.rand.NextBool(5))
            {
                float width = 2 * MaxHorizontalVariance * (1 - (Projectile.ai[0] / Countdown));
                width *= 4;
                Rectangle area = new Rectangle((int)(Projectile.ai[1] - (width * 0.5f)), (int)Projectile.ai[2] - (HeightAboveGround * 16), (int)width, HeightAboveGround * 16);

                Dust dust = Dust.NewDustDirect(area.Location.ToVector2(), area.Width, area.Height, DustID.Firework_Pink);
                dust.noGravity = true;
                dust.velocity = -Vector2.UnitY * Main.rand.NextFloat(4);
            }

            return;
        }
    
        int len = ProjectileID.Sets.TrailCacheLength[Projectile.type];
        Vector2 oldPos = Projectile.oldPos[len-1];
        for(int i = len-2; i > 0; i--)
        {
            Vector2 newOldPos = Projectile.oldPos[i];
            if(newOldPos == oldPos) break;

            if(Main.rand.NextBool(60))
            {
                Dust dust = Dust.NewDustDirect(Vector2.Lerp(oldPos, newOldPos, Main.rand.NextFloat()) - new Vector2(16), 32, 32, DustID.Firework_Pink);
                dust.velocity = (oldPos - newOldPos).SafeNormalize(Vector2.Zero);
                dust.noGravity = true;
            }
            
            Lighting.AddLight(oldPos, LightningColour.ToVector3() * 0.1f);

            oldPos = newOldPos;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        if (Projectile.ai[0] < Countdown)
        {
            float width = 2 * MaxHorizontalVariance * (1 - (Projectile.ai[0] / Countdown));
            width *= 3.5f;
            Rectangle area = new Rectangle((int)(Projectile.ai[1] - (width * 0.5f)), (int)Projectile.ai[2] - (HeightAboveGround * 16), (int)width, HeightAboveGround * 16);
            area.X -= (int)Main.screenPosition.X;
            area.Y -= (int)Main.screenPosition.Y;
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, area, LightningColour * 0.33f);
        }
        else
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;// TextureAssets.Extra[ExtrasID.CultistLightingArc].Value;
            int len = ProjectileID.Sets.TrailCacheLength[Projectile.type];

            int max = 0;
            Vector2 oldPos = Projectile.oldPos[len - 1];
            for(int i = len - 2; i > 0; i--)
            {
                Vector2 newOldPos = Projectile.oldPos[i];
                if (newOldPos == oldPos) break;
                oldPos = newOldPos;
                max++;
            }
            
            if(max == 0) return false;
            
            oldPos = Projectile.oldPos[len-1];
            for (int i = len - 2; i > 0; i--)
            {
                Vector2 newOldPos = Projectile.oldPos[i];
                if (newOldPos == oldPos) break;
                float scale = (float)(len-i)/max;
                scale = (scale + 2) * 0.33f;

                DelegateMethods.f_1 = 1f;

                DelegateMethods.c_1 = LightningColour * 0.66f;
                Utils.DrawLaser(Main.spriteBatch, tex, oldPos - Main.screenPosition, newOldPos - Main.screenPosition, new Vector2(Projectile.scale) * 0.75f * scale, DelegateMethods.LightningLaserDraw);

                oldPos = newOldPos;
            }

            oldPos = Projectile.oldPos[len - 1];
            for (int i = len - 2; i > 0; i--)
            {
                Vector2 newOldPos = Projectile.oldPos[i];
                if (newOldPos == oldPos) break;
                float scale = (float)(len - i) / max;
                scale = (scale + 2) * 0.33f;

                DelegateMethods.f_1 = 1f;

                DelegateMethods.c_1 = Color.White;
                Utils.DrawLaser(Main.spriteBatch, tex, oldPos - Main.screenPosition, newOldPos - Main.screenPosition, new Vector2(Projectile.scale) * 0.33f * scale, DelegateMethods.LightningLaserDraw);

                oldPos = newOldPos;
            }
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);


        //Decomp from Terraria.Main.DrawProjDirect(..) for type 617 (ProjectileID.NebulaArcanum)
        
        Vector2 vector81 = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
        Texture2D value95 = TextureAssets.Projectile[ProjectileID.NebulaArcanum].Value;
        vector81 = new Vector2(Projectile.ai[1], Projectile.ai[2] - (HeightAboveGround * 16) - 8f) + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
        Color color87 = Color.White;
        Vector2 origin24 = new Vector2(value95.Width, value95.Height) / 2f;
        float num341 = Projectile.rotation;
        Vector2 vector82 = Vector2.One * Projectile.scale;
        Color color96 = color87 * 0.8f;
        color96.A /= 2;
        Color color97 = Color.Lerp(color87, Color.Black, 0.5f);
        color97.A = color87.A;
        float rotation = MathHelper.ToRadians((float)Main.timeForVisualEffects) * 1.5f;
        float num362 = 0.95f + (rotation * 0.75f).ToRotationVector2().Y * 0.1f;
        color97 *= num362;
        float scale12 = 0.6f + Projectile.scale * 0.6f * num362;
        SpriteEffects dir = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
        {
            dir = SpriteEffects.FlipHorizontally;
        }
        Main.EntitySpriteDraw(TextureAssets.Extra[50].Value, vector81, null, color97, 0f - rotation + 0.35f, origin24, scale12, dir ^ SpriteEffects.FlipHorizontally);
        Main.EntitySpriteDraw(TextureAssets.Extra[50].Value, vector81, null, color87, 0f - rotation, origin24, Projectile.scale, dir ^ SpriteEffects.FlipHorizontally);
        Main.EntitySpriteDraw(TextureAssets.Projectile[ProjectileID.NebulaArcanum].Value, vector81, null, color96, (0f - Projectile.rotation) * 0.7f, origin24, Projectile.scale, dir ^ SpriteEffects.FlipHorizontally);
        Main.EntitySpriteDraw(TextureAssets.Extra[50].Value, vector81, null, color87 * 0.8f, rotation * 0.5f, origin24, Projectile.scale * 0.9f, dir);

        Main.EntitySpriteDraw(value95, vector81, new Rectangle(0, 0, value95.Width, value95.Height), Color.Lerp(LightningColour, Color.White, 0.5f), rotation, origin24, Projectile.scale, dir);
        
        //=====

        return false;
    }

    public override bool CanHitPlayer(Player target)
    {
        return Projectile.velocity.Y > float.Epsilon;
    }
}