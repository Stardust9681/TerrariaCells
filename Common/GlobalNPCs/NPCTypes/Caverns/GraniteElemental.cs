using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Caverns;

public class GraniteElemental : GlobalNPC, PreHitEffect.IGlobal, PreFindFrame.IGlobal, OnAnyPlayerHit.IGlobal
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.GraniteFlyer;
    public override void SetDefaults(NPC entity)
    {
        entity.noGravity = true;
    }


    const int Passive = 0;
    const int WakeUp = 1;
    const int Move = 2;
    const int FireTheLaser_Kronk = 3;
    const int Recharge = 4;
    const int Explode = 5;

    public override bool PreAI(NPC npc)
    {
        float ai1_old = npc.ai[1];
    
        switch((int)npc.ai[1])
        {
            case Passive:
                PassiveAI(npc);
                break;
            case WakeUp:
                WakeUpAI(npc);
                break;
            case Move:
                MoveAI(npc);
                break;
            case FireTheLaser_Kronk:
                LaserAI(npc);
                break;
            case Recharge:
                RechargeAI(npc);
                break;
            case Explode:
                ExplodeAI(npc);
                break;
        }
    
        if(npc.ai[1] != ai1_old)
            npc.netUpdate = true;
        
        return false;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        CombatNPC.ToggleContactDamage(npc, false);
    }
    
    private void PassiveAI(NPC npc)
    {
        npc.dontTakeDamage = true;
        npc.TargetClosest(false);
        
        npc.velocity.Y += 0.14f;
        npc.rotation += npc.velocity.X * 0.1f;
        
        if (npc.TargetInAggroRange(10*16))
        {        
            npc.ai[1] = WakeUp;
            npc.dontTakeDamage = false;
            return;
        }
    }
    private void WakeUpAI(NPC npc)
    {
        npc.dontTakeDamage = true;

        Vector2 ground = npc.FindGroundInFront();
        float targetY = ground.Y - (3*16);
        if(npc.position.Y > targetY)
            npc.velocity.Y -= 0.02f;
        else
            npc.velocity.Y += 0.02f;
            
        if(Main.rand.NextFloat() < 0.33f)
        {
            float spawnDistance = Main.rand.NextFloat(1.5f * 16, 5 * 16);
            Vector2 dustPosition = npc.Center + (Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * spawnDistance);
            Vector2 dustVelocity = (npc.Center - dustPosition).SafeNormalize(Vector2.Zero) * (spawnDistance / (1.5f*16));
            
            Dust d = Dust.NewDustDirect(dustPosition, 1, 1, DustID.Electric);
            d.noGravity = true;
            d.velocity = dustVelocity;
        }
        
        if(npc.ai[0] < 90)
            npc.rotation += MathHelper.ToRadians(npc.ai[0] * 0.25f);
        else
            npc.rotation = Utils.AngleLerp(npc.rotation, 0, 0.02f);

        npc.ai[0]++;
        if (npc.ai[0] > 135f)
        {
            npc.rotation = 0;
        
            npc.ai[0] = 0;
            npc.ai[1] = Move;
            return;
        }
    }
    
    private void MoveAI(NPC npc)
    {
        npc.dontTakeDamage = false;
        npc.TargetClosest();
        
        if(!npc.TryGetTarget(out Entity target))
        {
            npc.velocity.X += Main.rand.NextFloat(-2f, 2f);
            
            npc.ai[0] = 0;
            npc.ai[1] = Passive;
            return;
        }
        
        if(npc.Center.DistanceSQ(target.Center) > MathF.Pow(24*16, 2))
        {
            npc.noTileCollide = true;
            npc.ai[0] = MathF.Max(0, npc.ai[0]-2);
            npc.netUpdate = true;
        }
        if(npc.noTileCollide)
        {
            if(npc.LineOfSight(target.Center))
            { 
                npc.noTileCollide = false;
                npc.netUpdate = true;
            }
        }
        
        npc.rotation = npc.velocity.X * 0.2f;
        
        Vector2 targetPosition = target.Center + ((npc.Center - target.Center).SafeNormalize(Vector2.Zero) * 8 * 16);
        if(MathF.Abs(npc.FindGroundInFront().Y - npc.Center.Y) < 4 * 16)
            targetPosition.Y -= 4 * 16;
        //Dust.NewDustDirect(targetPosition, 1, 1, DustID.GemDiamond).velocity = Vector2.Zero;
        
        if(npc.position.X < targetPosition.X)
            npc.velocity.X += 0.06f;
        else
            npc.velocity.X -= 0.06f;
            
        if(npc.position.Y < targetPosition.Y)
            npc.velocity.Y += 0.04f;
        else
            npc.velocity.Y -= 0.04f;
            
        float appxDist = MathF.Abs(npc.position.X - targetPosition.X) + MathF.Abs(npc.position.Y - targetPosition.Y);
        if (appxDist < (1.5f*16) || npc.ai[0] > 150)
        {
            npc.velocity *= 0.5f;
            npc.ai[0] = 0;
            npc.ai[1] = FireTheLaser_Kronk;
            npc.ai[2] = -1;
            return;
        }
        else if(appxDist < (3*16))
        {
            npc.velocity *= 0.95f;
        }
            
        npc.ai[0]++;
    }
    
    private void LaserAI(NPC npc)
    {
        npc.dontTakeDamage = false;
        npc.velocity *= 0.9f;
        npc.rotation = Utils.AngleLerp(npc.rotation, 0, 0.05f);
        
        if(!npc.TryGetTarget(out Entity target))
        {
            npc.TargetClosest();
            if(!npc.TryGetTarget(out target))
            {
                npc.velocity.X += Main.rand.NextFloat(-2f, 2f);
                
                npc.ai[0] = 0;
                npc.ai[1] = Passive;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }
        }


        const int MaxAngleDEG = 20;
        float MaxAngleRAD = MathHelper.ToRadians(MaxAngleDEG);
        const float AngleAdjustment = 1.1f;
        if (npc.ai[0] == 0)
        {
            npc.DoAttackWarning();
        
            npc.ai[2] = (target.Center - npc.Center).ToRotation();
            npc.ai[3] = Main.rand.NextDirection();
            if(npc.ai[3] != MathF.Sign(target.position.X - npc.position.X))
                npc.ai[3] = Main.rand.NextDirection(); //Prefer swiping "down" on top of player
            float rotationStartSweep = npc.ai[2] - (npc.ai[3] * MaxAngleRAD * AngleAdjustment);
            float rotationEndSweep = npc.ai[2] + (npc.ai[3] * MaxAngleRAD / AngleAdjustment);
            Vector2 spawnPos = Vector2.UnitX.RotatedBy(rotationStartSweep);
            Vector2 targetPos = Vector2.UnitX.RotatedBy(rotationEndSweep);
            for (int i = 0; i < 8; i++)
            {
                float mult = 12f * (i+1);
                Dust d = Dust.NewDustDirect(npc.Center + (spawnPos * mult), 1, 1, DustID.Electric);
                d.noGravity = true;
                d.velocity = (targetPos - spawnPos) * (i+1);
            }
        }
        
        if(npc.ai[0] == 30)
        {
            Vector2 velocity = Vector2.UnitX.RotatedBy(npc.ai[2] - (npc.ai[3] * MaxAngleRAD * AngleAdjustment)).SafeNormalize(Vector2.Zero);
            for (int i = 0; i < 5 + Main.rand.Next(5); i++)
            {
                Dust d = Dust.NewDustDirect(npc.Center, 1, 1, DustID.Electric, Scale:0.8f);
                d.noGravity = true;
                d.velocity = velocity.RotatedByRandom(MaxAngleRAD) * Main.rand.NextFloat(1f, 3f);
            }
            //Main.NewText($"r:{npc.ai[2] - (npc.ai[3] * MathHelper.ToRadians(25))} rT:{npc.ai[2] + (npc.ai[3] * MathHelper.ToRadians(25))}");
            int index = Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                velocity,
                ModContent.ProjectileType<Content.Projectiles.GeodeBlast>(),
                (int)(npc.damage * .75f),
                1f,
                Main.myPlayer,
                ai0:npc.whoAmI,
                ai1: npc.ai[2] + (npc.ai[3] * MaxAngleRAD / AngleAdjustment));
                
            npc.ai[2] = index;
            npc.ai[3] = 0;
            npc.netUpdate = true;
        }
        
        if(npc.ai[0] > 105)
        {
            Main.projectile[(int)npc.ai[2]].Kill();
            npc.ai[0] = 0;
            npc.ai[1] = Recharge;
            npc.ai[2] = 0;
            npc.ai[3] = 0;
            return;
        }

        npc.ai[0]++;
    }
    
    private void RechargeAI(NPC npc)
    {
        npc.dontTakeDamage = false;
        npc.velocity.X *= 0.95f;
        npc.velocity.Y = MathF.Sin(npc.ai[0] * MathHelper.Pi * 0.033f) * 0.9f;
        
        // if(Main.rand.NextBool(14))
        // {
        //     Dust.NewDust(npc.Center + Main.rand.NextVector2Circular(4 * 16, 4 * 16), 1, 1, DustID.Electric);
        // }
        
        npc.ai[0]++;
        
        if(npc.ai[0] > 120)
        {
            npc.ai[0] = 0;
            npc.ai[1] = Move;
            return;
        }
    }
    
    private void ExplodeAI(NPC npc)
    {
        npc.velocity.Y += 0.14f;
        npc.rotation += npc.velocity.X * 0.1f;
        
        npc.ai[0]++;

        Vector2 spawnPos = Main.rand.NextVector2Circular(4 * 16, 4 * 16);
        Vector2 vel = -spawnPos * 0.1f;
        spawnPos += npc.Center;

        Dust d = Dust.NewDustDirect(spawnPos, 1, 1, DustID.Electric);
        d.noGravity = true;
        d.velocity = vel;

        if (npc.ai[0] == 60)
            npc.DoAttackWarning();
        else if(npc.ai[0] > 80 && Main.netMode != 1)
        {
            npc.StrikeInstantKill();
        }
    }

    public bool PreHitEffect(NPC npc, int hitDirection, double damageDealt, bool instantKill)
    {
        if(npc.ai[1] == Explode) return true;
        return false;
    }
    public override bool CheckDead(NPC npc)
    {
        if(npc.ai[1] != Explode)
        {
            if(npc.ai[1] == FireTheLaser_Kronk && npc.ai[2] > -1 && Main.projectile[(int)npc.ai[2]].type == ModContent.ProjectileType<Content.Projectiles.GeodeBlast>())
            {
                Main.projectile[(int)npc.ai[2]].Kill();
            }
            
            npc.ai[0] = 0;
            npc.ai[1] = Explode;
            npc.ai[2] = 0;
            npc.ai[3] = 0;

            npc.velocity.X += Main.rand.NextFloat(-2f, 2f);

            npc.dontTakeDamage = true;
            npc.life = 1;
            
            npc.netUpdate = true;
            return false;
        }
        return base.CheckDead(npc);
    }

    public override void OnKill(NPC npc)
    {
        Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GraniteElementalExplosion>(), (int)(npc.damage * 1.33f), 1f, Main.myPlayer);
    }

    public bool PreFindFrame(NPC npc, int frameHeight)
    {
        int frameNum = npc.frame.Y / frameHeight;
        switch((int)npc.ai[1])
        {
            case Passive:
                frameNum = 15;
                break;
            case WakeUp:
                //timer 0-90
                //15 -> 16 -> 17 -> 18 -> 12
                frameNum = 15 + (int)((npc.ai[0]+2) / 22);
                if(frameNum > 18)
                    frameNum = 12;
                break;
            case Explode:
                //frames 16-21
                npc.frameCounter++;
                if(npc.frameCounter > 8)
                {
                    npc.frameCounter=0;
                    frameNum++;
                }
                if(frameNum < 16 || frameNum > 21) frameNum = 16;
                break;
            default:
                int invFramerate = 2;
                if(npc.ai[1] == Recharge) invFramerate *= 2;
                npc.frameCounter++;
                if(npc.frameCounter > invFramerate)
                {
                    npc.frameCounter = 0;
                    frameNum++;
                }
                if(frameNum < 0 || frameNum > 12) frameNum = 0;
                break;
        }
        npc.frame.Y = frameNum * frameHeight;
        return false;
    }

    public void OnAnyPlayerHit(NPC npc, Player attacker, NPC.HitInfo hit, int damage)
    {
        if(hit.DamageType.CountsAsClass(DamageClass.Melee))
        {
            switch((int)npc.ai[1])
            {
                case FireTheLaser_Kronk:
                    if(npc.ai[0] < 25)
                        npc.ai[0] -= 4;
                    break;
                case Recharge:
                    npc.ai[0] -= 3;
                    break;
            }
        }
    }
}

public class GraniteElementalExplosion : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1;
        Projectile.tileCollide = false;
        Projectile.width = 128;
        Projectile.height = 128;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 projCenter = projHitbox.Center();
        Vector2[] arr = new Vector2[] {targetHitbox.TopLeft(), targetHitbox.TopRight(), targetHitbox.BottomLeft(), targetHitbox.BottomRight(), targetHitbox.Center()};
        return arr.Any(v => projCenter.DistanceSQ(v) < 4 * 16 * 4 * 16);
    }
    
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 spawnPos = Main.rand.NextVector2Circular(4 * 16, 4 * 16);
            Vector2 vel = spawnPos * 0.2f;
            spawnPos += Projectile.Center;

            Dust d = Dust.NewDustDirect(spawnPos, 1, 1, DustID.Electric);
            d.noGravity = true;
            d.velocity = vel;
        }
    }
}