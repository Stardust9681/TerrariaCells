using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;

namespace TerrariaCells.Common.GlobalProjectiles;

public class BoomerangAI : GlobalProjectile
{
    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        return entity.aiStyle == ProjAIStyleID.Boomerang;
    }

    public override void SetDefaults(Projectile entity)
    {
        entity.usesLocalNPCImmunity = true;
        entity.localNPCHitCooldown = 10;
        entity.penetrate = -1;
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        //Set as invalid target
        projectile.ai[1] = -1;
        
        if(source is IEntitySource_WithStatsFromItem { Item : Item item })
        {
            //Set "rebound" limit to spawning item level +1
            projectile.ai[2] = 1 + item.GetGlobalItem<TierSystemGlobalItem>().itemLevel;
        }
        
        projectile.netUpdate = true;
    }

    public override bool PreAI(Projectile projectile)
    {
        //Prevent some vanilla shenanigans
        if (projectile.ai[1] > 0) projectile.ai[0] = 0;
        return base.PreAI(projectile);
    }

    private const float TIMEOUT = 30f;

    public override void PostAI(Projectile projectile)
    {
        //"Undo" vanilla behaviour to substitute our own
        projectile.ai[1]--;
        
        //Gently nudge towards target
        if(projectile.ai[1] >= 0)
        {
            float len = projectile.velocity.Length();
            Vector2 targetVelocity = projectile.DirectionTo(Main.npc[(int)projectile.ai[1]].Center) * len;
            projectile.velocity = Vector2.Lerp(projectile.velocity, targetVelocity, 0.0085f);
        }
        
        //Timer + timeout for return
        projectile.localAI[2]++;
        if(projectile.localAI[2] > TIMEOUT)
        {
            projectile.ai[0] = 1f;
            projectile.ai[1] = 0f;
            projectile.ai[2] = 0f;
            projectile.netUpdate = true;
        }
    }

    //Force timeout if projectile hits a tile
    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
    {
        projectile.ai[0] = 1f;
        projectile.ai[1] = 0f;
        projectile.ai[2] = 0f;
        projectile.netUpdate = true;
        return base.OnTileCollide(projectile, oldVelocity);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        //Rebound limit 
        projectile.ai[2]--;
        
        //Reset timeout timer
        projectile.localAI[2] = 0;
    
        //If still has rebounds
        if(projectile.ai[2] > 0)
        {
            //Aim towards next available target
            const float SPEED_MULT = 1.1f;
            float speed = projectile.velocity.Length();
            projectile.ai[1] = projectile.FindTargetWithLineOfSight(speed * TIMEOUT * SPEED_MULT);
            if (projectile.ai[1] != -1)
            {
                projectile.velocity = projectile.DirectionTo(Main.npc[(int)projectile.ai[1]].Center) * speed * SPEED_MULT;
                projectile.direction = MathF.Sign(projectile.velocity.X);
                projectile.ai[0] = 0;
            }
        }
        else
        {
            //Return to owner
            projectile.ai[0] = 1f;
            projectile.ai[1] = 0f;
            projectile.ai[2] = 0f;
        }
        
        projectile.netUpdate = true;
    }

    // Only hit "target" NPC ?
    // public override bool? CanHitNPC(Projectile projectile, NPC target)
    // {
    //     if(projectile.ai[1] < 0) return base.CanHitNPC(projectile, target);
    //     return target.whoAmI == (int)projectile.ai[1] ? base.CanHitNPC(projectile, target) : false;
    // }
}