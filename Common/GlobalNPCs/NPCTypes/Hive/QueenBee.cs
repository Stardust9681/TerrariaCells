using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Common.GlobalNPCs;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Hive;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;
using TerrariaCells.Common.Utilities;

public class QueenBee : AIType
{
    public static Vector2 SpawnPosition = Vector2.Zero;
    //0: never made a wall
    //1: has made phase 2 wall
    //2: has made phase 3 wall
    public int WallPhase = 0;

    const int BazookaPullOutAnimTime = 60;
    const int BazookaHeldOutTime = 190;
    const int BazookaPutAwayAnimTime = 30;
    public override bool FindFrame(NPC npc, int frameHeight)
    {
        int animSpeed = 5;
        if (npc.ai[0] == 1 && npc.ai[3] >= 30 && npc.ai[1] % 3 == 2)
        {
            animSpeed = 8;
        }

        npc.frameCounter++;
        
        if (npc.ai[0] == 1 && npc.frame.Y >= 4)
        {
            npc.frame.Y = 0;
        }
        if (npc.ai[0] == 3 && npc.ai[2] > 0)
        {
            if (npc.ai[2] < BazookaPullOutAnimTime)
                npc.frame.Y = (int)MathHelper.Lerp(0, 15, (npc.ai[2]) / BazookaPullOutAnimTime);
            else if (npc.ai[2] < BazookaPullOutAnimTime + BazookaHeldOutTime)
            {
                npc.frameCounter++;
                if (npc.frameCounter >= 15)
                {
                    npc.frame.Y++;
                    if (npc.frame.Y < 13 || npc.frame.Y >= 15)
                    {
                        npc.frame.Y = 13;
                    }
                    npc.frameCounter = 0;
                }
            }
            else
                npc.frame.Y = (int)MathHelper.Lerp(15, 0, (npc.ai[2] - (BazookaPullOutAnimTime + BazookaHeldOutTime)) / BazookaPutAwayAnimTime);
        }
        else
        {
            if (npc.frameCounter >= animSpeed)
            {
                npc.frame.Y++;
                if (npc.frame.Y >= 8)
                {
                    npc.frame.Y = 0;
                }
                npc.frameCounter = 0;
            }
        }
            return false;
    }
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write7BitEncodedInt(WallPhase);
    }
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        WallPhase = binaryReader.Read7BitEncodedInt();
    }
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Asset<Texture2D> t = TextureAssets.Npc[npc.type];
        Asset<Texture2D> beezooka = ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/QueenBeeBazooka");
        Asset<Texture2D> wings = ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/QueenBeeWings");
        //qb has 12 frames. divide image height by 12. multiply by frame.Y (determines what frame its on.) add 4 if not charging (npc.ai[0] != 1) because idle frames are after charging frames
        Rectangle frame = new Rectangle(0, t.Height() / 12 * (npc.frame.Y + (npc.localAI[2] == 1 ? 0 : 4)), t.Width(), t.Height() / 12);

        //wings are a separate sprite during bazooka animation
        if (npc.ai[0] == 3 && npc.ai[2] > 0)
        {
            frame = new Rectangle(0, beezooka.Height() / 15 * npc.frame.Y, beezooka.Width(), beezooka.Height() / 15);
            Rectangle wingframe = new(0, wings.Height() / 4 * ((int)npc.localAI[2] / 5), wings.Width(), wings.Height() / 4);
            spriteBatch.Draw(wings.Value, npc.Center - Main.screenPosition - new Vector2(0, 30), wingframe, drawColor, npc.rotation, new Vector2(wings.Width(), wings.Height() / 4) / 2, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
            spriteBatch.Draw(beezooka.Value, npc.Center - Main.screenPosition - new Vector2(0, 30), frame, drawColor, npc.rotation, new Vector2(beezooka.Width(), beezooka.Height() / 15) / 2, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
        }
        else
        {
            //wobble when stunned
            float rotation = npc.rotation;
            if (npc.ai[0] == 1 && npc.ai[1] == 1)
            {
                rotation = TCellsUtils.LerpFloat(-10, 10, npc.ai[3], 40, TCellsUtils.LerpEasing.Bell, 0, false);
            }
            spriteBatch.Draw(t.Value, npc.Center - Main.screenPosition - new Vector2(0, 30), frame, drawColor, MathHelper.ToRadians(rotation), new Vector2(t.Width(), t.Height() / 12) / 2, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
        }
        return false;
    }

    public override void Behaviour(NPC npc)
    {
        Trailing(npc);
        Targetting(npc);
        if (!npc.HasValidTarget)
        {
            return;
        }
        //SpawnPosition = Vector2.Zero;
        if (SpawnPosition == Vector2.Zero)
        {
            SpawnPosition = Main.player[npc.target].Center;
        }
        if (npc.ai[0] == 0)
        {
            Stingers(npc);
        }
        if (npc.ai[0] == 1)
        {
            Dash(npc);
        }
        if (npc.ai[0] == 2)
        {
            MakeWall(npc);
        }
        if (npc.ai[0] == 3)
        {
            Beezooka(npc);
        }
    }
    public void Beezooka(NPC npc)
    {
        
        npc.ai[3]++;
        Player target = Main.player[npc.target];
        npc.spriteDirection = target.Center.X > npc.Center.X ? -1 : 1;

        Vector2 spawnoffset =  new Vector2(-25 * npc.spriteDirection, -40);

        Vector2 targetPos = SpawnPosition;
        if (npc.ai[1] == 0)
        {
            targetPos = new Vector2(SpawnPosition.X + 140 * (target.Center.X > npc.Center.X ? -1 : 1), SpawnPosition.Y - 160);
        }else if (npc.ai[1] != 0)
        {
            targetPos = new Vector2(SpawnPosition.X + 140 * npc.ai[1], SpawnPosition.Y - 160);
        }
        if (npc.ai[1] == 0 && npc.ai[3] >= 140)
        {
            npc.ai[1] = target.Center.X > npc.Center.X ? 1 : -1;
        }

        
        if ((npc.ai[1] != 0 && npc.ai[3] == 240) || (npc.ai[1] == 0 && npc.ai[3] == 100))
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item42, npc.Center);
                Vector2 pos = target.Center + new Vector2(Main.rand.NextFloat(1, 20)).RotatedByRandom(MathHelper.Pi);
                float angle = (npc.Center + spawnoffset).AngleTo(pos);
                for (int i = 0; i < 15; i++)
                {
                    Vector2 speed = angle.ToRotationVector2().RotatedByRandom(MathHelper.ToRadians(10)) * Main.rand.NextFloat(1, 25);
                    Dust.NewDustDirect(npc.Center + spawnoffset, 0, 0, DustID.Torch, speed.X, speed.Y);
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 pos = target.Center + new Vector2(Main.rand.NextFloat(1, 20)).RotatedByRandom(MathHelper.Pi);
                float angle = (npc.Center + spawnoffset).AngleTo(pos);

                Vector2 vel = angle.ToRotationVector2() * 10;

                Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center + spawnoffset, vel, ModContent.ProjectileType<BeeMissile>(), 20, 1, -1, pos.X, pos.Y);
            }
        }
        if (npc.ai[1] != 0 && npc.ai[3] == 280)
        {
            npc.ai[3] = 0;
            npc.ai[2] = 0;
            ChooseNewAttack(npc);
        }

        npc.localAI[2]++;
        if (npc.localAI[2] >= 20)
        {
            npc.localAI[2] = 0;
        }
        npc.ai[2]++;
        //slow down when close to target pos
        npc.velocity = Vector2.Lerp(npc.velocity, npc.AngleTo(targetPos).ToRotationVector2()* Math.Min(npc.Distance(targetPos)/5, 20), 0.05f);
        
    }
    public void ChooseNewAttack(NPC npc)
    {
        //no idea why this variable is saving between instances of queen bee
        if (WallPhase > 0 && npc.GetLifePercent() > 0.7f)
        {
            WallPhase = 0;
        }

        //if stingers go into dash
        if (npc.ai[0] == 0)
        {
            npc.ai[0] = 1;
            

        }
        //if dash or wall go into singers
        else if (npc.ai[0] == 1 || npc.ai[0] == 2)
        {
            npc.ai[0] = 0;
            //go into bazooka if below 60%
            if (npc.GetLifePercent() < 0.6f)
            {
                npc.ai[0] = 3;
            }
        }
        //go into stingers if bazooka
        else if (npc.ai[0] == 3)
        {
            npc.ai[0] = 0;
        }
        //go into wall if below threshold and hasnt done that wall yet
        if ((WallPhase == 0 && npc.GetLifePercent() <= 0.7f) || (WallPhase == 1 && npc.GetLifePercent() <= 0.3f))
        {
            npc.ai[0] = 2;
        }
        npc.netUpdate = true;
    }
    public void MakeWall(NPC npc)
    {
        
        Player target = Main.player[npc.target];
        Vector2 targetPos = target.Center;
        npc.ai[3]++;
        
        if (WallPhase == 0)
        {
            targetPos = SpawnPosition - new Vector2(220, 200);

        }
        if (WallPhase == 1)
        {
            targetPos = SpawnPosition - new Vector2(-220, 200);

        }
        if (npc.ai[2] == 0)
        {
            npc.velocity = Vector2.Lerp(npc.velocity, npc.AngleTo(targetPos).ToRotationVector2() * 10, 0.05f);
            if (npc.Distance(targetPos) <= 60 || npc.ai[3] >= 120)
            {
                npc.ai[3] = 0;
                npc.ai[2]++;
            }
        }
        if (npc.ai[2] == 1)
        {
            npc.velocity *= 0.91f;
            if (npc.ai[3] >= 20 && npc.ai[3] % 3 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item17 with { MaxInstances = 3 }, npc.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + new Vector2(-20 * npc.spriteDirection + Main.rand.NextFloat(-10, 10), 40), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(5, 8)), ModContent.ProjectileType<BeeWallBee>(), 20, 1);
            }
            if (npc.ai[3] >= 120)
            {
                npc.ai[3] = 0;
                npc.ai[2] = 0;
                WallPhase++;
                ChooseNewAttack(npc);
            }
        }
    }
    public void Stingers(NPC npc)
    {
        Player target = Main.player[npc.target];
        Vector2 targetPos = target.Center with { Y = SpawnPosition.Y - 200};
        if (npc.ai[1] % 2 == 0) {
            int delay = 0;
            if (npc.ai[1] == 0) delay = 40;
            npc.ai[3]++;
            npc.velocity = Vector2.Lerp(npc.velocity, npc.AngleTo(targetPos).ToRotationVector2() * 15, 0.08f);
            if ((npc.Center.X > targetPos.X ? 1 : -1) != npc.ai[2] && npc.ai[2] != 0 && npc.ai[3] >= delay)
            {
                npc.ai[1]++;
                npc.ai[3] = 0;
            }
            npc.ai[2] = npc.Center.X > targetPos.X ? 1 : -1;
        }
        else
        {
            npc.velocity *= 0.9f;
            npc.ai[2]++;
            
            if (npc.ai[2] > 5  && npc.ai[2] < 30 && npc.ai[2] % 7 == 0)
            {
                Vector2 pos = npc.Center + new Vector2(-15 * npc.spriteDirection, 20);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectileDirect(npc.GetSource_FromAI(), pos, pos.AngleTo(target.Center).ToRotationVector2() * 14, ProjectileID.QueenBeeStinger, 20, 1);
                npc.velocity -= npc.AngleTo(target.Center).ToRotationVector2()*3;
                SoundEngine.PlaySound(SoundID.Item17, npc.Center);
            }
            if (npc.ai[2] > 40 )
            {
                npc.ai[2] = 0;
                npc.ai[1]++;
                if (npc.ai[1] >= 6)
                {
                    ChooseNewAttack(npc);
                    npc.ai[1] = 0;
                    npc.ai[2] = 0;
                    npc.ai[3] = 0;
                    
                }
            }
        }
    }
    public void Dash(NPC npc)
    {
        Player target = Main.player[npc.target];

        int numDashes = 2;
        float dashRepositionSpeed = 15;
        if (npc.GetLifePercent() < 0.7f)
        {
            numDashes = 3;
            dashRepositionSpeed = 20;
        }if (npc.GetLifePercent() < 0.3f)
        {
            numDashes = 4;
            dashRepositionSpeed = 30;
        }
        
        if (npc.ai[1] == 0 && npc.ai[3] == 0)
        {
            npc.ai[1] = numDashes * 3;
            npc.ai[2] = npc.Center.Y > target.Center.Y - 15 ? 1 : -1;
        }
        
        //dash itself
        if (npc.ai[1] % 3 == 2)
        {
            npc.GetGlobalNPC<CombatNPC>().allowContactDamage = true;
            if (npc.ai[3] == 0)
            {
                npc.velocity = new Vector2(target.Center.X > npc.Center.X ? 22 : -22, 0);
            }
            npc.ai[3]++;
            npc.localAI[1] = 1;
            npc.localAI[2] = 1;
            if (npc.ai[3] >= 60 || (npc.Center.X < SpawnPosition.X - 433 && npc.velocity.X < 0) || (npc.Center.X > SpawnPosition.X + 433 && npc.velocity.X > 0))
            {
                npc.localAI[1] = 0;
                npc.ai[3] = 0;
                npc.ai[2] = npc.Center.X > target.Center.X ? -1 : 1; ;
                npc.ai[1]--;
                npc.localAI[2] = 0;
            }
        }
        //stun
        else if (npc.ai[1] % 3 == 1)
        {

            if (npc.ai[3] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item14, npc.Center);

                int recoil = 8;
                if (npc.ai[1] == 1)
                {
                    recoil = 20;
                    SoundEngine.PlaySound(SoundID.Item150, npc.Center);
                }

                npc.velocity = new Vector2(npc.velocity.X > 0 ? -recoil : recoil, Main.rand.NextFloat(-3, 3));
                PunchCameraModifier modifier = new PunchCameraModifier(npc.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 10, 1000f, npc.FullName);
                Main.instance.CameraModifiers.Add(modifier);
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Gore.NewGoreDirect(npc.GetSource_FromAI(), npc.velocity.X > 0 ? npc.Left : npc.Right, new Vector2(0, Main.rand.NextFloat(5, 10) * (Main.rand.NextBool() ? 1 : -1)).RotatedByRandom(MathHelper.Pi / 12), Main.rand.Next([GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3]));
                    }
                }
            }
            npc.ai[3]++;
            npc.velocity *= 0.94f;
            npc.spriteDirection = (int)npc.ai[2];
            
            
            //stun longer if last dash
            if (npc.ai[3] >= (npc.ai[1] == 1 ? 160 : 30))
            {
                npc.ai[3] = 0;
                npc.ai[1]--;
                
                npc.ai[2] = npc.Center.Y > target.Center.Y - 15 ? 1 : -1;
                if (npc.ai[1] == 0)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath32 with { Volume = 0.2f}, npc.Center);
                    npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
                    npc.localAI[1] = 0;
                    npc.ai[3] = 0;
                    npc.ai[2] = 0;
                    npc.ai[1] = 0;
                    ChooseNewAttack(npc);
                    npc.localAI[2] = 0;
                }
            }
        }
        //reposition
        else if (npc.ai[1] % 3 == 0)
        {
            npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
            int dir = npc.Center.X > target.Center.X ? 1 : -1;
            Vector2 targetPos = target.Center + new Vector2(350 * dir, 0);
            npc.velocity = Vector2.Lerp(npc.velocity, npc.AngleTo(targetPos).ToRotationVector2() * dashRepositionSpeed, 0.05f);
            npc.ai[3]++;
            if ((npc.ai[3] >= 60 || ((npc.Center.Y > targetPos.Y - 15 ? 1 : -1) != npc.ai[2] && npc.ai[2] != 0)))
            {
                npc.ai[3] = 0;
                npc.ai[2] = 0;
                npc.ai[1]--;
                SoundEngine.PlaySound(SoundID.Zombie125, npc.Center);
            }
        }
        
    }
    public void Targetting(NPC npc)
    {
        if (!npc.HasValidTarget)
        {
            npc.TargetClosest();
        }
        if (!npc.HasValidTarget)
        {
            npc.velocity.Y += 1;
            npc.EncourageDespawn(30);
        }
        if (npc.HasValidTarget)
        {
            npc.spriteDirection = npc.Center.X > Main.player[npc.target].Center.X ? 1 : -1;
            if (npc.localAI[2] == 1) npc.spriteDirection = npc.velocity.X > 0 ? -1 : 1;
        }
    }
    public void Trailing(NPC npc)
    {
    }
    
    public override bool AppliesToNPC(int npcType)
    {
        return npcType == NPCID.QueenBee;
    }
}
public class ExtraQueenBee : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.QueenBee;
    }
    public override void SetDefaults(NPC entity)
    {
        base.SetDefaults(entity);
    }
    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailCacheLength[NPCID.QueenBee] = 10;
        NPCID.Sets.TrailingMode[NPCID.QueenBee] = 2;
    }
    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        return npc.localAI[2] == 1;
    }
}