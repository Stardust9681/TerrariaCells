using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Forest;
public class RavenAI : GlobalNPC, OnAnyPlayerHit.IGlobal, PreFindFrame.IGlobal
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Raven;

    private const int Idle = 0;
    private const int Track = 1;
    private const int Swoop = 2;
    public override bool PreAI(NPC npc)
    {
        int oldAI1 = (int)npc.ai[1];
        switch(oldAI1)
        {
            case Idle:
                IdleAI(npc);
                break;
            case Track:
                TrackAI(npc);
                break;
            case Swoop:
                SwoopAI(npc);
                break;
        }
        if(oldAI1 != (int)npc.ai[1])
            npc.netUpdate = true;
        return false;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        const int TilesToCheck = 16;
        const int AdjustmentPerStep = 3;
        for(int i = 0; i < TilesToCheck * 16 / AdjustmentPerStep; i++)
        {
            if(Collision.IsWorldPointSolid(npc.Bottom + new Vector2(0, AdjustmentPerStep)))
                break;
            npc.position.Y += AdjustmentPerStep;
        }
    }
    
    private void IdleAI(NPC npc)
    {
        npc.noTileCollide = false;
    
        npc.TargetClosest(false);
        Vector2 npcCenter = npc.Center;
        const float ScareDistance = 8 * 16; //8 tiles
        const float ScareDistanceSQ = ScareDistance * ScareDistance;

        if (npc.TryGetTarget(out Entity target))
        {
            if (Vector2.DistanceSquared(npcCenter, target.Center) < ScareDistanceSQ)
            {
                npc.ai[0] = 0f;
                npc.ai[1] = Track;
                return;
            }
        }

        foreach(Projectile projectile in Main.ActiveProjectiles)
        {
            if(Vector2.DistanceSquared(npcCenter, projectile.Center) < ScareDistanceSQ)
            {
                npc.ai[0] = 0f;
                npc.ai[1] = Track;
                npc.ai[2] = 0f;
                return;
            }
        }
    }
    
    const float TrackingDistance_Horizontal = 16;
    const float TrackingDistance_Vertical = 9;
    private void TrackAI(NPC npc)
    {
        CombatNPC.ToggleContactDamage(npc, false);
        npc.noTileCollide = true;
        npc.velocity.X *= 0.99f;
        npc.rotation = MathHelper.ToRadians(npc.velocity.X * 5f);
        npc.direction = MathF.Sign(npc.velocity.X);

        npc.TargetClosest(true);
        if(npc.TryGetTarget(out Entity target))
        {
            Vector2 targetOffset = new Vector2(TrackingDistance_Horizontal * MathF.Sign(npc.position.X - target.position.X), -TrackingDistance_Vertical) * 16;
            Vector2 targetPosition = target.Center + targetOffset;

            if (npc.ai[2] <= 0)
            {
                if(npc.position.Y > targetPosition.Y)
                {
                    npc.velocity.Y = -4.5f;
                    float speed = MathF.Abs(npc.position.X - targetPosition.X) / 16f;
                    speed = MathHelper.Clamp(speed, 2, 8);
                    npc.velocity.X = MathHelper.Lerp(npc.velocity.X, MathF.Sign(targetPosition.X - npc.position.X) * speed, 0.8f);
                    npc.ai[2] = 24f;
                }
                else
                {
                    npc.GravityMultiplier *= 0.66f;
                }
            }
            else
            {
                npc.ai[2]--;
                npc.GravityMultiplier *= 0.5f;
            }

            if((MathF.Abs(npc.Center.X - targetPosition.X)*3f) + (MathF.Abs(npc.Center.Y - targetPosition.Y)*0.33f) < 6 * 16)
            {
                npc.ai[0]++;
                
                if(npc.ai[0] > 40)
                {
                    npc.DoAttackWarning();
                    npc.ai[0] = 0;
                    npc.ai[1] = Swoop;
                    npc.ai[2] = MathF.Sign(target.Center.X - npc.Center.X);
                    return;
                }
            }
            else if(npc.ai[0] > 20)
            {
                npc.ai[0]--;
            }
        }
    }
    
    private void SwoopAI(NPC npc)
    {
        CombatNPC.ToggleContactDamage(npc, true);
        npc.direction = MathF.Sign(npc.velocity.X);
        npc.GravityMultiplier *= 0.1f;
        bool foundTarget = npc.TryGetTarget(out Entity target);
        npc.rotation = npc.direction * MathHelper.ToRadians(npc.velocity.Y * 10f);
        if(!foundTarget)
            target = npc;
        if (MathF.Sign(target.Center.X - npc.Center.X) != npc.ai[2] || !foundTarget)
        {
            npc.ai[0]++;
        }
        
        if(npc.ai[0] < 25)
        {
            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.ai[2] * 5f, 0.05f);
        }
        else
        {
            npc.velocity.X *= 0.98f;
        }
        float lerpAmount = (1 + (npc.position.X - target.position.X) * npc.ai[2] / (TrackingDistance_Horizontal * 16)) * 0.5f;
        if(npc.ai[0] < 10)
            lerpAmount = MathHelper.Clamp(lerpAmount, 0, 1);
        else
            lerpAmount = MathHelper.Clamp(lerpAmount, 0, 0.8f);
        float targetYVelocity = 6f * MathHelper.Lerp(1, -1, lerpAmount);
        npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, targetYVelocity, 0.15f);
        if(npc.position.Y >= target.position.Y + 48 && npc.velocity.Y > 0)
            npc.velocity.Y *= 0.75f;
        
        if(npc.ai[0] > 40)
        {
            npc.ai[0] = 0;
            npc.ai[1] = Track;
            npc.ai[2] = 0;
            npc.ai[3] = 0; //Set on hit during this step
            return;
        }
    }

    public void OnAnyPlayerHit(NPC npc, Player attacker, NPC.HitInfo hit, int damage)
    {
        if(hit.DamageType.CountsAsClass(DamageClass.Melee))
        {
            switch((int)npc.ai[1])
            {
                case Track:
                    npc.ai[0] = MathHelper.Max(npc.ai[0]-5, 20);
                    break;
                case Swoop:
                    npc.ai[3]++;
                    if(npc.ai[3] > 2)
                    {
                        npc.velocity *= 1.4f;
                        npc.ai[0] = 0;
                        npc.ai[1] = Track;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                    }
                    break;
            }
        }
    }

    public bool PreFindFrame(NPC npc, int frameHeight)
    {
        npc.spriteDirection = -npc.direction;
        int frameNum = npc.frame.Y / frameHeight;
        switch((int)npc.ai[1])
        {
            case Idle:
                frameNum = 0;
                break;
                
            case Track:
                if(frameNum == 0) frameNum = 1;
                if(npc.oldVelocity.Y > 0 && npc.velocity.Y < 0)
                    frameNum = frameNum != 1 ? 3 : 2;
                npc.frameCounter++;
                if(npc.frameCounter > 8)
                {
                    npc.frameCounter = 0;
                    frameNum++;
                    if(frameNum > Main.npcFrameCount[npc.type] - 1)
                        frameNum = 1;
                }
                break;
            
            case Swoop:
                npc.frameCounter++;
                if (npc.frameCounter > 6)
                {
                    npc.frameCounter = 0;
                    frameNum++;
                    if (frameNum > Main.npcFrameCount[npc.type] - 1)
                        frameNum = 1;
                }
                break;
        }
        npc.frame.Y = frameNum * frameHeight;
        return false;
    }

    public override bool CheckActive(NPC npc)
    {
        npc.oldVelocity = npc.velocity;
        return base.CheckActive(npc);
    }
}